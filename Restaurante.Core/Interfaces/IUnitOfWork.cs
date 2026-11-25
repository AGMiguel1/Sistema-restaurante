using System.Threading;
using System.Threading.Tasks;

namespace Restaurante.Core.Interfaces;

public interface IUnitOfWork
{
    IMesaRepository Mesas { get; }
    IClienteRepository Clientes { get; }
    IReservaRepository Reservas { get; }
    ICancelacionRepository Cancelaciones { get; }
    ISancionRepository Sanciones { get; }
    IConfiguracionReservaRepository ConfiguracionReservas { get; }

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
