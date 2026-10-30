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
        public ReservaRepository(RestauranteContext context) : base(context)
        {
        }

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
        public async Task<IEnumerable<Reserva>> ConsultarReservasAsync(
            DateTime? fecha,
            EstadoReserva? estado,
            string? telefono,
            CancellationToken cancellationToken = default)
        {
            var query = _entities
                .Include(r => r.Cliente)
                .Include(r => r.Mesa)
                .AsNoTracking()
                .AsQueryable();

            if (fecha.HasValue)
            {
                var inicioDia = fecha.Value.Date;
                var finDia = inicioDia.AddDays(1);
                query = query.Where(r => r.FechaHoraInicio >= inicioDia && r.FechaHoraInicio < finDia);
            }

            if (estado.HasValue)
            {
                query = query.Where(r => r.Estado == estado.Value);
            }

            if (!string.IsNullOrWhiteSpace(telefono))
            {
                query = query.Where(r => r.Cliente.Telefono == telefono.Trim());
            }

            return await query
                .OrderByDescending(r => r.FechaHoraInicio)
                .ToListAsync(cancellationToken);
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
