using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurante.Core.Entities;

namespace Restaurante.Infrastructure.Data.Configurations
{
    public class EscalaSancionConfiguration : IEntityTypeConfiguration<EscalaSancion>
    {
        public void Configure(EntityTypeBuilder<EscalaSancion> entity)
        {
            entity.HasKey(e => e.NumeroFalta).HasName("PRIMARY");
            entity.ToTable("escalas_sancion");

            entity.Property(e => e.NumeroFalta)
                .ValueGeneratedNever();

            entity.Property(e => e.Descripcion)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Bloquea)
                .IsRequired();

            entity.Property(e => e.DuracionDias);
        }
    }
}
