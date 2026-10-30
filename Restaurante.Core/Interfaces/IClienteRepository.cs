using Restaurante.Core.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Restaurante.Core.Interfaces
{
    public interface IClienteRepository : IBaseRepository<Cliente>
    {
        Task<Cliente?> ObtenerPorTelefonoAsync(
            string telefono,
            CancellationToken cancellationToken = default);
    }
}
