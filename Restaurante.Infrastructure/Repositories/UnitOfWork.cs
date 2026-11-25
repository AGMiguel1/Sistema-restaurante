using Microsoft.EntityFrameworkCore;
using Restaurante.Core.Exceptions;
using Restaurante.Core.Interfaces;
using Restaurante.Infrastructure.Data;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Restaurante.Infrastructure.Repositories;

public class UnitOfWork(
    RestauranteContext context,
    IMesaRepository mesas,
    IClienteRepository clientes,
    IReservaRepository reservas,
    ICancelacionRepository cancelaciones,
    ISancionRepository sanciones,
    IConfiguracionReservaRepository configuracionReservas) : IUnitOfWork
{
    public UnitOfWork(
        RestauranteContext context,
        IMesaRepository? mesas = null,
        IClienteRepository? clientes = null,
        IReservaRepository? reservas = null,
        ISancionRepository? sanciones = null)
        : this(context, mesas ?? new MesaRepository(context), clientes ?? new ClienteRepository(context),
            reservas ?? new ReservaRepository(context), new CancelacionRepository(context),
            sanciones ?? new SancionRepository(context), new ConfiguracionReservaRepository(context))
    {
    }

    public IMesaRepository Mesas { get; } = mesas;
    public IClienteRepository Clientes { get; } = clientes;
    public IReservaRepository Reservas { get; } = reservas;
    public ICancelacionRepository Cancelaciones { get; } = cancelaciones;
    public ISancionRepository Sanciones { get; } = sanciones;
    public IConfiguracionReservaRepository ConfiguracionReservas { get; } = configuracionReservas;

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            context.ChangeTracker.Clear();
            throw new BusinessException("Los datos cambiaron durante la operación. Vuelve a consultarlos e inténtalo de nuevo.",
                HttpStatusCode.Conflict);
        }
        catch (DbUpdateException)
        {
            context.ChangeTracker.Clear();
            throw;
        }
    }
}
