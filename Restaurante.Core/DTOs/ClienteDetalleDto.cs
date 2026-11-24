namespace Restaurante.Core.DTOs;

public class ClienteDetalleDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string? Correo { get; set; }
    public int ContadorSanciones { get; set; }
    public bool Bloqueado { get; set; }
    public DateTime? FechaBloqueo { get; set; }
    public DateTime? BloqueadoHasta { get; set; }
}
