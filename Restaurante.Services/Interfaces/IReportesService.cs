using Restaurante.Core.DTOs.Reporteria;

namespace Restaurante.Services.Interfaces;

public interface IReportesService
{
    Task<IEnumerable<ClienteFrecuenteDto>> ObtenerClientesFrecuentesAsync(int limite, CancellationToken cancellationToken);
}
