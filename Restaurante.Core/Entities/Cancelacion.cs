using System;

namespace Restaurante.Core.Entities
{
    public partial class Cancelacion : BaseEntity
    {
        public int ReservaId { get; set; }
        public DateTime FechaCancelacion { get; set; }
        public string CanceladaPor { get; set; } = null!;
        public decimal HorasAnticipacion { get; set; }
        public decimal PorcentajeAplicado { get; set; }
        public decimal MontoReembolso { get; set; }
        public string? Motivo { get; set; }

        public virtual Reserva Reserva { get; set; } = null!;
    }
}
