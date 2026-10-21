using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Restaurante.Api.Response;
using Restaurante.Core.DTOs;
using Restaurante.Core.Interfaces;
using Restaurante.Services.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Restaurante.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservasController : ControllerBase
    {
        private readonly IReservaRepository _reservaRepository;
        private readonly CrearReservaValidator _validador;
        private readonly IMapper _mapper;

        public ReservasController(
            IReservaRepository reservaRepository,
            CrearReservaValidator validador,
            IMapper mapper)
        {
            _reservaRepository = reservaRepository;
            _validador = validador;
            _mapper = mapper;
        }

        // POST: api/reservas
        [HttpPost]
        public async Task<IActionResult> RegistrarReserva(
            [FromBody] CrearReservaDto dto)
        {
            // Valida los datos del cliente, las fechas y las personas.
            var validacion = await _validador.ValidateAsync(dto);

            if (!validacion.IsValid)
            {
                var errores = validacion.Errors
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(
                    new ApiResponse<List<string>>(errores));
            }

            try
            {
                // Registra el cliente y la reserva.
                var reserva = await _reservaRepository
                    .RegistrarReservaConBloqueoAsync(dto);

                // Convierte la reserva a su DTO de respuesta.
                var reservaDto = _mapper
                    .Map<ReservaDetalleDto>(reserva);

                // Devuelve HTTP 201 con la reserva creada.
                return StatusCode(
                    201,
                    new ApiResponse<ReservaDetalleDto>(reservaDto));
            }
            catch (KeyNotFoundException error)
            {
                // HTTP 404: la mesa no existe.
                return NotFound(
                    new ApiResponse<string>(error.Message));
            }
            catch (ArgumentException error)
            {
                // HTTP 400: capacidad insuficiente.
                return BadRequest(
                    new ApiResponse<string>(error.Message));
            }
            catch (InvalidOperationException error)
                when (error.Data.Contains("ConflictoReserva"))
            {
                // HTTP 409: la mesa tiene una reserva superpuesta.
                return Conflict(
                    new ApiResponse<string>(error.Message));
            }
        }
    }
}