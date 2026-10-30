using Restaurante.Core.DTOs;
using Restaurante.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Restaurante.Core.Interfaces
{
    public interface IReservaRepository : IBaseRepository<Reserva>
    {
        // Comprueba si una mesa tiene reservas en el horario solicitado.
        Task<bool> ExisteCruceHorarioAsync(
            int mesaId,
            DateTime inicio,
            DateTime fin,
            int? excluirReservaId = null,
            CancellationToken cancellationToken = default);

        // Busca una reserva por su código único con sus relaciones (Cliente y Mesa).
        Task<Reserva?> ObtenerPorCodigoAsync(
            string codigo,
            CancellationToken cancellationToken = default);

        // Consulta reservas con filtros opcionales de fecha, estado o teléfono.
        Task<IEnumerable<Reserva>> ConsultarReservasAsync(
            DateTime? fecha,
            EstadoReserva? estado,
            string? telefono,
            CancellationToken cancellationToken = default);

        // Registra el cliente y la reserva protegiendo la mesa con bloqueo pesimista.
        Task<Reserva> RegistrarReservaConBloqueoAsync(
            CrearReservaDto dto,
            CancellationToken cancellationToken = default);
    }
}
