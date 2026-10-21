using System;
using System.Collections.Generic;

namespace Restaurante.Core.Entities;

public partial class Cliente
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string Telefono { get; set; } = null!;

    public string? Correo { get; set; }

    // Reservas asociadas al cliente.
    public virtual ICollection<Reserva> Reservas { get; set; }
        = new List<Reserva>();
}