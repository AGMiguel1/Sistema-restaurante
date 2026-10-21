using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurante.Core.Entities;

namespace Restaurante.Infrastructure.Data.Configurations
{
    public class MesaConfiguration : IEntityTypeConfiguration<Mesa>
    {
        public void Configure(EntityTypeBuilder<Mesa> entity)
        {
            // Clave primaria de la mesa
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("mesas");

            // El numero de mesa no puede repetirse
            entity.HasIndex(e => e.Numero)
                .IsUnique();

            entity.Property(e => e.Numero)
                .IsRequired();

            // Cantidad max de personas
            entity.Property(e => e.Capacidad)
                .IsRequired();

            // Una mesa puede tener varias reservas
            entity.HasMany(e => e.Reservas)
                .WithOne(e => e.Mesa)
                .HasForeignKey(e => e.MesaId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}