using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurante.Core.Entities;

namespace Restaurante.Infrastructure.Data.Configurations
{
    public class PoliticaReembolsoConfiguration : IEntityTypeConfiguration<PoliticaReembolso>
    {
        public void Configure(EntityTypeBuilder<PoliticaReembolso> entity)
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
            entity.ToTable("politicas_reembolso");

            entity.Property(e => e.HorasMinimasAnticipacion)
                .IsRequired();

            entity.Property(e => e.PorcentajeReembolso)
                .IsRequired()
                .HasPrecision(5, 2);

            entity.Property(e => e.Activa)
                .IsRequired()
                .HasDefaultValue(true);
        }
    }
}
