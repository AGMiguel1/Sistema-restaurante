using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Restaurante.Api.Response;
using Restaurante.Core.DTOs;
using Restaurante.Core.Entities;
using Restaurante.Core.Exceptions;
using Restaurante.Services.Interfaces;
using Restaurante.Services.Validators;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Restaurante.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MesasController : ControllerBase
    {
        private readonly IMesaService _mesaService;
        private readonly ConsultaDisponibilidadValidator _validador;
        private readonly IMapper _mapper;

        public MesasController(
            IMesaService mesaService,
            ConsultaDisponibilidadValidator validador,
            IMapper mapper)
        {
            _mesaService = mesaService;
            _validador = validador;
            _mapper = mapper;
        }

        // GET: api/mesas
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var mesas = await _mesaService.GetAllMesasAsync();
            var mesasDto = _mapper.Map<IEnumerable<MesaDisponibleDto>>(mesas);
            var response = new ApiResponse<IEnumerable<MesaDisponibleDto>>(mesasDto);
            return Ok(response);
        }

        // GET: api/mesas/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var mesa = await _mesaService.GetMesaByIdAsync(id);
            var mesaDto = _mapper.Map<MesaDisponibleDto>(mesa);
            var response = new ApiResponse<MesaDisponibleDto>(mesaDto);
            return Ok(response);
        }

        // GET: api/mesas/disponibles
        [HttpGet("disponibles")]
        public async Task<IActionResult> ConsultarDisponibilidad(
            [FromQuery] ConsultaDisponibilidadDto consulta)
        {
            // Valida las fechas y la cantidad de personas
            var validacion = await _validador.ValidateAsync(consulta);
            if (!validacion.IsValid)
            {
                throw new BusinessException(
                    "Errores de validación en la consulta de disponibilidad.",
                    HttpStatusCode.BadRequest,
                    validacion.Errors.Select(e => new { Field = e.PropertyName, Error = e.ErrorMessage }));
            }

            // Consulta las mesas disponibles a través del servicio
            var mesas = await _mesaService.ObtenerMesasDisponiblesAsync(
                consulta.FechaHoraInicio,
                consulta.FechaHoraFin,
                consulta.CantidadPersonas);

            var mesasDto = _mapper.Map<IEnumerable<MesaDisponibleDto>>(mesas);
            var response = new ApiResponse<IEnumerable<MesaDisponibleDto>>(mesasDto);

            return Ok(response);
        }
    }
}
