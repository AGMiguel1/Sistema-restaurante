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
    public class MesaRepository : IMesaRepository
    {
        private readonly RestauranteContext _context;

        public MesaRepository(RestauranteContext context)
        {
            _context = context;
        }

        // Consulta mesas con capacidad suficiente y horario libre.
        public async Task<IReadOnlyList<Mesa>> ObtenerMesasDisponiblesAsync(
            DateTime inicio,
            DateTime fin,
            int capacidadMinima,
            CancellationToken cancellationToken = default)
        {
            var mesas = await _context.Mesas
                .AsNoTracking()

                // Verifica la capacidad de la mesa.
                .Where(m => m.Capacidad >= capacidadMinima)

                // Excluye mesas con reservas que se cruzan con el horario.
                .Where(m => !m.Reservas.Any(r =>
                    r.Estado != EstadoReserva.Cancelada &&
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
            var mesa = await _context.Mesas
                .FirstOrDefaultAsync(
                    m => m.Id == id,
                    cancellationToken);

            return mesa;
        }
    }
}