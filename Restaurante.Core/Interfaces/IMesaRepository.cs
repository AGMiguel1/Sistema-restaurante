using Restaurante.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Restaurante.Core.Interfaces
{
    public interface IMesaRepository
    {
        // Obtener mesas disponibles según horario y cantidad de personas
        Task<IEnumerable<Mesa>> ObtenerMesasDisponiblesAsync(
            DateTime inicio,
            DateTime fin,
            int cantidadPersonas);

        // Obtener una mesa por su identificador
        Task<Mesa?> ObtenerPorIdAsync(int id);
    }
}