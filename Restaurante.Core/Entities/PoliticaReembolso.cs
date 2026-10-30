namespace Restaurante.Core.Entities
{
    public partial class PoliticaReembolso : BaseEntity
    {
        public int HorasMinimasAnticipacion { get; set; }
        public decimal PorcentajeReembolso { get; set; }
        public bool Activa { get; set; } = true;
    }
}
