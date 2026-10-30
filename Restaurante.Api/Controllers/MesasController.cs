using FluentValidation;
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

        public MesasController(
            IMesaService mesaService,
            ConsultaDisponibilidadValidator validador)
        {
            _mesaService = mesaService;
            _validador = validador;
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
                throw new ValidationException(validacion.Errors);
            }

            var mesas = await _mesaService
                .ConsultarDisponibilidadAsync(consulta, cancellationToken);

            return Ok(new ApiResponse<IEnumerable<MesaDisponibleDto>>(mesas));
        }
    }
}
