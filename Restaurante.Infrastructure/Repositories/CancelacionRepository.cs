using Microsoft.EntityFrameworkCore;
using Restaurante.Core.Entities;
using Restaurante.Core.Interfaces;
using Restaurante.Infrastructure.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Restaurante.Infrastructure.Repositories
{
    public class CancelacionRepository : BaseRepository<Cancelacion>, ICancelacionRepository
    {
        public CancelacionRepository(RestauranteContext context) : base(context)
        {
        }

        public async Task<Cancelacion?> ObtenerPorReservaIdAsync(
            int reservaId,
            CancellationToken cancellationToken = default)
        {
            return await _entities
                .FirstOrDefaultAsync(c => c.ReservaId == reservaId, cancellationToken);
        }
    }
}
