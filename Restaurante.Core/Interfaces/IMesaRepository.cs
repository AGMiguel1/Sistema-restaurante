using Restaurante.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Restaurante.Core.Interfaces
{
    public interface IMesaRepository : IBaseRepository<Mesa>
    {
        Task<bool> ExisteNumeroAsync(int numero, int? excluirId, CancellationToken cancellationToken);
        Task<bool> TieneReservasAsync(int mesaId, CancellationToken cancellationToken);
        Task<bool> TieneReservaVigenteSobreCapacidadAsync(
            int mesaId, int capacidad, DateTime ahora, CancellationToken cancellationToken);

        // Consulta mesas activas con capacidad suficiente
        // y sin reservas que se crucen con el horario solicitado.
        Task<IReadOnlyList<Mesa>> ObtenerMesasDisponiblesAsync(
            DateTime inicio,
            DateTime fin,
            int capacidadMinima,
            CancellationToken cancellationToken = default);

        // Busca una mesa por su identificador.
        Task<Mesa?> ObtenerPorIdAsync(
            int id,
            CancellationToken cancellationToken = default);
    }
}
