using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurante.Core.Entities;

namespace Restaurante.Infrastructure.Data.Configurations
{
    public class ReservaConfiguration : IEntityTypeConfiguration<Reserva>
    {
        public void Configure(EntityTypeBuilder<Reserva> entity)
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
            entity.ToTable("reservas");

            entity.HasIndex(e => e.Codigo)
                .IsUnique();

            entity.Property(e => e.Codigo)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.FechaHoraInicio)
                .IsRequired()
                .HasColumnType("datetime(6)");

            entity.Property(e => e.FechaHoraFin)
                .IsRequired()
                .HasColumnType("datetime(6)");

            entity.Property(e => e.CantidadPersonas)
                .IsRequired();

            entity.Property(e => e.Estado)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(e => e.FechaRegistro)
                .IsRequired()
                .HasColumnType("datetime(6)");

            entity.HasIndex(e => new
            {
                e.MesaId,
                e.Estado,
                e.FechaHoraInicio,
                e.FechaHoraFin
            });

            entity.HasOne(e => e.Cliente)
                .WithMany(e => e.Reservas)
                .HasForeignKey(e => e.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Mesa)
                .WithMany(e => e.Reservas)
                .HasForeignKey(e => e.MesaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Cancelacion)
                .WithOne(e => e.Reserva)
                .HasForeignKey<Cancelacion>(e => e.ReservaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.Sanciones)
                .WithOne(e => e.Reserva)
                .HasForeignKey(e => e.ReservaId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
