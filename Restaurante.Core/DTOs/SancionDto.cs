using System;

namespace Restaurante.Core.DTOs
{
    public class SancionDto
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public int ReservaId { get; set; }
        public int Motivo { get; set; }
        public string MotivoTexto { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime FechaFin { get; set; }
    }
}

