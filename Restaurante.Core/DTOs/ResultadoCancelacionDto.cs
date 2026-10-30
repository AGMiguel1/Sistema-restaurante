using System;

namespace Restaurante.Core.DTOs
{
    // Detalle de la cancelación y del reembolso calculado.
    public class ResultadoCancelacionDto
    {
        public string CodigoReserva { get; set; } = string.Empty;
        public DateTime FechaCancelacion { get; set; }
        public string CanceladaPor { get; set; } = string.Empty;
        public decimal HorasAnticipacion { get; set; }
        public decimal PorcentajeReembolso { get; set; }
        public decimal MontoReembolso { get; set; }
        public string Mensaje { get; set; } = string.Empty;
    }
}
