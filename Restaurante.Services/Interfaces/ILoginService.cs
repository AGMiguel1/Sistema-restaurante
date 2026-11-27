
using Restaurante.Core.DTOs;
using System.Threading;
using System.Threading.Tasks;

namespace Restaurante.Services.Interfaces
{
    public  interface ILoginService
    {
        Task<UsuarioDetalleDto> LoginAsync(LoginDto dto,
            CancellationToken cancellationToken = default);
     
    }
}
