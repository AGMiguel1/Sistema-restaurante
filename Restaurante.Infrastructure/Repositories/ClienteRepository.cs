using Microsoft.EntityFrameworkCore;
using Restaurante.Core.Entities;
using Restaurante.Core.Interfaces;
using Restaurante.Infrastructure.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Restaurante.Infrastructure.Repositories
{
    public class ClienteRepository : BaseRepository<Cliente>, IClienteRepository
    {
        public ClienteRepository(RestauranteContext context) : base(context)
        {
        }

        public async Task<Cliente?> ObtenerPorTelefonoAsync(
            string telefono,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(telefono))
                return null;

            return await _entities
                .FirstOrDefaultAsync(c => c.Telefono == telefono.Trim(), cancellationToken);
        }
    }
}
