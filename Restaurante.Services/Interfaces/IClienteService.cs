using Restaurante.Core.DTOs;

namespace Restaurante.Services.Interfaces;

public interface IClienteService
{
        Task<IEnumerable<ClienteDetalleDto>> ObtenerTodosAsync(CancellationToken cancellationToken = default);
        Task<ClienteDetalleDto> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
        Task<ClienteDetalleDto> CrearAsync(GuardarClienteDto dto, CancellationToken cancellationToken = default);
        Task<ClienteDetalleDto> ActualizarAsync(int id, GuardarClienteDto dto, CancellationToken cancellationToken = default);
        Task EliminarAsync(int id, CancellationToken cancellationToken = default);
}
