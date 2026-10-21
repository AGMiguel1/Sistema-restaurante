using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurante.Core.Entities;

namespace Restaurante.Infrastructure.Data.Configurations
{
    public class ReservaConfiguration : IEntityTypeConfiguration<Reserva>
    {
        public void Configure(EntityTypeBuilder<Reserva> entity)
        {
            // Clave primaria de la reserva
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("reservas");

            // El codigo de reserva no puede repetirse
            entity.HasIndex(e => e.Codigo)
                .IsUnique();

            entity.Property(e => e.Codigo)
                .IsRequired()
                .HasMaxLength(50);

            // Inicio y fin 
            entity.Property(e => e.FechaHoraInicio)
                .IsRequired()
                .HasColumnType("datetime(6)");

            entity.Property(e => e.FechaHoraFin)
                .IsRequired()
                .HasColumnType("datetime(6)");

            // Cantidad de personas que asistiran
            entity.Property(e => e.CantidadPersonas)
                .IsRequired();

            // Guarda el estado como nuemro: 1, 2 o 3
            entity.Property(e => e.Estado)
                .IsRequired()
                .HasConversion<int>();

            // Momento del registro guardado en tiempo del mundo
            entity.Property(e => e.FechaRegistro)
                .IsRequired()
                .HasColumnType("datetime(6)");

            // Índice para las consultas de disponibilidad.
            entity.HasIndex(e => new
            {
                e.MesaId,
                e.Estado,
                e.FechaHoraInicio,
                e.FechaHoraFin
            });

            // Relación de la reserva con el cliente.
            entity.HasOne(e => e.Cliente)
                .WithMany(e => e.Reservas)
                .HasForeignKey(e => e.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación de la reserva con la mesa.
            entity.HasOne(e => e.Mesa)
                .WithMany(e => e.Reservas)
                .HasForeignKey(e => e.MesaId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}