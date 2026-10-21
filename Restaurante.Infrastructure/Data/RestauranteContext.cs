using Microsoft.EntityFrameworkCore;
using Restaurante.Core.Entities;
using System.Reflection;

namespace Restaurante.Infrastructure.Data;

public partial class RestauranteContext : DbContext
{
    public RestauranteContext()
    {
    }

    public RestauranteContext(
        DbContextOptions<RestauranteContext> options)
        : base(options)
    {
    }

    // Tablas de la base de datos.
    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<Mesa> Mesas { get; set; }

    public virtual DbSet<Reserva> Reservas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Carga las configuraciones de entidades de Infrastructure.
        modelBuilder.ApplyConfigurationsFromAssembly(
            Assembly.GetExecutingAssembly());

        // Nombres de las tablas que aparecen en tu MySQL Workbench.
        modelBuilder.Entity<Cliente>().ToTable("clientes");
        modelBuilder.Entity<Mesa>().ToTable("mesas");
        modelBuilder.Entity<Reserva>().ToTable("reservas");
    }
}