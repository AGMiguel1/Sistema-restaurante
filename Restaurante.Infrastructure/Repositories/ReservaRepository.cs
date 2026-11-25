using Microsoft.EntityFrameworkCore;
using Restaurante.Core.DTOs;
using Restaurante.Core.Entities;
using Restaurante.Core.Interfaces;
using Restaurante.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Restaurante.Infrastructure.Repositories
{
    public class ReservaRepository : BaseRepository<Reserva>, IReservaRepository
    {
        private readonly DapperContext _dapper;

        public ReservaRepository(RestauranteContext context) : this(context, new DapperContext(context))
        {
        }

        public ReservaRepository(RestauranteContext context, DapperContext dapper) : base(context)
        {
            _dapper = dapper;
        }

        public Task<bool> TieneRegistrosAsociadosAsync(int reservaId, CancellationToken cancellationToken)
            => _entities.AnyAsync(r => r.Id == reservaId &&
                (r.Cancelacion != null || r.Sanciones.Any()), cancellationToken);

        // Comprueba si existen reservas activas que se cruzan con el horario.
        public async Task<bool> ExisteCruceHorarioAsync(
            int mesaId,
            DateTime inicio,
            DateTime fin,
            int? excluirReservaId = null,
            CancellationToken cancellationToken = default)
        {
            return await _entities.AnyAsync(r =>
                r.MesaId == mesaId &&
                r.Estado != EstadoReserva.Cancelada &&
                r.Estado != EstadoReserva.NoAsistio &&
                r.FechaHoraInicio < fin &&
                r.FechaHoraFin > inicio &&
                (!excluirReservaId.HasValue || r.Id != excluirReservaId.Value),
                cancellationToken);
        }

        // Busca una reserva por código único incluyendo Cliente y Mesa.
        public async Task<Reserva?> ObtenerPorCodigoAsync(
            string codigo,
            CancellationToken cancellationToken = default)
        {
            return await _entities
                .Include(r => r.Cliente)
                .Include(r => r.Mesa)
                .Include(r => r.Cancelacion)
                .FirstOrDefaultAsync(r => r.Codigo == codigo, cancellationToken);
        }

        // Consulta listado de reservas con filtros opcionales.
        public Task<IEnumerable<Reserva>> ConsultarReservasAsync(
            DateTime? fecha,
            EstadoReserva? estado,
            string? telefono,
            CancellationToken cancellationToken = default)
        {
            const string sql = """
                SELECT r.Id, r.Codigo, r.ClienteId, r.MesaId, r.FechaHoraInicio, r.FechaHoraFin,
                       r.CantidadPersonas, r.Estado, r.FechaRegistro,
                       c.Id, c.Nombre, c.Telefono, c.Correo, c.ContadorSanciones,
                       c.Bloqueado, c.FechaBloqueo, c.BloqueadoHasta,
                       m.Id, m.Numero, m.Capacidad
                FROM reservas r
                INNER JOIN clientes c ON c.Id = r.ClienteId
                INNER JOIN mesas m ON m.Id = r.MesaId
                WHERE (@InicioDia IS NULL OR r.FechaHoraInicio >= @InicioDia)
                  AND (@FinDia IS NULL OR r.FechaHoraInicio < @FinDia)
                  AND (@Estado IS NULL OR r.Estado = @Estado)
                  AND (@Telefono IS NULL OR c.Telefono = @Telefono)
                ORDER BY r.FechaHoraInicio DESC
                """;

            var inicioDia = fecha?.Date;
            var parametros = new
            {
                InicioDia = inicioDia,
                FinDia = inicioDia < DateTime.MaxValue.Date ? inicioDia?.AddDays(1) : null,
                Estado = (int?)estado,
                Telefono = string.IsNullOrWhiteSpace(telefono) ? null : telefono.Trim()
            };

            return _dapper.QueryAsync<Reserva, Cliente, Mesa, Reserva>(
                sql, parametros, (reserva, cliente, mesa) =>
                {
                    reserva.Cliente = cliente;
                    reserva.Mesa = mesa;
                    return reserva;
                }, "Id,Id", cancellationToken);
        }

        public async Task<Reserva> RegistrarReservaConBloqueoAsync(
            CrearReservaDto dto,
            CancellationToken cancellationToken = default)
        {
            await using var transaccion = await _context.Database.BeginTransactionAsync(
                IsolationLevel.ReadCommitted,
                cancellationToken);

            // Bloquea la mesa en MySQL con FOR UPDATE
            var mesas = await _context.Mesas
                .FromSqlRaw("SELECT * FROM mesas WHERE Id = {0} FOR UPDATE", dto.MesaId)
                .ToListAsync(cancellationToken);

            var mesa = mesas.FirstOrDefault();
            if (mesa == null)
            {
                throw new KeyNotFoundException("La mesa no existe.");
            }

            if (dto.CantidadPersonas > mesa.Capacidad)
            {
                throw new ArgumentException($"La mesa admite hasta {mesa.Capacidad} personas.");
            }

            var existeCruce = await ExisteCruceHorarioAsync(
                dto.MesaId,
                dto.FechaHoraInicio,
                dto.FechaHoraFin,
                cancellationToken: cancellationToken);

            if (existeCruce)
            {
                var error = new InvalidOperationException("La mesa ya tiene una reserva en ese horario.");
                error.Data["ConflictoReserva"] = true;
                throw error;
            }

            // Buscar si el cliente ya existe por teléfono para no duplicarlo y validar sanciones
            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.Telefono == dto.TelefonoCliente.Trim(), cancellationToken);

            if (cliente != null)
            {
                if (cliente.EstaBloqueado(DateTime.UtcNow))
                {
                    throw new InvalidOperationException("El cliente se encuentra sancionado y no puede realizar reservas.");
                }

                cliente.Nombre = dto.NombreCliente.Trim();
                if (!string.IsNullOrWhiteSpace(dto.CorreoCliente))
                    cliente.Correo = dto.CorreoCliente.Trim();
            }
            else
            {
                cliente = new Cliente
                {
                    Nombre = dto.NombreCliente.Trim(),
                    Telefono = dto.TelefonoCliente.Trim(),
                    Correo = string.IsNullOrWhiteSpace(dto.CorreoCliente) ? null : dto.CorreoCliente.Trim()
                };
                _context.Clientes.Add(cliente);
                await _context.SaveChangesAsync(cancellationToken);
            }

            var reserva = new Reserva
            {
                Codigo = "RES-" + Guid.NewGuid().ToString("N"),
                ClienteId = cliente.Id,
                Cliente = cliente,
                MesaId = mesa.Id,
                Mesa = mesa,
                FechaHoraInicio = dto.FechaHoraInicio,
                FechaHoraFin = dto.FechaHoraFin,
                CantidadPersonas = dto.CantidadPersonas,
                Estado = EstadoReserva.Confirmada,
                FechaRegistro = DateTime.UtcNow
            };

            _context.Reservas.Add(reserva);
            await _context.SaveChangesAsync(cancellationToken);

            reserva.Codigo = $"RES-{reserva.Id:D4}";
            await _context.SaveChangesAsync(cancellationToken);

            await transaccion.CommitAsync(cancellationToken);

            return reserva;
        }
    }
}
