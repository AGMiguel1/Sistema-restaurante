namespace Restaurante.Core.DTOs.Reporteria;

public class ClienteFrecuenteDto
{
    public int ClienteId { get; set; }
    public string Nombre { get; set; } = null!;
    public long TotalReservas { get; set; }
}
