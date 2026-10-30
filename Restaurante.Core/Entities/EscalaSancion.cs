namespace Restaurante.Core.Entities
{
    public partial class EscalaSancion
    {
        public int NumeroFalta { get; set; }
        public string Descripcion { get; set; } = null!;
        public bool Bloquea { get; set; }
        public int? DuracionDias { get; set; }
    }
}
