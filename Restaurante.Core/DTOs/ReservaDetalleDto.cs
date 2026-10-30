using System;

namespace Restaurante.Core.DTOs
{
    // Datos de confirmación y detalle de una reserva.
    public class ReservaDetalleDto
    {
        // Identificador único de la reserva.
        public int Id { get; set; }

        // Código único generado para identificar la reserva.
        public string Codigo { get; set; } = string.Empty;

        // Nombre del cliente que realizó la reserva.
        public string NombreCliente { get; set; } = string.Empty;

        // Teléfono del cliente.
        public string TelefonoCliente { get; set; } = string.Empty;

        // Número de la mesa reservada.
        public int NumeroMesa { get; set; }

        // Fecha y hora de inicio, en horario local.
        public DateTime FechaHoraInicio { get; set; }

        // Fecha y hora de finalización, en horario local.
        public DateTime FechaHoraFin { get; set; }

        // Cantidad de personas que asistirán.
        public int CantidadPersonas { get; set; }

        // Nombre del estado de la reserva, por ejemplo: Confirmada, Cancelada, etc.
        public string Estado { get; set; } = string.Empty;

        // Fecha en que se registró la reserva.
        public DateTime FechaRegistro { get; set; }
    }
}
