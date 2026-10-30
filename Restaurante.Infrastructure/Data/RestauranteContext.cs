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
    public virtual DbSet<Cancelacion> Cancelaciones { get; set; }
    public virtual DbSet<Configuracion> Configuraciones { get; set; }
    public virtual DbSet<EscalaSancion> EscalasSancion { get; set; }
    public virtual DbSet<PoliticaReembolso> PoliticasReembolso { get; set; }
    public virtual DbSet<Sancion> Sanciones { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Carga las configuraciones de entidades de Infrastructure.
        modelBuilder.ApplyConfigurationsFromAssembly(
            Assembly.GetExecutingAssembly());

        // Nombres de las tablas en MySQL Workbench.
        modelBuilder.Entity<Cliente>().ToTable("clientes");
        modelBuilder.Entity<Mesa>().ToTable("mesas");
        modelBuilder.Entity<Reserva>().ToTable("reservas");
        modelBuilder.Entity<Cancelacion>().ToTable("cancelaciones");
        modelBuilder.Entity<Configuracion>().ToTable("configuraciones");
        modelBuilder.Entity<EscalaSancion>().ToTable("escalas_sancion");
        modelBuilder.Entity<PoliticaReembolso>().ToTable("politicas_reembolso");
        modelBuilder.Entity<Sancion>().ToTable("sanciones");
    }
}
