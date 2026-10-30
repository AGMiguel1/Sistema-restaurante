using Microsoft.Extensions.Configuration;
using Restaurante.Core.DTOs;
using Restaurante.Core.Entities;
using Restaurante.Core.Exceptions;
using Restaurante.Core.Interfaces;
using Restaurante.Infrastructure.Data;
using Restaurante.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Restaurante.Services.Services
{
    public class ReservaService : IReservaService
    {
        private readonly IBaseRepository<Reserva> _reservaBaseRepository;
        private readonly IBaseRepository<Cliente> _clienteBaseRepository;
        private readonly IBaseRepository<Mesa> _mesaBaseRepository;
        private readonly IBaseRepository<Sancion> _sancionBaseRepository;
        private readonly IReservaRepository _reservaRepository;
        private readonly RestauranteContext _context;
        private readonly IConfiguration _configuration;

        public ReservaService(
            IBaseRepository<Reserva> reservaBaseRepository,
            IBaseRepository<Cliente> clienteBaseRepository,
            IBaseRepository<Mesa> mesaBaseRepository,
            IBaseRepository<Sancion> sancionBaseRepository,
            IReservaRepository reservaRepository,
            RestauranteContext context,
            IConfiguration configuration)
        {
            _reservaBaseRepository = reservaBaseRepository;
            _clienteBaseRepository = clienteBaseRepository;
            _mesaBaseRepository = mesaBaseRepository;
            _sancionBaseRepository = sancionBaseRepository;
            _reservaRepository = reservaRepository;
            _context = context;
            _configuration = configuration;
        }

        public async Task<IEnumerable<Reserva>> GetAllReservasAsync()
        {
            return await _reservaRepository.GetAllWithDetailsAsync();
        }

        public async Task<Reserva> GetReservaByIdAsync(int id)
        {
            var reserva = await _reservaRepository.GetByIdWithDetailsAsync(id);
            if (reserva == null)
            {
                throw new BusinessException("La reserva no existe.", HttpStatusCode.NotFound);
            }
            return reserva;
        }

        public async Task<Reserva> RegistrarReservaAsync(CrearReservaDto dto)
        {
            // 1. Verificar si el cliente está bloqueado por sanciones vigentes
            var clienteExistente = await _reservaRepository.BuscarClientePorTelefonoOCorreoAsync(
                dto.TelefonoCliente.Trim(),
                string.IsNullOrWhiteSpace(dto.CorreoCliente) ? null : dto.CorreoCliente.Trim());

            var ahora = DateTime.Now;
            if (clienteExistente != null && clienteExistente.BloqueadoHasta.HasValue && clienteExistente.BloqueadoHasta.Value > ahora)
            {
                throw new BusinessException(
                    $"El cliente se encuentra bloqueado para realizar reservas hasta el {clienteExistente.BloqueadoHasta.Value:dd/MM/yyyy HH:mm}.",
                    HttpStatusCode.BadRequest);
            }

            // 2. Verificar que la mesa exista
            var mesa = await _mesaBaseRepository.GetById(dto.MesaId);
            if (mesa == null)
            {
                throw new BusinessException("La mesa seleccionada no existe.", HttpStatusCode.NotFound);
            }

            // 3. Verificar capacidad de la mesa
            if (dto.CantidadPersonas > mesa.Capacidad)
            {
                throw new BusinessException("La mesa no tiene capacidad suficiente.", HttpStatusCode.BadRequest);
            }

            // 4. Verificar cruce de horario
            var existeCruce = await _reservaRepository.ExisteCruceHorarioAsync(
                dto.MesaId,
                dto.FechaHoraInicio,
                dto.FechaHoraFin);

            if (existeCruce)
            {
                throw new BusinessException("La mesa ya tiene una reserva en ese horario.", HttpStatusCode.Conflict);
            }

            // 5. Registrar cliente y reserva en una transacción
            await using var transaccion = await _context.Database.BeginTransactionAsync();
            try
            {
                Cliente cliente;
                if (clienteExistente != null)
                {
                    cliente = clienteExistente;
                    cliente.Nombre = dto.NombreCliente.Trim();
                    if (!string.IsNullOrWhiteSpace(dto.CorreoCliente))
                    {
                        cliente.Correo = dto.CorreoCliente.Trim();
                    }
                    _context.Clientes.Update(cliente);
                }
                else
                {
                    cliente = new Cliente
                    {
                        Nombre = dto.NombreCliente.Trim(),
                        Telefono = dto.TelefonoCliente.Trim(),
                        Correo = string.IsNullOrWhiteSpace(dto.CorreoCliente) ? null : dto.CorreoCliente.Trim()
                    };
                    await _context.Clientes.AddAsync(cliente);
                }

                var reserva = new Reserva
                {
                    Codigo = "RES-" + Guid.NewGuid().ToString("N"),
                    Cliente = cliente,
                    MesaId = mesa.Id,
                    Mesa = mesa,
                    FechaHoraInicio = dto.FechaHoraInicio,
                    FechaHoraFin = dto.FechaHoraFin,
                    CantidadPersonas = dto.CantidadPersonas,
                    Estado = EstadoReserva.Confirmada,
                    FechaRegistro = DateTime.UtcNow
                };

                await _context.Reservas.AddAsync(reserva);
                await _context.SaveChangesAsync();

                // Genera código definitivo con formato RES-XXXX
                reserva.Codigo = $"RES-{reserva.Id:D4}";
                await _context.SaveChangesAsync();

                await transaccion.CommitAsync();

                return reserva;
            }
            catch
            {
                await transaccion.RollbackAsync();
                throw;
            }
        }

        public async Task CancelarReservaAsync(int id, string? descripcion = null)
        {
            var reserva = await _reservaRepository.GetByIdWithDetailsAsync(id);
            if (reserva == null)
            {
                throw new BusinessException("La reserva no existe.", HttpStatusCode.NotFound);
            }

            // Solo se puede cambiar desde Confirmada
            if (reserva.Estado != EstadoReserva.Confirmada)
            {
                throw new BusinessException(
                    $"Solo se puede cancelar una reserva en estado Confirmada. El estado actual es '{reserva.Estado}'.",
                    HttpStatusCode.BadRequest);
            }

            // Evitar sanciones duplicadas para la misma reserva
            var yaTieneSancion = await _reservaRepository.TieneSancionAsync(id) || reserva.Sancion != null;
            if (yaTieneSancion)
            {
                throw new BusinessException("La reserva ya tiene una sanción registrada.", HttpStatusCode.BadRequest);
            }

            // Duración desde appsettings.json (valor inicial 1 día)
            var diasCancelacion = _configuration.GetValue<int>("Sanciones:DiasCancelacion", 1);
            var ahora = DateTime.Now;
            var fechaFin = ahora.AddDays(diasCancelacion);

            await using var transaccion = await _context.Database.BeginTransactionAsync();
            try
            {
                reserva.Estado = EstadoReserva.Cancelada;
                _context.Reservas.Update(reserva);

                var sancion = new Sancion
                {
                    ClienteId = reserva.ClienteId,
                    ReservaId = reserva.Id,
                    Motivo = (int)MotivoSancion.Cancelacion,
                    Descripcion = string.IsNullOrWhiteSpace(descripcion) ? "Cancelación de reserva" : descripcion.Trim(),
                    FechaRegistro = ahora,
                    FechaFin = fechaFin
                };

                await _context.Sanciones.AddAsync(sancion);

                // Actualizar bloqueo del cliente sin reducir un bloqueo existente
                var cliente = reserva.Cliente ?? await _clienteBaseRepository.GetById(reserva.ClienteId);
                if (cliente != null)
                {
                    if (cliente.BloqueadoHasta == null || cliente.BloqueadoHasta.Value < fechaFin)
                    {
                        cliente.BloqueadoHasta = fechaFin;
                        _context.Clientes.Update(cliente);
                    }
                }

                await _context.SaveChangesAsync();
                await transaccion.CommitAsync();
            }
            catch
            {
                await transaccion.RollbackAsync();
                throw;
            }
        }

        public async Task CompletarReservaAsync(int id)
        {
            var reserva = await _reservaRepository.GetByIdWithDetailsAsync(id);
            if (reserva == null)
            {
                throw new BusinessException("La reserva no existe.", HttpStatusCode.NotFound);
            }

            // Solo se puede cambiar desde Confirmada
            if (reserva.Estado != EstadoReserva.Confirmada)
            {
                throw new BusinessException(
                    $"Solo se puede completar una reserva en estado Confirmada. El estado actual es '{reserva.Estado}'.",
                    HttpStatusCode.BadRequest);
            }

            reserva.Estado = EstadoReserva.Completada;
            await _reservaBaseRepository.Update(reserva);
        }

        public async Task RegistrarInasistenciaAsync(int id, string? descripcion = null)
        {
            var reserva = await _reservaRepository.GetByIdWithDetailsAsync(id);
            if (reserva == null)
            {
                throw new BusinessException("La reserva no existe.", HttpStatusCode.NotFound);
            }

            // Solo se puede cambiar desde Confirmada
            if (reserva.Estado != EstadoReserva.Confirmada)
            {
                throw new BusinessException(
                    $"Solo se puede registrar inasistencia en una reserva en estado Confirmada. El estado actual es '{reserva.Estado}'.",
                    HttpStatusCode.BadRequest);
            }

            // Una inasistencia se registra después de finalizar la reserva
            var ahora = DateTime.Now;
            if (ahora < reserva.FechaHoraFin)
            {
                throw new BusinessException(
                    "La inasistencia solo se puede registrar después de finalizar la reserva.",
                    HttpStatusCode.BadRequest);
            }

            // Evitar sanciones duplicadas para la misma reserva
            var yaTieneSancion = await _reservaRepository.TieneSancionAsync(id) || reserva.Sancion != null;
            if (yaTieneSancion)
            {
                throw new BusinessException("La reserva ya tiene una sanción registrada.", HttpStatusCode.BadRequest);
            }

            // Duración desde appsettings.json (valor inicial 7 días)
            var diasInasistencia = _configuration.GetValue<int>("Sanciones:DiasInasistencia", 7);
            var fechaFin = ahora.AddDays(diasInasistencia);

            await using var transaccion = await _context.Database.BeginTransactionAsync();
            try
            {
                reserva.Estado = EstadoReserva.NoAsistio;
                _context.Reservas.Update(reserva);

                var sancion = new Sancion
                {
                    ClienteId = reserva.ClienteId,
                    ReservaId = reserva.Id,
                    Motivo = (int)MotivoSancion.Inasistencia,
                    Descripcion = string.IsNullOrWhiteSpace(descripcion) ? "Inasistencia a reserva" : descripcion.Trim(),
                    FechaRegistro = ahora,
                    FechaFin = fechaFin
                };

                await _context.Sanciones.AddAsync(sancion);

                // Actualizar bloqueo del cliente sin reducir un bloqueo existente
                var cliente = reserva.Cliente ?? await _clienteBaseRepository.GetById(reserva.ClienteId);
                if (cliente != null)
                {
                    if (cliente.BloqueadoHasta == null || cliente.BloqueadoHasta.Value < fechaFin)
                    {
                        cliente.BloqueadoHasta = fechaFin;
                        _context.Clientes.Update(cliente);
                    }
                }

                await _context.SaveChangesAsync();
                await transaccion.CommitAsync();
            }
            catch
            {
                await transaccion.RollbackAsync();
                throw;
            }
        }
    }
}
