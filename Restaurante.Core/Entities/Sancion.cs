using System;

namespace Restaurante.Core.Entities
{
    public partial class Sancion : BaseEntity
    {
        public int ClienteId { get; set; }
        public int ReservaId { get; set; }
        public MotivoSancion Motivo { get; set; }
        public int NumeroFalta { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime? FechaFin { get; set; }

        public virtual Cliente Cliente { get; set; } = null!;
        public virtual Reserva Reserva { get; set; } = null!;
    }
}
