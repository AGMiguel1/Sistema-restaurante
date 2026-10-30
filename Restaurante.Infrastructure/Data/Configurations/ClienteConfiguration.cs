using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurante.Core.Entities;

namespace Restaurante.Infrastructure.Data.Configurations
{
    public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
    {
        public void Configure(EntityTypeBuilder<Cliente> entity)
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
            entity.ToTable("clientes");

            entity.HasIndex(e => e.Telefono)
                .IsUnique()
                .HasDatabaseName("UQ_Clientes_Telefono");

            entity.Property(e => e.Nombre)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(e => e.Telefono)
                .IsRequired()
                .HasMaxLength(30);

            entity.Property(e => e.Correo)
                .IsRequired(false)
                .HasMaxLength(150);

            entity.Property(e => e.ContadorSanciones)
                .IsRequired()
                .HasDefaultValue(0);

            entity.Property(e => e.Bloqueado)
                .IsRequired()
                .HasDefaultValue(false);

            entity.Property(e => e.FechaBloqueo)
                .HasColumnType("datetime(6)");

            entity.Property(e => e.BloqueadoHasta)
                .HasColumnType("datetime(6)");

            entity.HasMany(e => e.Reservas)
                .WithOne(e => e.Cliente)
                .HasForeignKey(e => e.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.Sanciones)
                .WithOne(e => e.Cliente)
                .HasForeignKey(e => e.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
