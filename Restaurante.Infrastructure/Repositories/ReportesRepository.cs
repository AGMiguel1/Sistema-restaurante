using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Restaurante.Core.CustomEntities.Reporteria;
using Restaurante.Core.Entities;
using Restaurante.Core.Interfaces;
using Restaurante.Infrastructure.Data;
using Restaurante.Infrastructure.Queries;

namespace Restaurante.Infrastructure.Repositories;

public class ReportesRepository(DapperContext dapper) : IReportesRepository
{
    public Task<IEnumerable<ClienteFrecuente>> ObtenerClientesFrecuentesAsync(int limite, CancellationToken cancellationToken)
        => dapper.QueryAsync<ClienteFrecuente>(
            Reportes.ClientesFrecuentes,
            new
            {
                Confirmada = (int)EstadoReserva.Confirmada,
                Completada = (int)EstadoReserva.Completada,
                Limite = limite
            },
            cancellationToken);
}
