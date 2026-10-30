namespace Restaurante.Core.DTOs
{
    // Datos requeridos para registrar la cancelación de una reserva.
    public class CancelarReservaDto
    {
        // Quién realiza o solicita la cancelación (cliente, recepcionista, etc.).
        public string CanceladaPor { get; set; } = string.Empty;

        // Motivo opcional de la cancelación.
        public string? Motivo { get; set; }
    }
}
