using Restaurante.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Restaurante.Services.Interfaces
{
    public interface IMesaService
    {
        Task<IEnumerable<Mesa>> GetAllMesasAsync();
        Task<Mesa> GetMesaByIdAsync(int id);
        Task<IEnumerable<Mesa>> ObtenerMesasDisponiblesAsync(DateTime inicio, DateTime fin, int cantidadPersonas);
        Task InsertMesa(Mesa mesa);
        Task UpdateMesa(Mesa mesa);
        Task DeleteMesa(int id);
    }
}

