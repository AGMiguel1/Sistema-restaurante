using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Restaurante.Api.Response;
using Restaurante.Core.DTOs;
using Restaurante.Core.Entities;
using Restaurante.Core.Interfaces;
using Restaurante.Services.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
            [FromBody] CrearReservaDto dto,
            CancellationToken cancellationToken)
        {
            // Verifica los datos recibidos
            var validacion = await _validador.ValidateAsync(
                dto, cancellationToken);

            if (!validacion.IsValid)
            {
                var errores = validacion.Errors
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(
                    new ApiResponse<List<string>>(errores));
            }

            Reserva reserva;

            try
            {
                // Registra la reserva y protege la mesa
                // contra reservas simultáneas
                reserva = await _reservaRepository
                    .RegistrarReservaConBloqueoAsync(
                        dto, cancellationToken);
            }
            catch (KeyNotFoundException error)
            {
                // La mesa no existe
                return NotFound(
                    new ApiResponse<string>(error.Message));
            }
            catch (ArgumentException error)
            {
                // La mesa está inactiva o tiene capacidad insuficiente
                return BadRequest(
                    new ApiResponse<string>(error.Message));
            }
            catch (InvalidOperationException error)
                when (error.Data.Contains("ConflictoReserva"))
            {
                // La mesa ya tiene una reserva en ese horario
                return Conflict(new ApiResponse<string>(error.Message));
            }

            // Convierte la reserva al DTO de confirmación.
            var reservaDto = _mapper.Map<ReservaDetalleDto>(reserva);

            // Devuelve HTTP 201 con la reserva registrada.
            return StatusCode(
                201,
                new ApiResponse<ReservaDetalleDto>(reservaDto));
        }
    }
}