using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurante.Core.Entities;

namespace Restaurante.Infrastructure.Data.Configurations
{
    public class SancionConfiguration : IEntityTypeConfiguration<Sancion>
    {
        public void Configure(EntityTypeBuilder<Sancion> entity)
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("sanciones");

            // Índice único para evitar sanciones duplicadas por reserva
            entity.HasIndex(e => e.ReservaId, "UQ_Sanciones_ReservaId")
                .IsUnique();

            // Índice para cliente
            entity.HasIndex(e => e.ClienteId, "IX_Sanciones_ClienteId");

            entity.Property(e => e.Motivo)
                .IsRequired();

            entity.Property(e => e.Descripcion)
                .IsRequired(false)
                .HasMaxLength(300);

            entity.Property(e => e.FechaRegistro)
                .IsRequired()
                .HasColumnType("datetime(6)");

            entity.Property(e => e.FechaFin)
                .IsRequired()
                .HasColumnType("datetime(6)");

            // Relación con Cliente
            entity.HasOne(e => e.Cliente)
                .WithMany(e => e.Sanciones)
                .HasForeignKey(e => e.ClienteId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Sanciones_Clientes");

            // Relación con Reserva
            entity.HasOne(e => e.Reserva)
                .WithOne(e => e.Sancion)
                .HasForeignKey<Sancion>(e => e.ReservaId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Sanciones_Reservas");
        }
    }
}

