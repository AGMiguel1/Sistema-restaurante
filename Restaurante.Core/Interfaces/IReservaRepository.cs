using Restaurante.Core.Entities;
using System;
using System.Threading.Tasks;

namespace Restaurante.Core.Interfaces
{
    public interface IReservaRepository : IBaseRepository<Reserva>
    {
        // Comprueba si la mesa tiene reservas que se cruzan con el horario
        Task<bool> ExisteCruceHorarioAsync(
            int mesaId,
            DateTime inicio,
            DateTime fin,
            int? excluirReservaId = null);

        // Obtiene todas las reservas con sus relaciones (Cliente, Mesa, Sanción)
        Task<IEnumerable<Reserva>> GetAllWithDetailsAsync();

        // Obtiene una reserva con sus relaciones (Cliente, Mesa, Sancion)
        Task<Reserva?> GetByIdWithDetailsAsync(int id);

        // Verifica si la reserva ya tiene una sanción asociada
        Task<bool> TieneSancionAsync(int reservaId);

        // Busca un cliente por teléfono o correo para verificar bloqueos o reutilizar datos
        Task<Cliente?> BuscarClientePorTelefonoOCorreoAsync(string telefono, string? correo);
    }
}
