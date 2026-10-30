using Microsoft.EntityFrameworkCore;
using Restaurante.Core.Entities;
using Restaurante.Core.Interfaces;
using Restaurante.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Restaurante.Infrastructure.Repositories
{
    public class MesaRepository : BaseRepository<Mesa>, IMesaRepository
    {
        public MesaRepository(RestauranteContext context) : base(context)
        {
        }

        // Obtiene las mesas con capacidad suficiente y sin cruces de horario
        public async Task<IEnumerable<Mesa>> ObtenerMesasDisponiblesAsync(
            DateTime inicio,
            DateTime fin,
            int cantidadPersonas)
        {
            var mesas = await _context.Mesas
                .Where(m => m.Capacidad >= cantidadPersonas)
                .Where(m => !m.Reservas.Any(r =>
                    r.Estado != EstadoReserva.Cancelada &&
                    r.FechaHoraInicio < fin &&
                    r.FechaHoraFin > inicio))
                .OrderBy(m => m.Capacidad)
                .ThenBy(m => m.Numero)
                .ToListAsync();

            return mesas;
        }
    }
}
