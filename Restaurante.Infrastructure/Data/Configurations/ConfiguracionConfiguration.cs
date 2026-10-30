using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurante.Core.Entities;

namespace Restaurante.Infrastructure.Data.Configurations
{
    public class ConfiguracionConfiguration : IEntityTypeConfiguration<Configuracion>
    {
        public void Configure(EntityTypeBuilder<Configuracion> entity)
        {
            entity.HasKey(e => e.Clave).HasName("PRIMARY");
            entity.ToTable("configuraciones");

            entity.Property(e => e.Clave)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Valor)
                .IsRequired()
                .HasMaxLength(200);
        }
    }
}
