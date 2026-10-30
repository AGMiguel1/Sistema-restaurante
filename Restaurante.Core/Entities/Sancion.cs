using System;

namespace Restaurante.Core.Entities;

public partial class Sancion : BaseEntity
{
    // Id heredado de BaseEntity

    public int ClienteId { get; set; }

    public int ReservaId { get; set; }

    public int Motivo { get; set; }

    public string? Descripcion { get; set; }

    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public DateTime FechaFin { get; set; }

    // Relación con el cliente sancionado
    public virtual Cliente Cliente { get; set; } = null!;

    // Relación con la reserva que originó la sanción
    public virtual Reserva Reserva { get; set; } = null!;
}

