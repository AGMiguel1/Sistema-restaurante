using Microsoft.EntityFrameworkCore;
using Restaurante.Core.Entities;
using Restaurante.Core.Interfaces;
using Restaurante.Infrastructure.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Restaurante.Infrastructure.Repositories
{
    public class ClienteRepository : BaseRepository<Cliente>, IClienteRepository
    {
        public ClienteRepository(RestauranteContext context) : base(context)
        {
        }

        public Task<bool> ExisteTelefonoAsync(string telefono, int? excluirId, CancellationToken cancellationToken)
            => _entities.AnyAsync(c => c.Telefono == telefono &&
                (!excluirId.HasValue || c.Id != excluirId.Value), cancellationToken);

        public Task<bool> TieneRegistrosAsociadosAsync(int clienteId, CancellationToken cancellationToken)
            => _entities.AnyAsync(c => c.Id == clienteId &&
                (c.Reservas.Any() || c.Sanciones.Any() ||
                 _context.Usuarios.Any(usuario => usuario.ClienteId == c.Id)), cancellationToken);

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
