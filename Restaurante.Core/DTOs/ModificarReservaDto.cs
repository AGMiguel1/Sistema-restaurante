using System;

namespace Restaurante.Core.DTOs
{
    // Datos necesarios para modificar una reserva existente (CU-04).
    public class ModificarReservaDto
    {
        // Identificador de la mesa (puede ser la misma o una nueva).
        public int MesaId { get; set; }

        // Nueva fecha y hora de inicio.
        public DateTime FechaHoraInicio { get; set; }

        // Nueva fecha y hora de fin.
        public DateTime FechaHoraFin { get; set; }

        // Nueva cantidad de comensales.
        public int CantidadPersonas { get; set; }
    }
}
