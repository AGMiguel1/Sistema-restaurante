using Restaurante.Core.DTOs;
using Restaurante.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Restaurante.Services.Interfaces
{
    public interface IReservaService
    {
        Task<IEnumerable<Reserva>> GetAllReservasAsync();
        Task<Reserva> GetReservaByIdAsync(int id);
        Task<Reserva> RegistrarReservaAsync(CrearReservaDto dto);
        Task CancelarReservaAsync(int id, string? descripcion = null);
        Task CompletarReservaAsync(int id);
        Task RegistrarInasistenciaAsync(int id, string? descripcion = null);
    }
}

