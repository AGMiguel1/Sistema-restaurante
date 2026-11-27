using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurante.Api.Response;
using Restaurante.Core.DTOs;
using Restaurante.Core.Enums;
using Restaurante.Core.Exceptions;
using Restaurante.Services.Interfaces;

namespace Restaurante.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SecurityController(IRegistroUsuarioService registroUsuarioService) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = nameof(RoleType.Administrator))]
    public async Task<IActionResult> Registrar([FromBody] RegistroUsuarioDto? dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid || dto is null)
            throw new BusinessException("La solicitud contiene datos inválidos.", HttpStatusCode.BadRequest);

        var usuario = await registroUsuarioService.RegistrarAsync(dto, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, new ApiResponse<UsuarioDetalleDto>(usuario));
    }

    [HttpGet("perfil")]
    [Authorize(Roles = nameof(RoleType.Consumer))]
    public IActionResult ObtenerPerfil()
    {
        return Ok(new ApiResponse<object>(new
        {
            UsuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier),
            Correo = User.FindFirstValue(ClaimTypes.Email),
            Rol = User.FindFirstValue(ClaimTypes.Role)
        }));
    }
}
