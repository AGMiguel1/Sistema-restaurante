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

            entity.HasIndex(e => new { e.ReservaId, e.Motivo })
                .IsUnique()
                .HasDatabaseName("UQ_Sanciones_Reserva_Motivo");

            entity.HasIndex(e => e.ClienteId)
                .HasDatabaseName("FK_Sanciones_Clientes");

            entity.Property(e => e.Motivo)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(e => e.NumeroFalta)
                .IsRequired()
                .HasDefaultValue(0);

            entity.Property(e => e.FechaRegistro)
                .IsRequired()
                .HasColumnType("datetime(6)");

            entity.Property(e => e.FechaFin)
                .HasColumnType("datetime(6)");
        }
    }
}
