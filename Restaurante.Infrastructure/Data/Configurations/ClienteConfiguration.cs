using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurante.Core.Entities;

namespace Restaurante.Infrastructure.Data.Configurations
{
    public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
    {
        public void Configure(EntityTypeBuilder<Cliente> entity)
        {
            // Clave primaria del cliente.
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("clientes");

            // Nombre y teléfono obligatorios.
            entity.Property(e => e.Nombre)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(e => e.Telefono)
                .IsRequired()
                .HasMaxLength(30);

            // El correo es opcional.
            entity.Property(e => e.Correo)
                .IsRequired(false)
                .HasMaxLength(150);

            // Un cliente puede tener varias reservas.
            // Impide eliminar un cliente que tenga reservas.
            entity.HasMany(e => e.Reservas)
                .WithOne(e => e.Cliente)
                .HasForeignKey(e => e.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}