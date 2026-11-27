using Restaurante.Core.DTOs;
using Restaurante.Core.Interfaces;
using Restaurante.Services.Interfaces;
using Restaurante.Core.Entities;

namespace Restaurante.Services.Services
{
    public class LoginService : ILoginService
    {
        private readonly IUsuarioRepository _usuarioRepository;

        public LoginService(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        public async Task<UsuarioDetalleDto> LoginAsync(
    LoginDto dto,
    CancellationToken cancellationToken = default)
        {
            // 1. Validar que se hayan ingresado los datos
            if (string.IsNullOrWhiteSpace(dto.Correo) ||
                string.IsNullOrWhiteSpace(dto.Password))
            {
                throw new ArgumentException("Debe ingresar correo y contraseña.");
            }

            // 2. Buscar al usuario por su correo
            var usuario = await _usuarioRepository.ObtenerPorCorreoAsync(
                dto.Correo,
                cancellationToken);

            // 3. Comprobar que exista y esté activo
            if (usuario == null || !usuario.Activo)
            {
                throw new UnauthorizedAccessException("Credenciales incorrectas.");
            }

            // El prefijo identifica las contraseñas de texto y excluye los formatos anteriores.
            if (!string.Equals(usuario.Password, Usuario.PrefijoPasswordTexto + dto.Password, StringComparison.Ordinal))
            {
                throw new UnauthorizedAccessException("Credenciales incorrectas.");
            }

            return new UsuarioDetalleDto(usuario.Id, usuario.ClienteId, usuario.Correo, usuario.Rol);
        }
    }
}
