namespace Restaurante.Core.DTOs
{
    public class RegistroUsuarioDto
    {
        public int ClienteId { get; set; }

        public string Correo { get; set; } = null!;

        public string Password { get; set; } = null!;
    }
}