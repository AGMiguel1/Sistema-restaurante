namespace Restaurante.Core.Entities;


public enum EstadoReserva
{
    //La reserva fue creada y confirmada exitosamente y bloquea el horario de la mesa
    Confirmada = 1,
    //La reserva fue cancelada y no bloquea la disponibilidad de la mesa
    Cancelada = 2,
    //El cliente asistio
    Completada = 3
}
