using System;
using System.Collections.Generic;

namespace Restaurante.Core.Entities;

public partial class Cliente : BaseEntity
{
    //public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string Telefono { get; set; } = null!;

    public string? Correo { get; set; }
    public int ContadorSanciones { get; set; }
    public bool Bloqueado { get; set; }
    public DateTime? FechaBloqueo { get; set; }
    // Si no es null, el cliente está suspendido hasta esa fecha (UTC).
    public DateTime? BloqueadoHasta { get; set; }
    // true si el bloqueo es permanente o la suspensión aún no venció.
    public bool EstaBloqueado(DateTime ahoraUtc) =>
        Bloqueado || (BloqueadoHasta.HasValue && BloqueadoHasta.Value > ahoraUtc);

    // Reservas asociadas al cliente.
    public virtual ICollection<Reserva> Reservas { get; set; }
        = new List<Reserva>();

    // Sanciones aplicadas al cliente.
    public virtual ICollection<Sancion> Sanciones { get; set; }
        = new List<Sancion>();
}
