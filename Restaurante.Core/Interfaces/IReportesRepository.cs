using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Restaurante.Core.CustomEntities.Reporteria;

namespace Restaurante.Core.Interfaces;

public interface IReportesRepository
{
    Task<IEnumerable<ClienteFrecuente>> ObtenerClientesFrecuentesAsync(int limite, CancellationToken cancellationToken);
}
