using System;

namespace Restaurante.Core.DTOs
{
    public class ConsultaDisponibilidadDto
    {
        // Fecha y hora de inicio solicitada
        public DateTime FechaHoraInicio { get; set; }

        // Fecha y hora de finalización solicitada
        public DateTime FechaHoraFin { get; set; }

        // Cantidad de personas que ocuparán la mesa y debe ser mayor a cero
        public int CantidadPersonas { get; set; }
    }
}