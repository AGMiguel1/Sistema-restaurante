using Restaurante.Core.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Restaurante.Core.Interfaces
{
    public interface ICancelacionRepository : IBaseRepository<Cancelacion>
    {
        Task<Cancelacion?> ObtenerPorReservaIdAsync(
            int reservaId,
            CancellationToken cancellationToken = default);
    }
}
