using Restaurante.Core.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Restaurante.Core.Interfaces
{
    public interface IClienteRepository : IBaseRepository<Cliente>
    {
        Task<bool> ExisteTelefonoAsync(string telefono, int? excluirId, CancellationToken cancellationToken);
        Task<bool> TieneRegistrosAsociadosAsync(int clienteId, CancellationToken cancellationToken);

        Task<Cliente?> ObtenerPorTelefonoAsync(
            string telefono,
            CancellationToken cancellationToken = default);
    }
}
