using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Restaurante.Api.Controllers;
using Restaurante.Api.Filters;
using Restaurante.Services.Interfaces;
using System.Text;
using System.Text.Json;
using AutoMapper;
using FluentValidation;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Restaurante.Core.DTOs;
using Restaurante.Core.Entities;
using Restaurante.Core.Exceptions;
using Restaurante.Infrastructure.Data;
using Restaurante.Infrastructure.Mappings;
using Restaurante.Infrastructure.Repositories;
using Restaurante.Services.Services;
using Restaurante.Services.Validators;
using System.Net;
using Xunit;

namespace Restaurante.Tests;

public class CrudTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly RestauranteContext _context;
    private readonly MesaService _mesas;
    private readonly ClienteService _clientes;

    public CrudTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        _context = new RestauranteContext(new DbContextOptionsBuilder<RestauranteContext>()
            .UseSqlite(_connection).Options);
        _context.Database.EnsureCreated();
        var mapper = new MapperConfiguration(c => c.AddProfile<MesaProfile>()).CreateMapper();
        _mesas = new MesaService(new MesaRepository(_context), mapper, _context, new GuardarMesaValidator(), new BaseRepository<Mesa>(_context));
        _clientes = new ClienteService(new BaseRepository<Cliente>(_context), _context, new GuardarClienteValidator());
    }

    [Fact]
    public async Task Mesas_CicloCrud()
    {
        var mesa = await _mesas.CrearAsync(new() { Numero = 1, Capacidad = 4 });
        Assert.True(mesa.Id > 0);
        Assert.Equal(1, (await _mesas.ObtenerPorIdAsync(mesa.Id)).Numero);
        Assert.Single(await _mesas.ObtenerTodosAsync());
        await _mesas.ActualizarAsync(mesa.Id, new() { Numero = 2, Capacidad = 6 });
        _context.ChangeTracker.Clear();
        var actual = await _mesas.ObtenerPorIdAsync(mesa.Id);
        Assert.Equal(2, actual.Numero);
        Assert.Equal(6, actual.Capacidad);
        await _mesas.EliminarAsync(mesa.Id);
        Assert.Empty(await _mesas.ObtenerTodosAsync());
        await Estado(HttpStatusCode.NotFound, () => _mesas.ObtenerPorIdAsync(mesa.Id));
    }

    [Fact]
    public async Task Clientes_CicloCrud_NormalizaDatos()
    {
        var cliente = await _clientes.CrearAsync(new() { Nombre = " Ana ", Telefono = " 77712345 ", Correo = " " });
        Assert.True(cliente.Id > 0);
        Assert.Equal("Ana", cliente.Nombre);
        Assert.Null(cliente.Correo);
        Assert.Single(await _clientes.ObtenerTodosAsync());
        await _clientes.ActualizarAsync(cliente.Id, new() { Nombre = "Maria", Telefono = "77712346", Correo = "maria@example.com" });
        _context.ChangeTracker.Clear();
        var actual = await _clientes.ObtenerPorIdAsync(cliente.Id);
        Assert.Equal("Maria", actual.Nombre);
        Assert.Equal("77712346", actual.Telefono);
        Assert.Equal("maria@example.com", actual.Correo);
        await _clientes.EliminarAsync(cliente.Id);
        Assert.Empty(await _clientes.ObtenerTodosAsync());
        await Estado(HttpStatusCode.NotFound, () => _clientes.ObtenerPorIdAsync(cliente.Id));
    }

    [Fact]
    public async Task Duplicados_DevuelvenConflicto_YPermitenActualizarMismoRegistro()
    {
        var mesa = await _mesas.CrearAsync(new() { Numero = 1, Capacidad = 4 });
        var otra = await _mesas.CrearAsync(new() { Numero = 2, Capacidad = 4 });
        await Estado(HttpStatusCode.Conflict, () => _mesas.CrearAsync(new() { Numero = 1, Capacidad = 2 }));
        await Estado(HttpStatusCode.Conflict, () => _mesas.ActualizarAsync(otra.Id, new() { Numero = 1, Capacidad = 2 }));
        await _mesas.ActualizarAsync(mesa.Id, new() { Numero = 1, Capacidad = 5 });
        var cliente = await _clientes.CrearAsync(new() { Nombre = "Ana", Telefono = "777" });
        var otro = await _clientes.CrearAsync(new() { Nombre = "Luis", Telefono = "888" });
        await Estado(HttpStatusCode.Conflict, () => _clientes.CrearAsync(new() { Nombre = "Ana", Telefono = " 777 " }));
        await Estado(HttpStatusCode.Conflict, () => _clientes.ActualizarAsync(otro.Id, new() { Nombre = "Luis", Telefono = "777" }));
        await _clientes.ActualizarAsync(cliente.Id, new() { Nombre = "Ana Maria", Telefono = "777" });
    }

    [Fact]
    public async Task DatosInvalidos_NoSePersisten()
    {
        await Assert.ThrowsAsync<ValidationException>(() => _mesas.CrearAsync(new() { Numero = 0, Capacidad = 4 }));
        await Assert.ThrowsAsync<ValidationException>(() => _mesas.CrearAsync(new() { Numero = 1, Capacidad = 0 }));
        await Assert.ThrowsAsync<ValidationException>(() => _clientes.CrearAsync(new() { Nombre = " ", Telefono = "777" }));
        await Assert.ThrowsAsync<ValidationException>(() => _clientes.CrearAsync(new() { Nombre = "Ana", Telefono = " " }));
        await Assert.ThrowsAsync<ValidationException>(() => _clientes.CrearAsync(new() { Nombre = "Ana", Telefono = "777", Correo = "invalido" }));
        await Assert.ThrowsAsync<ValidationException>(() => _clientes.CrearAsync(new() { Nombre = new string('x', 151), Telefono = "777" }));
        Assert.Empty(await _clientes.ObtenerTodosAsync());
        Assert.Empty(await _mesas.ObtenerTodosAsync());
    }

    [Fact]
    public async Task Inexistentes_Devuelven404()
    {
        await Estado(HttpStatusCode.NotFound, () => _mesas.ActualizarAsync(99, new() { Numero = 1, Capacidad = 4 }));
        await Estado(HttpStatusCode.NotFound, () => _clientes.ActualizarAsync(99, new() { Nombre = "Ana", Telefono = "777" }));
        await Estado(HttpStatusCode.NotFound, () => _mesas.EliminarAsync(99));
        await Estado(HttpStatusCode.NotFound, () => _clientes.EliminarAsync(99));
    }

    [Fact]
    public async Task Reservas_ImpidenEliminar_YReducirCapacidad()
    {
        var mesa = await _mesas.CrearAsync(new() { Numero = 1, Capacidad = 4 });
        var cliente = await _clientes.CrearAsync(new() { Nombre = "Ana", Telefono = "777" });
        _context.Reservas.Add(new Reserva { Codigo = "RES-TEST", MesaId = mesa.Id, ClienteId = cliente.Id,
            CantidadPersonas = 4, Estado = EstadoReserva.Confirmada,
            FechaHoraInicio = DateTime.UtcNow.AddHours(1), FechaHoraFin = DateTime.UtcNow.AddHours(3) });
        await _context.SaveChangesAsync();
        await Estado(HttpStatusCode.Conflict, () => _mesas.EliminarAsync(mesa.Id));
        await Estado(HttpStatusCode.Conflict, () => _clientes.EliminarAsync(cliente.Id));
        await Estado(HttpStatusCode.Conflict, () => _mesas.ActualizarAsync(mesa.Id, new() { Numero = 1, Capacidad = 2 }));
        Assert.Equal(4, (await _mesas.ObtenerPorIdAsync(mesa.Id)).Capacidad);
        await _mesas.ActualizarAsync(mesa.Id, new() { Numero = 1, Capacidad = 6 });
    }

    [Fact]
    public async Task EditarCliente_ConservaSancionesYBloqueos()
    {
        var cliente = new Cliente { Nombre = "Ana", Telefono = "777", ContadorSanciones = 3,
            Bloqueado = true, FechaBloqueo = DateTime.UtcNow, BloqueadoHasta = DateTime.UtcNow.AddDays(5) };
        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();
        var resultado = await _clientes.ActualizarAsync(cliente.Id, new() { Nombre = "Ana Maria", Telefono = "888" });
        Assert.True(resultado.Bloqueado);
        Assert.Equal(3, resultado.ContadorSanciones);
        Assert.Equal(cliente.FechaBloqueo, resultado.FechaBloqueo);
        Assert.Equal(cliente.BloqueadoHasta, resultado.BloqueadoHasta);
    }

    [Theory]
    [InlineData("mesas", "{\"numero\":1,\"capacidad\":4}", "{\"numero\":2,\"capacidad\":6}")]
    [InlineData("clientes", "{\"nombre\":\"Ana\",\"telefono\":\"777\"}", "{\"nombre\":\"Maria\",\"telefono\":\"888\"}")]
    public async Task Api_CicloCrudYErrores(string recurso, string crear, string editar)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddSingleton<IMesaService>(_mesas);
        builder.Services.AddSingleton<IClienteService>(_clientes);
        builder.Services.AddSingleton<ConsultaDisponibilidadValidator>();
        builder.Services.AddSingleton<GuardarMesaValidator>();
        builder.Services.AddSingleton<GuardarClienteValidator>();
        builder.Services.AddControllers().AddApplicationPart(typeof(MesasController).Assembly)
            .ConfigureApiBehaviorOptions(options => options.SuppressModelStateInvalidFilter = true);
        await using var app = builder.Build();
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.MapControllers();
        await app.StartAsync();
        using var client = app.GetTestClient();
        var ruta = $"/api/{recurso}";
        var creado = await client.PostAsync(ruta, Json(crear));
        Assert.Equal(HttpStatusCode.Created, creado.StatusCode);
        Assert.NotNull(creado.Headers.Location);
        var location = creado.Headers.Location;
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(location)).StatusCode);
        using var listado = JsonDocument.Parse(await client.GetStringAsync(ruta));
        Assert.Single(listado.RootElement.GetProperty("data").EnumerateArray());
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsync(ruta, Json(crear))).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.PutAsync(location, Json(editar))).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync(ruta, Json("{}"))).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PutAsync(location, Json("{}"))).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync(ruta, Json("null"))).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync(ruta, Json("{"))).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync(location)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync(location)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync(location)).StatusCode);
    }

    [Theory]
    [InlineData("sin relaciones", HttpStatusCode.NoContent)]
    [InlineData("cancelacion", HttpStatusCode.Conflict)]
    [InlineData("sancion", HttpStatusCode.Conflict)]
    public async Task Api_EliminarReserva_RespetaRelaciones(string relacion, HttpStatusCode esperado)
    {
        var mesa = new Mesa { Numero = 1, Capacidad = 4 };
        var cliente = new Cliente { Nombre = "Ana", Telefono = "777", ContadorSanciones = 1, Bloqueado = true };
        var reserva = new Reserva { Codigo = "RES-TEST", Mesa = mesa, Cliente = cliente,
            CantidadPersonas = 4, FechaHoraInicio = DateTime.UtcNow.AddHours(1),
            FechaHoraFin = DateTime.UtcNow.AddHours(3) };
        _context.Reservas.Add(reserva);
        if (relacion == "cancelacion")
        {
            reserva.Estado = EstadoReserva.Cancelada;
            _context.Cancelaciones.Add(new Cancelacion { Reserva = reserva, CanceladaPor = "Ana", MontoReembolso = 25 });
        }
        if (relacion == "sancion")
        {
            reserva.Estado = EstadoReserva.NoAsistio;
            _context.Sanciones.Add(new Sancion { Reserva = reserva, Cliente = cliente, Motivo = MotivoSancion.Inasistencia, NumeroFalta = 1 });
        }
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();
        var mapper = new MapperConfiguration(c => c.AddProfile<ReservaProfile>()).CreateMapper();
        var service = new ReservaService(new ReservaRepository(_context), new MesaRepository(_context),
            new ClienteRepository(_context),
            new SancionRepository(_context), _context, new BaseRepository<Reserva>(_context),
            new BaseRepository<Cliente>(_context), new BaseRepository<Cancelacion>(_context),
            new BaseRepository<Sancion>(_context), mapper);
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddSingleton<IReservaService>(service);
        builder.Services.AddSingleton<CrearReservaValidator>();
        builder.Services.AddSingleton<ModificarReservaValidator>();
        builder.Services.AddSingleton<CancelarReservaValidator>();
        builder.Services.AddControllers().AddApplicationPart(typeof(ReservasController).Assembly);
        await using var app = builder.Build();
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.MapControllers();
        await app.StartAsync();
        using var clientHttp = app.GetTestClient();
        var respuesta = await clientHttp.DeleteAsync("/api/reservas/RES-TEST");
        Assert.Equal(esperado, respuesta.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await clientHttp.DeleteAsync("/api/reservas/NO-EXISTE")).StatusCode);
        _context.ChangeTracker.Clear();
        Assert.Single(await _context.Mesas.ToListAsync());
        var clienteGuardado = Assert.Single(await _context.Clientes.ToListAsync());
        Assert.True(clienteGuardado.Bloqueado);
        Assert.Equal(1, clienteGuardado.ContadorSanciones);
        if (esperado == HttpStatusCode.NoContent)
        {
            Assert.Empty(await _context.Reservas.ToListAsync());
            Assert.Equal(HttpStatusCode.NotFound, (await clientHttp.GetAsync("/api/reservas/RES-TEST")).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await clientHttp.DeleteAsync("/api/reservas/RES-TEST")).StatusCode);
            Assert.Single(await new MesaRepository(_context).ObtenerMesasDisponiblesAsync(
                reserva.FechaHoraInicio, reserva.FechaHoraFin, 4));
        }
        else
        {
            Assert.Single(await _context.Reservas.ToListAsync());
            if (relacion == "cancelacion") Assert.Equal(25m, (await _context.Cancelaciones.SingleAsync()).MontoReembolso);
            if (relacion == "sancion") Assert.Single(await _context.Sanciones.ToListAsync());
        }
    }

    [Fact]
    public async Task Controladores_LanzanBusinessException_AntesDeInvocarServicios()
    {
        var mesas = new MesasController(null!, new ConsultaDisponibilidadValidator(), new GuardarMesaValidator());
        var clientes = new ClientesController(null!, new GuardarClienteValidator());
        var reservas = new ReservasController(null!, new CrearReservaValidator(),
            new ModificarReservaValidator(), new CancelarReservaValidator());
        // Los servicios son null para comprobar que la validación ocurre en el controlador.
        Func<Task>[] acciones =
        [
            () => mesas.Crear(new GuardarMesaDto(), default),
            () => mesas.Actualizar(1, new GuardarMesaDto(), default),
            () => mesas.ConsultarDisponibilidad(new ConsultaDisponibilidadDto(), default),
            () => clientes.Crear(new GuardarClienteDto(), default),
            () => clientes.Actualizar(1, new GuardarClienteDto(), default),
            () => reservas.RegistrarReserva(new CrearReservaDto(), default),
            () => reservas.ModificarReserva("RES-TEST", new ModificarReservaDto(), default),
            () => reservas.CancelarReserva("RES-TEST", new CancelarReservaDto(), default)
        ];
        foreach (var accion in acciones)
        {
            var ex = await Assert.ThrowsAsync<BusinessException>(accion);
            Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
            Assert.NotNull(ex.Details);
        }
    }

    private static StringContent Json(string contenido) => new(contenido, Encoding.UTF8, "application/json");

    private static async Task Estado(HttpStatusCode esperado, Func<Task> accion)
    {
        var ex = await Assert.ThrowsAsync<BusinessException>(accion);
        Assert.Equal(esperado, ex.StatusCode);
    }

    public void Dispose() { _context.Dispose(); _connection.Dispose(); }
}
