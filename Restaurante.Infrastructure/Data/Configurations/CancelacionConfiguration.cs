using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurante.Core.Entities;

namespace Restaurante.Infrastructure.Data.Configurations
{
    public class CancelacionConfiguration : IEntityTypeConfiguration<Cancelacion>
    {
        public void Configure(EntityTypeBuilder<Cancelacion> entity)
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
            entity.ToTable("cancelaciones");

            entity.HasIndex(e => e.ReservaId)
                .IsUnique()
                .HasDatabaseName("UQ_Cancelaciones_ReservaId");

            entity.Property(e => e.FechaCancelacion)
                .IsRequired()
                .HasColumnType("datetime(6)");

            entity.Property(e => e.CanceladaPor)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.HorasAnticipacion)
                .IsRequired()
                .HasPrecision(8, 2);

            entity.Property(e => e.PorcentajeAplicado)
                .IsRequired()
                .HasPrecision(5, 2);

            entity.Property(e => e.MontoReembolso)
                .IsRequired()
                .HasPrecision(10, 2)
                .HasDefaultValue(0.00m);

            entity.Property(e => e.Motivo)
                .HasMaxLength(300);
        }
    }
}
