using Restaurante.Core.DTOs;
using Restaurante.Core.Entities;
using System;
using System.Threading.Tasks;

namespace Restaurante.Core.Interfaces
{
    public interface IReservaRepository
    {
        // Comprueba si la mesa tiene reservas que se cruzan con el horario
        Task<bool> ExisteCruceHorarioAsync(
            int mesaId,
            DateTime inicio,
            DateTime fin,
            int? excluirReservaId = null);

        // Registra el cliente y la reserva con bloqueo de la mesa
        Task<Reserva> RegistrarReservaConBloqueoAsync(
            CrearReservaDto dto);
    }
}