namespace Restaurante.Core.Entities;


public enum EstadoReserva
{
    /// La reserva fue creada y confirmada exitosamente. Bloquea el horario de la mesa
    Confirmada = 1,

    /// La reserva fue cancelada. No bloquea la disponibilidad de la mesa.
    Cancelada = 2,
    /// El cliente asistió y completó su estancia en el restaurante.
    Completada = 3
}
