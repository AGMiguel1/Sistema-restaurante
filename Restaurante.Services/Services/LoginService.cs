using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Restaurante.Core.DTOs;
using Restaurante.Core.Interfaces;
using Restaurante.Services.Interfaces;
using Restaurante.Core.Entities;

namespace Restaurante.Services.Services
{
    public class LoginService : ILoginService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IConfiguration _configuration;

        public LoginService(
    IUsuarioRepository usuarioRepository,
    IConfiguration configuration)
        {
            _usuarioRepository = usuarioRepository;
            _configuration = configuration;
        }

        public async Task<LoginResponseDto> LoginAsync(
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

            // 5. Preparar la respuesta
            return new LoginResponseDto
            {
                Token = GenerarToken(usuario),
                UsuarioId = usuario.Id,
                Correo = usuario.Correo,
                Rol = usuario.Rol
            };
        }

        private string GenerarToken(Usuario usuario)
        {
            var jwtKey = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("No se encontró la clave JWT.");

            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];

            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
        new Claim(ClaimTypes.Email, usuario.Correo),
        new Claim(ClaimTypes.Role, usuario.Rol)
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey));

            var credentials = new SigningCredentials(
                key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
