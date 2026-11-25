namespace Restaurante.Core.DTOs;

public class GuardarClienteDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string? Correo { get; set; }
}
