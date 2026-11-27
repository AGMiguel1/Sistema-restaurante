using System.Net;
using AutoMapper;
using Restaurante.Core.DTOs.Reporteria;
using Restaurante.Core.Exceptions;
using Restaurante.Core.Interfaces;
using Restaurante.Services.Interfaces;

namespace Restaurante.Services.Services;

public class ReportesService(IUnitOfWork unitOfWork, IMapper mapper) : IReportesService
{
    public async Task<IEnumerable<ClienteFrecuenteDto>> ObtenerClientesFrecuentesAsync(
        int limite, CancellationToken cancellationToken)
    {
        if (limite is < 1 or > 100)
            throw new BusinessException("El límite debe estar entre 1 y 100.", HttpStatusCode.BadRequest);

        var clientes = await unitOfWork.Reportes.ObtenerClientesFrecuentesAsync(limite, cancellationToken);
        return mapper.Map<IEnumerable<ClienteFrecuenteDto>>(clientes);
    }
}
