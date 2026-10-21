using System;

namespace Restaurante.Core.DTOs
{
    // Datos necesarios para registrar una reserva.
    public class CrearReservaDto
    {
        // Identificador de la mesa que se desea reservar.
        public int MesaId { get; set; }

        // Nombre de la persona que realiza la reserva.
        public string NombreCliente { get; set; } = string.Empty;

        // Teléfono de contacto del cliente.
        public string TelefonoCliente { get; set; } = string.Empty;

        // Correo electrónico del cliente; es opcional.
        public string? CorreoCliente { get; set; }

        // Fecha y hora de inicio, en horario de Bolivia.
        public DateTime FechaHoraInicio { get; set; }

        // Fecha y hora de finalización, en horario de Bolivia.
        public DateTime FechaHoraFin { get; set; }

        // Cantidad de personas que asistirán.
        public int CantidadPersonas { get; set; }
    }
}