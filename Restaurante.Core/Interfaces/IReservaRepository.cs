using Restaurante.Core.DTOs;
using Restaurante.Core.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Restaurante.Core.Interfaces
{
    public interface IReservaRepository
    {
        // Comprueba si una mesa tiene reservas en el horario solicitado
        Task<bool> ExisteCruceHorarioAsync(
            int mesaId,
            DateTime inicio,
            DateTime fin,
            int? excluirReservaId = null,
            CancellationToken cancellationToken = default);

        // Registra el cliente y la reserva la mesa
        Task<Reserva> RegistrarReservaConBloqueoAsync(
            CrearReservaDto dto,
            CancellationToken cancellationToken = default);
    }
}