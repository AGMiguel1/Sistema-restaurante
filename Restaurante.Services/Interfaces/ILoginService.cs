
using Restaurante.Core.DTOs;
using System.Threading;
using System.Threading.Tasks;

namespace Restaurante.Services.Interfaces
{
    public  interface ILoginService
    {
        Task<LoginResponseDto> LoginAsync(LoginDto dto,
            CancellationToken cancellationToken = default);
     
    }
}
