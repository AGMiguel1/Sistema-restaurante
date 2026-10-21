using System;

namespace Restaurante.Core.Entities;

public partial class Reserva
{
    // id resevra
    public int Id { get; set; }

    // codigo para identificar la reserva
    public string Codigo { get; set; } = null!;

    // id del cliente que realiza la reserva
    public int ClienteId { get; set; }

    // id de la mesa reservada
    public int MesaId { get; set; }

    // Fecha y hora de inicio 
    public DateTime FechaHoraInicio { get; set; }

    // Fecha y hora de fin
    public DateTime FechaHoraFin { get; set; }

    // Cantidad de personas que ocuparán la mesa
    public int CantidadPersonas { get; set; }

    // Estado de la reserva al crearla queda Confirmada
    public EstadoReserva Estado { get; set; }
        = EstadoReserva.Confirmada;

    // Momento en que se registra la reserva,
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    // Relación con el cliente que realizó la reserva
    public virtual Cliente Cliente { get; set; } = null!;

    // Relación con la mesa asignada a la reserva
    public virtual Mesa Mesa { get; set; } = null!;
}