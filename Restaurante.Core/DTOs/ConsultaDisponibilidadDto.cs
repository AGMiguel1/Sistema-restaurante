using System;

namespace Restaurante.Core.DTOs
{
    // Datos necesarios para consultar las mesas disponibles.
    public class ConsultaDisponibilidadDto
    {
        // Fecha y hora de inicio solicitada, en horario de Bolivia.
        public DateTime FechaHoraInicio { get; set; }

        // Fecha y hora de finalización solicitada, en horario de Bolivia.
        public DateTime FechaHoraFin { get; set; }

        // Cantidad de personas que ocuparán la mesa; debe ser mayor a cero.
        public int CantidadPersonas { get; set; }
    }
}