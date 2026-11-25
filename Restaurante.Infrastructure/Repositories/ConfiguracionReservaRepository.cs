using Microsoft.EntityFrameworkCore;
using Restaurante.Core.Entities;
using Restaurante.Core.Interfaces;
using Restaurante.Infrastructure.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Restaurante.Infrastructure.Repositories;

public class ConfiguracionReservaRepository(RestauranteContext context) : IConfiguracionReservaRepository
{
    public Task<decimal> ObtenerPorcentajeReembolsoAsync(decimal horasAnticipacion, CancellationToken cancellationToken)
        => context.PoliticasReembolso
            .Where(p => p.Activa && p.HorasMinimasAnticipacion <= horasAnticipacion)
            .OrderByDescending(p => p.HorasMinimasAnticipacion)
            .Select(p => p.PorcentajeReembolso)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<string?> ObtenerValorAsync(string clave, CancellationToken cancellationToken)
        => context.Configuraciones
            .Where(c => c.Clave == clave)
            .Select(c => (string?)c.Valor)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<EscalaSancion?> ObtenerEscalaSancionAsync(int numeroFalta, CancellationToken cancellationToken)
        => context.EscalasSancion.AsNoTracking()
            .FirstOrDefaultAsync(e => e.NumeroFalta == numeroFalta, cancellationToken);
}
