using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Restaurante.Core.DTOs;
using Restaurante.Core.Entities;
using Restaurante.Core.Exceptions;
using Restaurante.Core.Interfaces;
using Restaurante.Infrastructure.Data;
using Restaurante.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Restaurante.Services.Services
{
    public class ReservaService : IReservaService
    {
        private readonly IReservaRepository _reservaRepository;
        private readonly IMesaRepository _mesaRepository;
        private readonly IClienteRepository _clienteRepository;
        private readonly ICancelacionRepository _cancelacionRepository;
        private readonly ISancionRepository _sancionRepository;
        private readonly RestauranteContext _context;
        private readonly IMapper _mapper;

        public ReservaService(
            IReservaRepository reservaRepository,
            IMesaRepository mesaRepository,
            IClienteRepository clienteRepository,
            ICancelacionRepository cancelacionRepository,
            ISancionRepository sancionRepository,
            RestauranteContext context,
            IMapper mapper)
        {
            _reservaRepository = reservaRepository;
            _mesaRepository = mesaRepository;
            _clienteRepository = clienteRepository;
            _cancelacionRepository = cancelacionRepository;
            _sancionRepository = sancionRepository;
            _context = context;
            _mapper = mapper;
        }

        // CU-03: Registrar reserva con bloqueo pesimista y comprobación de cliente.
        public async Task<ReservaDetalleDto> RegistrarReservaAsync(
            CrearReservaDto dto,
            CancellationToken cancellationToken = default)
        {
            // 1. Verificar si el cliente ya existe y si está bloqueado.
            var clienteExistente = await _clienteRepository
                .ObtenerPorTelefonoAsync(dto.TelefonoCliente, cancellationToken);

            if (clienteExistente != null && clienteExistente.EstaBloqueado(DateTime.UtcNow))
            {
                var motivo = clienteExistente.Bloqueado
                    ? "El cliente se encuentra bloqueado permanentemente por reiteradas sanciones."
                    : $"El cliente se encuentra suspendido hasta el {clienteExistente.BloqueadoHasta:dd/MM/yyyy HH:mm} UTC.";

                throw new BusinessException(motivo, HttpStatusCode.Forbidden);
            }

            // 2. Registrar reserva protegiendo la mesa contra solapamientos.
            Reserva reserva;
            try
            {
                reserva = await _reservaRepository
                    .RegistrarReservaConBloqueoAsync(dto, cancellationToken);
            }
            catch (KeyNotFoundException ex)
            {
                throw new BusinessException(ex.Message, HttpStatusCode.NotFound);
            }
            catch (ArgumentException ex)
            {
                throw new BusinessException(ex.Message, HttpStatusCode.BadRequest);
            }
            catch (InvalidOperationException ex) when (ex.Data.Contains("ConflictoReserva"))
            {
                throw new BusinessException(ex.Message, HttpStatusCode.Conflict);
            }

            return _mapper.Map<ReservaDetalleDto>(reserva);
        }

        // CU-04: Modificar reserva confirmada.
        public async Task<ReservaDetalleDto> ModificarReservaAsync(
            string codigo,
            ModificarReservaDto dto,
            CancellationToken cancellationToken = default)
        {
            var reserva = await _reservaRepository.ObtenerPorCodigoAsync(codigo, cancellationToken);
            if (reserva == null)
            {
                throw new BusinessException($"No existe ninguna reserva con el código {codigo}.", HttpStatusCode.NotFound);
            }

            if (reserva.Estado != EstadoReserva.Confirmada)
            {
                throw new BusinessException(
                    $"No se puede modificar una reserva en estado {reserva.Estado}. Solo se permiten cambios en reservas Confirmadas.",
                    HttpStatusCode.BadRequest);
            }

            var mesa = await _mesaRepository.ObtenerPorIdAsync(dto.MesaId, cancellationToken);
            if (mesa == null)
            {
                throw new BusinessException("La mesa seleccionada no existe.", HttpStatusCode.NotFound);
            }

            if (dto.CantidadPersonas > mesa.Capacidad)
            {
                throw new BusinessException($"La mesa {mesa.Numero} solo admite hasta {mesa.Capacidad} personas.", HttpStatusCode.BadRequest);
            }

            // Validar que no haya cruce en el nuevo horario, excluyendo la reserva actual.
            var hayCruce = await _reservaRepository.ExisteCruceHorarioAsync(
                dto.MesaId,
                dto.FechaHoraInicio,
                dto.FechaHoraFin,
                excluirReservaId: reserva.Id,
                cancellationToken: cancellationToken);

            if (hayCruce)
            {
                throw new BusinessException("La mesa ya tiene otra reserva en ese horario.", HttpStatusCode.Conflict);
            }

            reserva.MesaId = dto.MesaId;
            reserva.Mesa = mesa;
            reserva.FechaHoraInicio = dto.FechaHoraInicio;
            reserva.FechaHoraFin = dto.FechaHoraFin;
            reserva.CantidadPersonas = dto.CantidadPersonas;

            await _reservaRepository.Update(reserva);

            return _mapper.Map<ReservaDetalleDto>(reserva);
        }

        // CU-05: Buscar por código.
        public async Task<ReservaDetalleDto> ObtenerPorCodigoAsync(
            string codigo,
            CancellationToken cancellationToken = default)
        {
            var reserva = await _reservaRepository.ObtenerPorCodigoAsync(codigo, cancellationToken);
            if (reserva == null)
            {
                throw new BusinessException($"No se encontró la reserva con código {codigo}.", HttpStatusCode.NotFound);
            }

            return _mapper.Map<ReservaDetalleDto>(reserva);
        }

        // CU-05: Consultar listado con filtros.
        public async Task<IEnumerable<ReservaDetalleDto>> ConsultarReservasAsync(
            DateTime? fecha,
            EstadoReserva? estado,
            string? telefono,
            CancellationToken cancellationToken = default)
        {
            var reservas = await _reservaRepository
                .ConsultarReservasAsync(fecha, estado, telefono, cancellationToken);

            return _mapper.Map<IEnumerable<ReservaDetalleDto>>(reservas);
        }

        // Cancelación anticipada y cálculo de reembolso.
        public async Task<ResultadoCancelacionDto> CancelarReservaAsync(
            string codigo,
            CancelarReservaDto dto,
            CancellationToken cancellationToken = default)
        {
            var reserva = await _reservaRepository.ObtenerPorCodigoAsync(codigo, cancellationToken);
            if (reserva == null)
            {
                throw new BusinessException($"No se encontró la reserva con código {codigo}.", HttpStatusCode.NotFound);
            }

            if (reserva.Estado == EstadoReserva.Cancelada)
            {
                throw new BusinessException("La reserva ya se encuentra cancelada.", HttpStatusCode.BadRequest);
            }

            if (reserva.Estado == EstadoReserva.Completada)
            {
                throw new BusinessException("No es posible cancelar una reserva que ya fue completada.", HttpStatusCode.BadRequest);
            }

            // Calcular horas de anticipación con respecto a la fecha y hora de inicio
            var horasAnticipacion = (decimal)(reserva.FechaHoraInicio - DateTime.UtcNow).TotalHours;
            if (horasAnticipacion < 0) horasAnticipacion = 0;

            // Consultar política de reembolso activa aplicable
            var politicas = await _context.PoliticasReembolso
                .AsNoTracking()
                .Where(p => p.Activa && p.HorasMinimasAnticipacion <= horasAnticipacion)
                .OrderByDescending(p => p.HorasMinimasAnticipacion)
                .ToListAsync(cancellationToken);

            var porcentaje = politicas.FirstOrDefault()?.PorcentajeReembolso ?? 0m;

            // Obtener monto base de garantía configurable (por defecto 50 si no está configurado)
            var configGarantia = await _context.Configuraciones
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Clave == "MontoGarantiaReserva", cancellationToken);

            decimal montoBase = 50.00m;
            if (configGarantia != null && decimal.TryParse(configGarantia.Valor, out var valorParsed))
            {
                montoBase = valorParsed;
            }

            var montoReembolso = Math.Round(montoBase * (porcentaje / 100m), 2);

            var cancelacion = new Cancelacion
            {
                ReservaId = reserva.Id,
                FechaCancelacion = DateTime.UtcNow,
                CanceladaPor = dto.CanceladaPor.Trim(),
                HorasAnticipacion = Math.Round(horasAnticipacion, 2),
                PorcentajeAplicado = porcentaje,
                MontoReembolso = montoReembolso,
                Motivo = dto.Motivo?.Trim()
            };

            await _cancelacionRepository.Add(cancelacion);

            reserva.Estado = EstadoReserva.Cancelada;
            await _reservaRepository.Update(reserva);

            return new ResultadoCancelacionDto
            {
                CodigoReserva = reserva.Codigo,
                FechaCancelacion = cancelacion.FechaCancelacion,
                CanceladaPor = cancelacion.CanceladaPor,
                HorasAnticipacion = cancelacion.HorasAnticipacion,
                PorcentajeReembolso = cancelacion.PorcentajeAplicado,
                MontoReembolso = cancelacion.MontoReembolso,
                Mensaje = porcentaje > 0
                    ? $"Reserva cancelada con éxito. Corresponde un reembolso del {porcentaje}% ({montoReembolso:F2} Bs.)."
                    : "Reserva cancelada. No aplica reembolso debido al tiempo de anticipación."
            };
        }

        // Registrar inasistencia del cliente, aplicando escala de sanciones y bloqueo.
        public async Task<ReservaDetalleDto> RegistrarInasistenciaAsync(
            string codigo,
            CancellationToken cancellationToken = default)
        {
            var reserva = await _reservaRepository.ObtenerPorCodigoAsync(codigo, cancellationToken);
            if (reserva == null)
            {
                throw new BusinessException($"No se encontró la reserva con código {codigo}.", HttpStatusCode.NotFound);
            }

            if (reserva.Estado != EstadoReserva.Confirmada)
            {
                throw new BusinessException(
                    $"Solo se puede registrar inasistencia en reservas Confirmadas. La reserva actual se encuentra {reserva.Estado}.",
                    HttpStatusCode.BadRequest);
            }

            var yaSancionada = await _sancionRepository
                .ExisteSancionAsync(reserva.Id, MotivoSancion.Inasistencia, cancellationToken);

            if (yaSancionada)
            {
                throw new BusinessException("Esta reserva ya tiene registrada una sanción por inasistencia.", HttpStatusCode.Conflict);
            }

            // Cambiar estado a NoAsistio
            reserva.Estado = EstadoReserva.NoAsistio;

            // Incrementar contador de faltas del cliente
            var cliente = reserva.Cliente;
            cliente.ContadorSanciones++;

            // Consultar escala de sanción correspondiente al número de falta
            var escala = await _context.EscalasSancion
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.NumeroFalta == cliente.ContadorSanciones, cancellationToken);

            if (escala != null && escala.Bloquea)
            {
                if (escala.DuracionDias.HasValue && escala.DuracionDias.Value > 0)
                {
                    cliente.BloqueadoHasta = DateTime.UtcNow.AddDays(escala.DuracionDias.Value);
                }
                else
                {
                    cliente.Bloqueado = true;
                    cliente.FechaBloqueo = DateTime.UtcNow;
                }
            }

            var sancion = new Sancion
            {
                ClienteId = cliente.Id,
                ReservaId = reserva.Id,
                Motivo = MotivoSancion.Inasistencia,
                NumeroFalta = cliente.ContadorSanciones,
                FechaRegistro = DateTime.UtcNow,
                FechaFin = cliente.BloqueadoHasta
            };

            await _sancionRepository.Add(sancion);
            await _clienteRepository.Update(cliente);
            await _reservaRepository.Update(reserva);

            return _mapper.Map<ReservaDetalleDto>(reserva);
        }
    }
}
