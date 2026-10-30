using Microsoft.EntityFrameworkCore;
using Restaurante.Core.Entities;
using Restaurante.Core.Interfaces;
using Restaurante.Infrastructure.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Restaurante.Infrastructure.Repositories
{
    public class SancionRepository : BaseRepository<Sancion>, ISancionRepository
    {
        public SancionRepository(RestauranteContext context) : base(context)
        {
        }

        public async Task<bool> ExisteSancionAsync(
            int reservaId,
            MotivoSancion motivo,
            CancellationToken cancellationToken = default)
        {
            return await _entities
                .AnyAsync(s => s.ReservaId == reservaId && s.Motivo == motivo, cancellationToken);
        }
    }
}
