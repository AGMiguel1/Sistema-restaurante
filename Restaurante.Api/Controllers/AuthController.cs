using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

using Microsoft.AspNetCore.Mvc;
using Restaurante.Core.DTOs;
using Restaurante.Services.Interfaces;

namespace Restaurante.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ILoginService _loginService;
        private readonly IConfiguration _configuration;

        public AuthController(ILoginService loginService, IConfiguration configuration)
        {
            _loginService = loginService;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginDto dto,
            CancellationToken cancellationToken)
        {
            try
            {
                var usuario = await _loginService.LoginAsync(
                    dto,
                    cancellationToken);

                return Ok(new LoginResponseDto
                {
                    Token = GenerarToken(usuario),
                    UsuarioId = usuario.Id,
                    Correo = usuario.Correo,
                    Rol = usuario.Rol
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { mensaje = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        private string GenerarToken(UsuarioDetalleDto usuario)
        {
            var secretKey = _configuration["Authentication:SecretKey"]
                ?? throw new InvalidOperationException("No se encontró la clave JWT.");
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Email, usuario.Correo),
                new Claim(ClaimTypes.Role, usuario.Rol)
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var token = new JwtSecurityToken(
                issuer: _configuration["Authentication:Issuer"],
                audience: _configuration["Authentication:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
