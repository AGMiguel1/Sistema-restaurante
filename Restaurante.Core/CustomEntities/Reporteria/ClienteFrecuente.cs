namespace Restaurante.Core.CustomEntities.Reporteria;

public class ClienteFrecuente
{
    public int ClienteId { get; set; }
    public string Nombre { get; set; } = null!;
    public long TotalReservas { get; set; }
}
