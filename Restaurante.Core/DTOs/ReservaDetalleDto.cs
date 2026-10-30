using System;

namespace Restaurante.Core.DTOs
{
    public class ReservaDetalleDto
    {
        // id reserva
        public int Id { get; set; }

        // Código único generado para identificar la reserva
        public string Codigo { get; set; } = string.Empty;

        // Nombre del cliente que realizó la reserva
        public string NombreCliente { get; set; } = string.Empty;

        // Número de la mesa reservada
        public int NumeroMesa { get; set; }

        // Fecha y hora de inicio
        public DateTime FechaHoraInicio { get; set; }

        // Fecha y hora de fin
        public DateTime FechaHoraFin { get; set; }

        // Cantidad de personas que asistirán
        public int CantidadPersonas { get; set; }

        // Nombre del estado de la reserva confirmada
        public string Estado { get; set; } = string.Empty;
    }
}