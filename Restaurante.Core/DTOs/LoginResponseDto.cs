
namespace Restaurante.Core.DTOs
{
    public class LoginResponseDto
    {
        public string Token { get; set; } = null!;

        public int UsuarioId { get; set; }

        public string Correo { get; set; } = null!;

        public string Rol { get; set; } = null!;
    }
}
