using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Restaurante.Api.Response;
using Restaurante.Core.DTOs;
using Restaurante.Core.Interfaces;
using Restaurante.Services.Validators;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Restaurante.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MesasController : ControllerBase
    {
        private readonly IMesaRepository _mesaRepository;
        private readonly ConsultaDisponibilidadValidator _validador;
        private readonly IMapper _mapper;

        public MesasController(
            IMesaRepository mesaRepository,
            ConsultaDisponibilidadValidator validador,
            IMapper mapper)
        {
            _mesaRepository = mesaRepository;
            _validador = validador;
            _mapper = mapper;
        }

        // GET: api/mesas/disponibles
        [HttpGet("disponibles")]
        public async Task<IActionResult> ConsultarDisponibilidad(
            [FromQuery] ConsultaDisponibilidadDto consulta,
            CancellationToken cancellationToken)
        {
            // Verifica las fechas y la cantidad de personas.
            var validacion = await _validador.ValidateAsync(
                consulta, cancellationToken);

            if (!validacion.IsValid)
            {
                var errores = validacion.Errors
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(
                    new ApiResponse<List<string>>(errores));
            }

            // Consulta las mesas disponibles en la base de datos.
            var mesas = await _mesaRepository
                .ObtenerMesasDisponiblesAsync(
                    consulta.FechaHoraInicio,
                    consulta.FechaHoraFin,
                    consulta.CantidadPersonas,
                    cancellationToken);

            // Convierte las entidades Mesa a sus DTO.
            var mesasDto = _mapper
                .Map<IEnumerable<MesaDisponibleDto>>(mesas);

            // Devuelve las mesas; si no hay, devuelve una lista vacía.
            return Ok(
                new ApiResponse<IEnumerable<MesaDisponibleDto>>(mesasDto));
        }
    }
}