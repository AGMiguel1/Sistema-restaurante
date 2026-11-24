using Restaurante.Services.Validators;
using Restaurante.Core.Exceptions;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Restaurante.Api.Response;
using Restaurante.Core.DTOs;
using Restaurante.Services.Interfaces;

namespace Restaurante.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly IClienteService _service;
    private readonly GuardarClienteValidator _guardarValidator;

    public ClientesController(IClienteService service, GuardarClienteValidator guardarValidator)
    {
        _service = service;
        _guardarValidator = guardarValidator;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodos(CancellationToken cancellationToken)
        => Ok(new ApiResponse<IEnumerable<ClienteDetalleDto>>(await _service.ObtenerTodosAsync(cancellationToken)));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerPorId(int id, CancellationToken cancellationToken)
        => Ok(new ApiResponse<ClienteDetalleDto>(await _service.ObtenerPorIdAsync(id, cancellationToken)));

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] GuardarClienteDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid || dto == null)
            throw new BusinessException("La solicitud contiene datos inválidos.", HttpStatusCode.BadRequest,
                ModelState.SelectMany(x => x.Value!.Errors.Select(e => new { Field = x.Key, Error = string.IsNullOrEmpty(e.ErrorMessage) ? "Valor inválido." : e.ErrorMessage })));

        var validacion = await _guardarValidator.ValidateAsync(dto, cancellationToken);
        if (!validacion.IsValid)
            throw new BusinessException("Errores de validación en los datos enviados.", HttpStatusCode.BadRequest,
                validacion.Errors.Select(e => new { Field = e.PropertyName, Error = e.ErrorMessage }));


        var resultado = await _service.CrearAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = resultado.Id }, new ApiResponse<ClienteDetalleDto>(resultado));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] GuardarClienteDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid || dto == null)
            throw new BusinessException("La solicitud contiene datos inválidos.", HttpStatusCode.BadRequest,
                ModelState.SelectMany(x => x.Value!.Errors.Select(e => new { Field = x.Key, Error = string.IsNullOrEmpty(e.ErrorMessage) ? "Valor inválido." : e.ErrorMessage })));

        var validacion = await _guardarValidator.ValidateAsync(dto, cancellationToken);
        if (!validacion.IsValid)
            throw new BusinessException("Errores de validación en los datos enviados.", HttpStatusCode.BadRequest,
                validacion.Errors.Select(e => new { Field = e.PropertyName, Error = e.ErrorMessage }));


        return Ok(new ApiResponse<ClienteDetalleDto>(await _service.ActualizarAsync(id, dto, cancellationToken)));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id, CancellationToken cancellationToken)
    {
        await _service.EliminarAsync(id, cancellationToken);
        return NoContent();
    }

}
