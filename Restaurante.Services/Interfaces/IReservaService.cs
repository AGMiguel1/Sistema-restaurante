using Restaurante.Core.DTOs;
using Restaurante.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Restaurante.Services.Interfaces
{
    public interface IReservaService
    {
        // CU-03: Registrar reserva con verificación de bloqueo de cliente y mesa.
        Task<ReservaDetalleDto> RegistrarReservaAsync(
            CrearReservaDto dto,
            CancellationToken cancellationToken = default);

        // CU-04: Modificar reserva confirmada.
        Task<ReservaDetalleDto> ModificarReservaAsync(
            string codigo,
            ModificarReservaDto dto,
            CancellationToken cancellationToken = default);

        // CU-05: Buscar reserva por código único.
        Task<ReservaDetalleDto> ObtenerPorCodigoAsync(
            string codigo,
            CancellationToken cancellationToken = default);

        // CU-05: Listado de reservas con filtros opcionales.
        Task<IEnumerable<ReservaDetalleDto>> ConsultarReservasAsync(
            DateTime? fecha,
            EstadoReserva? estado,
            string? telefono,
            CancellationToken cancellationToken = default);

        // Cancelación anticipada y cálculo de reembolso.
        Task<ResultadoCancelacionDto> CancelarReservaAsync(
            string codigo,
            CancelarReservaDto dto,
            CancellationToken cancellationToken = default);

        // Registrar inasistencia del cliente, aplicando escala de sanción y bloqueo.
        Task<ReservaDetalleDto> RegistrarInasistenciaAsync(
            string codigo,
            CancellationToken cancellationToken = default);
    }
}
