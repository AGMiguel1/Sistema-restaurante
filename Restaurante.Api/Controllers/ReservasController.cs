using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Restaurante.Api.Response;
using Restaurante.Core.DTOs;
using Restaurante.Core.Entities;
using Restaurante.Services.Interfaces;
using Restaurante.Services.Validators;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Restaurante.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservasController : ControllerBase
    {
        private readonly IReservaService _reservaService;
        private readonly CrearReservaValidator _crearValidator;
        private readonly ModificarReservaValidator _modificarValidator;
        private readonly CancelarReservaValidator _cancelarValidator;

        public ReservasController(
            IReservaService reservaService,
            CrearReservaValidator crearValidator,
            ModificarReservaValidator modificarValidator,
            CancelarReservaValidator cancelarValidator)
        {
            _reservaService = reservaService;
            _crearValidator = crearValidator;
            _modificarValidator = modificarValidator;
            _cancelarValidator = cancelarValidator;
        }

        // POST: api/reservas (CU-03: Registrar Reserva)
        [HttpPost]
        public async Task<IActionResult> RegistrarReserva(
            [FromBody] CrearReservaDto dto,
            CancellationToken cancellationToken)
        {
            var validacion = await _crearValidator.ValidateAsync(dto, cancellationToken);
            if (!validacion.IsValid)
            {
                throw new ValidationException(validacion.Errors);
            }

            var reserva = await _reservaService.RegistrarReservaAsync(dto, cancellationToken);

            return StatusCode(201, new ApiResponse<ReservaDetalleDto>(reserva));
        }

        // PUT: api/reservas/{codigo} (CU-04: Modificar Reserva)
        [HttpPut("{codigo}")]
        public async Task<IActionResult> ModificarReserva(
            string codigo,
            [FromBody] ModificarReservaDto dto,
            CancellationToken cancellationToken)
        {
            var validacion = await _modificarValidator.ValidateAsync(dto, cancellationToken);
            if (!validacion.IsValid)
            {
                throw new ValidationException(validacion.Errors);
            }

            var reserva = await _reservaService.ModificarReservaAsync(codigo, dto, cancellationToken);

            return Ok(new ApiResponse<ReservaDetalleDto>(reserva));
        }

        // GET: api/reservas/{codigo} (CU-05: Buscar Reserva por Código)
        [HttpGet("{codigo}")]
        public async Task<IActionResult> ObtenerPorCodigo(
            string codigo,
            CancellationToken cancellationToken)
        {
            var reserva = await _reservaService.ObtenerPorCodigoAsync(codigo, cancellationToken);
            return Ok(new ApiResponse<ReservaDetalleDto>(reserva));
        }

        // GET: api/reservas (CU-05: Consultar Reservas con Filtros)
        [HttpGet]
        public async Task<IActionResult> ConsultarReservas(
            [FromQuery] DateTime? fecha,
            [FromQuery] EstadoReserva? estado,
            [FromQuery] string? telefono,
            CancellationToken cancellationToken)
        {
            var reservas = await _reservaService
                .ConsultarReservasAsync(fecha, estado, telefono, cancellationToken);

            return Ok(new ApiResponse<IEnumerable<ReservaDetalleDto>>(reservas));
        }

        // POST: api/reservas/{codigo}/cancelar (Cancelación Anticipada con Reembolso)
        [HttpPost("{codigo}/cancelar")]
        public async Task<IActionResult> CancelarReserva(
            string codigo,
            [FromBody] CancelarReservaDto dto,
            CancellationToken cancellationToken)
        {
            var validacion = await _cancelarValidator.ValidateAsync(dto, cancellationToken);
            if (!validacion.IsValid)
            {
                throw new ValidationException(validacion.Errors);
            }

            var resultado = await _reservaService.CancelarReservaAsync(codigo, dto, cancellationToken);
            return Ok(new ApiResponse<ResultadoCancelacionDto>(resultado));
        }

        // POST: api/reservas/{codigo}/inasistencia (Registro de Inasistencia y Sanción)
        [HttpPost("{codigo}/inasistencia")]
        public async Task<IActionResult> RegistrarInasistencia(
            string codigo,
            CancellationToken cancellationToken)
        {
            var reserva = await _reservaService.RegistrarInasistenciaAsync(codigo, cancellationToken);
            return Ok(new ApiResponse<ReservaDetalleDto>(reserva));
        }
    }
}
