using System;

namespace Restaurante.Core.DTOs
{
    public class CrearReservaDto
    {
        // Identificador de la mesa que se desea reservar
        public int MesaId { get; set; }

        // Nombre de la persona que realiza la reserva
        public string NombreCliente { get; set; } = string.Empty;

        // Teléfono del cliente
        public string TelefonoCliente { get; set; } = string.Empty;

        // Correo electrónico del cliente que es opcional
        public string? CorreoCliente { get; set; }

        // Fecha y hora de inicio
        public DateTime FechaHoraInicio { get; set; }

        // Fecha y hora de finalización
        public DateTime FechaHoraFin { get; set; }

        // Cantidad de personas 
        public int CantidadPersonas { get; set; }
    }
}