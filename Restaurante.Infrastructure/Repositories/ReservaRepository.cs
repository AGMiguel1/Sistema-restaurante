using Microsoft.EntityFrameworkCore;
using Restaurante.Core.DTOs;
using Restaurante.Core.Entities;
using Restaurante.Core.Interfaces;
using Restaurante.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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

        // Comprueba si existen reservas que se cruzan con el horario
        public async Task<bool> ExisteCruceHorarioAsync(
            int mesaId,
            DateTime inicio,
            DateTime fin,
            int? excluirReservaId = null)
        {
            var consulta = _context.Reservas
                .Where(r => r.MesaId == mesaId)
                .Where(r => r.Estado != EstadoReserva.Cancelada)
                .Where(r =>
                    r.FechaHoraInicio < fin &&
                    r.FechaHoraFin > inicio);

            // Permite ignorar una reserva si se indica su identificador
            if (excluirReservaId.HasValue)
            {
                consulta = consulta.Where(r =>
                    r.Id != excluirReservaId.Value);
            }

            return await consulta.AnyAsync();
        }

        // Registra el cliente y la reserva dentro de una transaccion
        public async Task<Reserva> RegistrarReservaConBloqueoAsync(
            CrearReservaDto dto)
        {
            await using var transaccion =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.ReadCommitted);

            try
            {
                // Bloquea la fila de la mesa hasta terminar la transacción
                var mesas = await _context.Mesas
                    .FromSqlRaw(
                        "SELECT * FROM mesas WHERE Id = {0} FOR UPDATE",
                        dto.MesaId)
                    .ToListAsync();

                var mesa = mesas.FirstOrDefault();

                // Verifica que la mesa exista
                if (mesa == null)
                {
                    throw new KeyNotFoundException(
                        "La mesa seleccionada no existe.");
                }

                // Verifica que haya capacidad suficiente
                if (dto.CantidadPersonas > mesa.Capacidad)
                {
                    throw new ArgumentException(
                        "La mesa no tiene capacidad suficiente.");
                }

                // Comprueba el horario mientras la mesa está bloqueada
                var existeCruce = await ExisteCruceHorarioAsync(
                    dto.MesaId,
                    dto.FechaHoraInicio,
                    dto.FechaHoraFin);

                if (existeCruce)
                {
                    var error = new InvalidOperationException(
                        "La mesa ya tiene una reserva en ese horario.");

                    // Permite al controlador identificar el conflicto
                    error.Data["ConflictoReserva"] = true;

                    throw error;
                }

                // Crea los datos del cliente
                var cliente = new Cliente
                {
                    Nombre = dto.NombreCliente.Trim(),
                    Telefono = dto.TelefonoCliente.Trim(),
                    Correo = string.IsNullOrWhiteSpace(dto.CorreoCliente)
                        ? null
                        : dto.CorreoCliente.Trim()
                };

                // Crea una reserva confirmada
                var reserva = new Reserva
                {
                    // Código provisional hasta obtener el Id
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

                // Guarda el cliente y la reserva para obtener sus Id
                _context.Reservas.Add(reserva);
                await _context.SaveChangesAsync();

                // Genera el código definitivo: RES-0001, 
                reserva.Codigo = $"RES-{reserva.Id:D4}";
                await _context.SaveChangesAsync();

                // Confirma todos los cambios
                await transaccion.CommitAsync();

                return reserva;
            }
            catch
            {
                // Revierte los cambios si ocurre un error
                await transaccion.RollbackAsync();
                throw;
            }
        }
    }
}