using System;
using System.Collections.Generic;

namespace Restaurante.Core.Entities;

public partial class Reserva : BaseEntity 
{
    // Identificador único de la reserva.
    //public int Id { get; set; }

    // Código único para identificar la reserva.
    public string Codigo { get; set; } = null!;

    // Identificador del cliente que realiza la reserva.
    public int ClienteId { get; set; }

    // Identificador de la mesa reservada.
    public int MesaId { get; set; }

    // Fecha y hora de inicio en horario local de Bolivia.
    public DateTime FechaHoraInicio { get; set; }

    // Fecha y hora de finalización en horario local de Bolivia.
    public DateTime FechaHoraFin { get; set; }

    // Cantidad de personas que ocuparán la mesa.
    public int CantidadPersonas { get; set; }

    // Estado de la reserva; al crearla queda Confirmada.
    public EstadoReserva Estado { get; set; }
        = EstadoReserva.Confirmada;

    // Momento en que se registra la reserva, guardado en UTC.
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    // Relación con el cliente que realizó la reserva.
    public virtual Cliente Cliente { get; set; } = null!;

    // Relación con la mesa asignada a la reserva.
    public virtual Mesa Mesa { get; set; } = null!;

    // Registro de cancelación en caso de que se cancele.
    public virtual Cancelacion? Cancelacion { get; set; }

    // Sanciones asociadas a esta reserva.
    public virtual ICollection<Sancion> Sanciones { get; set; }
        = new List<Sancion>();
}
