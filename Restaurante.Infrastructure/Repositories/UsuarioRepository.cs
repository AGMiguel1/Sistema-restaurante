using Microsoft.EntityFrameworkCore;
using Restaurante.Core.Entities;
using Restaurante.Core.Interfaces;
using Restaurante.Infrastructure.Data;
using System.Threading;
using System.Threading.Tasks;


namespace Restaurante.Infrastructure.Repositories
{
    public class UsuarioRepository: BaseRepository <Usuario>, IUsuarioRepository
    { 
        public UsuarioRepository(RestauranteContext context) : base(context)
    {
    }

    public async Task<Usuario?> ObtenerPorCorreoAsync(
        string correo,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(correo))
            return null;

        return await _entities
            .FirstOrDefaultAsync(
                u => u.Correo == correo.Trim(),
                cancellationToken);
    }
}
}
