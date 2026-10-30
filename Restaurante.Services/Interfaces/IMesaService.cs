using Restaurante.Core.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Restaurante.Services.Interfaces
{
    public interface IMesaService
    {
        Task<IEnumerable<MesaDisponibleDto>> ConsultarDisponibilidadAsync(
            ConsultaDisponibilidadDto consulta,
            CancellationToken cancellationToken = default);
    }
}
