using System;
using System.Collections.Generic;

namespace Restaurante.Core.Entities;

public partial class Cliente : BaseEntity
{
    // public int Id { get; set; } // Heredado de BaseEntity

    public string Nombre { get; set; } = null!;

    public string Telefono { get; set; } = null!;

    public string? Correo { get; set; }

    // Fecha y hora hasta la que el cliente está bloqueado para nuevas reservas
    public DateTime? BloqueadoHasta { get; set; }

    // Reservas asociadas al cliente
    public virtual ICollection<Reserva> Reservas { get; set; }
        = new List<Reserva>();

    // Sanciones aplicadas al cliente
    public virtual ICollection<Sancion> Sanciones { get; set; }
        = new List<Sancion>();
}
