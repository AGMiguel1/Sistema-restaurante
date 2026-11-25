using Restaurante.Core.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Restaurante.Core.Interfaces;

public interface IConfiguracionReservaRepository
{
    Task<decimal> ObtenerPorcentajeReembolsoAsync(decimal horasAnticipacion, CancellationToken cancellationToken);
    Task<string?> ObtenerValorAsync(string clave, CancellationToken cancellationToken);
    Task<EscalaSancion?> ObtenerEscalaSancionAsync(int numeroFalta, CancellationToken cancellationToken);
}
