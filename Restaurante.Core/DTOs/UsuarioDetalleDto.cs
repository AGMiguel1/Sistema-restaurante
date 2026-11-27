namespace Restaurante.Core.DTOs;

public record UsuarioDetalleDto(int Id, int? ClienteId, string Correo, string Rol);
