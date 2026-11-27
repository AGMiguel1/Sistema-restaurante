
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurante.Core.Entities;

namespace Restaurante.Infrastructure.Data.Configurations
{
    public class UsuarioConfiguration:IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> entity)
        {
            entity.HasKey(x => x.Id).HasName("PRIMARY");
            entity.ToTable("usuarios");
            entity.HasIndex(x => x.Correo).IsUnique().HasDatabaseName("UQ_Usuarios_Correo");
            entity.Property(e => e.Correo)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(e => e.Password)
                .HasColumnName("PasswordHash")
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.Rol)
                .IsRequired()
                .HasMaxLength(30);

            entity.Property(e => e.Activo)
                .IsRequired()
                .HasDefaultValue(true);

            entity.HasOne(e => e.Cliente)
                .WithOne()
                .HasForeignKey<Usuario>(e => e.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
