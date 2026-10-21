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
    public class ReservaRepository : IReservaRepository
    {
        private readonly RestauranteContext _context;

        public ReservaRepository(RestauranteContext context)
        {
            _context = context;
        }

        // Comprueba si existen reservas que se cruzan con el horario.
        public async Task<bool> ExisteCruceHorarioAsync(
            int mesaId,
            DateTime inicio,
            DateTime fin,
            int? excluirReservaId = null,
            CancellationToken cancellationToken = default)
        {
            return await _context.Reservas.AnyAsync(r =>
                r.MesaId == mesaId &&
                r.Estado != EstadoReserva.Cancelada &&
                r.FechaHoraInicio < fin &&
                r.FechaHoraFin > inicio &&
                (!excluirReservaId.HasValue ||
                 r.Id != excluirReservaId.Value),
                cancellationToken);
        }

        public async Task<Reserva> RegistrarReservaConBloqueoAsync(
            CrearReservaDto dto,
            CancellationToken cancellationToken = default)
        {
            // Todas las operaciones se confirman juntas.
            // Si ocurre un error, la transacción se revierte al desecharse.
            await using var transaccion =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.ReadCommitted,
                    cancellationToken);

            // Bloquea la mesa mientras se verifica y registra la reserva.
            // Otra solicitud para la misma mesa debe esperar.
            // El valor {0} se envía como parámetro a MySQL.
            var mesas = await _context.Mesas
                .FromSqlRaw(
                    "SELECT * FROM mesas WHERE Id = {0} FOR UPDATE",
                    dto.MesaId)
                .ToListAsync(cancellationToken);

            var mesa = mesas.FirstOrDefault();

            // Verifica que la mesa exista.
            if (mesa == null)
            {
                throw new KeyNotFoundException(
                    "La mesa no existe.");
            }

            // Verifica que tenga capacidad suficiente.
            if (dto.CantidadPersonas > mesa.Capacidad)
            {
                throw new ArgumentException(
                    $"La mesa admite hasta {mesa.Capacidad} personas.");
            }

            // Comprueba el horario mientras la mesa está bloqueada.
            var existeCruce = await ExisteCruceHorarioAsync(
                dto.MesaId,
                dto.FechaHoraInicio,
                dto.FechaHoraFin,
                cancellationToken: cancellationToken);

            if (existeCruce)
            {
                var error = new InvalidOperationException(
                    "La mesa ya tiene una reserva en ese horario.");

                // Permite al controlador identificar este conflicto.
                error.Data["ConflictoReserva"] = true;

                throw error;
            }

            // Crea al cliente con los datos recibidos.
            var cliente = new Cliente
            {
                Nombre = dto.NombreCliente.Trim(),
                Telefono = dto.TelefonoCliente.Trim(),
                Correo = string.IsNullOrWhiteSpace(dto.CorreoCliente)
                    ? null
                    : dto.CorreoCliente.Trim()
            };

            // Crea la reserva relacionada con el cliente y la mesa.
            var reserva = new Reserva
            {
                // Código provisional hasta obtener el Id de MySQL.
                Codigo = "RES-" + Guid.NewGuid().ToString("N"),
                Cliente = cliente,
                MesaId = mesa.Id,
                Mesa = mesa,
                FechaHoraInicio = dto.FechaHoraInicio,
                FechaHoraFin = dto.FechaHoraFin,
                CantidadPersonas = dto.CantidadPersonas,
                Estado = EstadoReserva.Confirmada,
                FechaRegistro = DateTime.UtcNow
            };

            // Guarda el cliente y la reserva para obtener sus Id.
            _context.Reservas.Add(reserva);
            await _context.SaveChangesAsync(cancellationToken);

            // Reemplaza el código provisional por uno sencillo.
            // Ejemplo: Id 3 se convierte en RES-0003.
            reserva.Codigo = $"RES-{reserva.Id:D4}";
            await _context.SaveChangesAsync(cancellationToken);

            // Confirma los cambios y libera el bloqueo de la mesa.
            await transaccion.CommitAsync(cancellationToken);

            return reserva;
        }
    }
}