using Microsoft.EntityFrameworkCore;
using Restaurante.Core.Entities;
using Restaurante.Core.Interfaces;
using Restaurante.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Restaurante.Infrastructure.Repositories
{
    public class MesaRepository : BaseRepository<Mesa>, IMesaRepository
    {
        public MesaRepository(RestauranteContext context) : base(context)
        {
        }

        // Consulta mesas con capacidad suficiente y horario libre.
        public async Task<IReadOnlyList<Mesa>> ObtenerMesasDisponiblesAsync(
            DateTime inicio,
            DateTime fin,
            int capacidadMinima,
            CancellationToken cancellationToken = default)
        {
            var mesas = await _entities
                .AsNoTracking()
                // Verifica la capacidad de la mesa.
                .Where(m => m.Capacidad >= capacidadMinima)
                // Excluye mesas con reservas confirmadas o completadas que se cruzan.
                .Where(m => !m.Reservas.Any(r =>
                    r.Estado != EstadoReserva.Cancelada &&
                    r.Estado != EstadoReserva.NoAsistio &&
                    r.FechaHoraInicio < fin &&
                    r.FechaHoraFin > inicio))
                .OrderBy(m => m.Capacidad)
                .ThenBy(m => m.Numero)
                .ToListAsync(cancellationToken);

            return mesas;
        }

        // Busca una mesa por su identificador.
        public async Task<Mesa?> ObtenerPorIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return await _entities
                .FirstOrDefaultAsync(
                    m => m.Id == id,
                    cancellationToken);
        }
    }
}
