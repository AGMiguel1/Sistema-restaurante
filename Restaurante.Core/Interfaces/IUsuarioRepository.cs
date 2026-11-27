using Restaurante.Core.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Restaurante.Core.Interfaces
{
    public interface IUsuarioRepository:IBaseRepository<Usuario>
    {
        Task<Usuario?> ObtenerPorCorreoAsync(string correo,
            CancellationToken cancellationToken = default); 
    }
}
