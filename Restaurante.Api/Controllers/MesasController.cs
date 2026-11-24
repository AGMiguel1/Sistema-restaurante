using Restaurante.Core.Exceptions;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Restaurante.Api.Response;
using Restaurante.Core.DTOs;
using Restaurante.Services.Interfaces;
using Restaurante.Services.Validators;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Restaurante.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MesasController : ControllerBase
    {
        private readonly IMesaService _mesaService;
        private readonly ConsultaDisponibilidadValidator _validador;
        private readonly GuardarMesaValidator _guardarValidator;

        public MesasController(
            IMesaService mesaService,
            ConsultaDisponibilidadValidator validador, GuardarMesaValidator guardarValidator)
        {
            _mesaService = mesaService;
            _validador = validador;
            _guardarValidator = guardarValidator;
        }

        // GET: api/mesas/disponibles (CU-02: Consultar Disponibilidad de Mesas)
        [HttpGet("disponibles")]
        public async Task<IActionResult> ConsultarDisponibilidad(
            [FromQuery] ConsultaDisponibilidadDto consulta,
            CancellationToken cancellationToken)
        {
            var validacion = await _validador.ValidateAsync(consulta, cancellationToken);
            if (!validacion.IsValid)
            {
                throw new BusinessException("Errores de validación en los datos enviados.", HttpStatusCode.BadRequest,
                    validacion.Errors.Select(e => new { Field = e.PropertyName, Error = e.ErrorMessage }));
            }

            var mesas = await _mesaService
                .ConsultarDisponibilidadAsync(consulta, cancellationToken);

            return Ok(new ApiResponse<IEnumerable<MesaDisponibleDto>>(mesas));
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerTodos(CancellationToken cancellationToken)
            => Ok(new ApiResponse<IEnumerable<MesaDetalleDto>>(await _mesaService.ObtenerTodosAsync(cancellationToken)));

        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObtenerPorId(int id, CancellationToken cancellationToken)
            => Ok(new ApiResponse<MesaDetalleDto>(await _mesaService.ObtenerPorIdAsync(id, cancellationToken)));

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] GuardarMesaDto dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid || dto == null)
                throw new BusinessException("La solicitud contiene datos inválidos.", HttpStatusCode.BadRequest,
                    ModelState.SelectMany(x => x.Value!.Errors.Select(e => new { Field = x.Key, Error = string.IsNullOrEmpty(e.ErrorMessage) ? "Valor inválido." : e.ErrorMessage })));

            var validacion = await _guardarValidator.ValidateAsync(dto, cancellationToken);
            if (!validacion.IsValid)
                throw new BusinessException("Errores de validación en los datos enviados.", HttpStatusCode.BadRequest,
                    validacion.Errors.Select(e => new { Field = e.PropertyName, Error = e.ErrorMessage }));


            var resultado = await _mesaService.CrearAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = resultado.Id }, new ApiResponse<MesaDetalleDto>(resultado));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] GuardarMesaDto dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid || dto == null)
                throw new BusinessException("La solicitud contiene datos inválidos.", HttpStatusCode.BadRequest,
                    ModelState.SelectMany(x => x.Value!.Errors.Select(e => new { Field = x.Key, Error = string.IsNullOrEmpty(e.ErrorMessage) ? "Valor inválido." : e.ErrorMessage })));

            var validacion = await _guardarValidator.ValidateAsync(dto, cancellationToken);
            if (!validacion.IsValid)
                throw new BusinessException("Errores de validación en los datos enviados.", HttpStatusCode.BadRequest,
                    validacion.Errors.Select(e => new { Field = e.PropertyName, Error = e.ErrorMessage }));


            return Ok(new ApiResponse<MesaDetalleDto>(await _mesaService.ActualizarAsync(id, dto, cancellationToken)));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id, CancellationToken cancellationToken)
        {
            await _mesaService.EliminarAsync(id, cancellationToken);
            return NoContent();
        }
    }
}
