using Restaurante.Core.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Restaurante.Core.Interfaces
{
    public interface ISancionRepository : IBaseRepository<Sancion>
    {
        Task<bool> ExisteSancionAsync(
            int reservaId,
            MotivoSancion motivo,
            CancellationToken cancellationToken = default);
    }
}
