using Restaurante.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Restaurante.Core.Interfaces
{
    public interface IMesaRepository
    {
        // Consulta mesas activas con capacidad suficiente
        // y sin reservas que se crucen con el horario solicitado
        Task<IReadOnlyList<Mesa>> ObtenerMesasDisponiblesAsync(
            DateTime inicio,
            DateTime fin,
            int capacidadMinima,
            CancellationToken cancellationToken = default);

        // Busca una mesa por su identificador
        // Devuelve null si no existe
        Task<Mesa?> ObtenerPorIdAsync(
            int id,
            CancellationToken cancellationToken = default);
    }
}