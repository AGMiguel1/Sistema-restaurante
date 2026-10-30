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
    public class ReservasController : ControllerBase
    {
        private readonly IReservaService _reservaService;
        private readonly CrearReservaValidator _validador;
        private readonly IMapper _mapper;

        public ReservasController(
            IReservaService reservaService,
            CrearReservaValidator validador,
            IMapper mapper)
        {
            _reservaService = reservaService;
            _validador = validador;
            _mapper = mapper;
        }

        // GET: api/reservas
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var reservas = await _reservaService.GetAllReservasAsync();
            var reservasDto = _mapper.Map<IEnumerable<ReservaDetalleDto>>(reservas);
            var response = new ApiResponse<IEnumerable<ReservaDetalleDto>>(reservasDto);
            return Ok(response);
        }

        // GET: api/reservas/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var reserva = await _reservaService.GetReservaByIdAsync(id);
            var reservaDto = _mapper.Map<ReservaDetalleDto>(reserva);
            var response = new ApiResponse<ReservaDetalleDto>(reservaDto);
            return Ok(response);
        }

        // POST: api/reservas
        [HttpPost]
        public async Task<IActionResult> RegistrarReserva([FromBody] CrearReservaDto dto)
        {
            // Valida los datos del cliente, las fechas y las personas.
            var validacion = await _validador.ValidateAsync(dto);
            if (!validacion.IsValid)
            {
                throw new BusinessException(
                    "Errores de validación en la reserva.",
                    HttpStatusCode.BadRequest,
                    validacion.Errors.Select(e => new { Field = e.PropertyName, Error = e.ErrorMessage }));
            }

            var reserva = await _reservaService.RegistrarReservaAsync(dto);
            var reservaDto = _mapper.Map<ReservaDetalleDto>(reserva);
            var response = new ApiResponse<ReservaDetalleDto>(reservaDto);

            return CreatedAtAction(nameof(GetById), new { id = reserva.Id }, response);
        }

        // POST / PUT: api/reservas/{id}/cancelar
        [HttpPost("{id}/cancelar")]
        [HttpPut("{id}/cancelar")]
        public async Task<IActionResult> CancelarReserva(int id, [FromBody] CambiarEstadoReservaDto? dto = null)
        {
            await _reservaService.CancelarReservaAsync(id, dto?.Descripcion);
            var response = new ApiResponse<string>("Reserva cancelada exitosamente y sanción registrada.");
            return Ok(response);
        }

        // POST / PUT: api/reservas/{id}/completar
        [HttpPost("{id}/completar")]
        [HttpPut("{id}/completar")]
        public async Task<IActionResult> CompletarReserva(int id)
        {
            await _reservaService.CompletarReservaAsync(id);
            var response = new ApiResponse<string>("Reserva completada exitosamente.");
            return Ok(response);
        }

        // POST / PUT: api/reservas/{id}/inasistencia
        [HttpPost("{id}/inasistencia")]
        [HttpPut("{id}/inasistencia")]
        [HttpPost("{id}/no-asistio")]
        public async Task<IActionResult> RegistrarInasistencia(int id, [FromBody] CambiarEstadoReservaDto? dto = null)
        {
            await _reservaService.RegistrarInasistenciaAsync(id, dto?.Descripcion);
            var response = new ApiResponse<string>("Inasistencia registrada exitosamente y sanción aplicada.");
            return Ok(response);
        }
    }
}
