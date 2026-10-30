using Restaurante.Core.Entities;
using Restaurante.Core.Exceptions;
using Restaurante.Core.Interfaces;
using Restaurante.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Restaurante.Services.Services
{
    public class MesaService : IMesaService
    {
        private readonly IBaseRepository<Mesa> _mesaBaseRepository;
        private readonly IMesaRepository _mesaRepository;

        public MesaService(
            IBaseRepository<Mesa> mesaBaseRepository,
            IMesaRepository mesaRepository)
        {
            _mesaBaseRepository = mesaBaseRepository;
            _mesaRepository = mesaRepository;
        }

        public async Task<IEnumerable<Mesa>> GetAllMesasAsync()
        {
            return await _mesaBaseRepository.GetAll();
        }

        public async Task<Mesa> GetMesaByIdAsync(int id)
        {
            var mesa = await _mesaBaseRepository.GetById(id);
            if (mesa == null)
            {
                throw new BusinessException("La mesa no existe", HttpStatusCode.NotFound);
            }
            return mesa;
        }

        public async Task<IEnumerable<Mesa>> ObtenerMesasDisponiblesAsync(
            DateTime inicio,
            DateTime fin,
            int cantidadPersonas)
        {
            if (cantidadPersonas <= 0)
            {
                throw new BusinessException("La cantidad de personas debe ser mayor que cero.");
            }

            if (fin <= inicio)
            {
                throw new BusinessException("La fecha de fin debe ser posterior a la fecha de inicio.");
            }

            return await _mesaRepository.ObtenerMesasDisponiblesAsync(inicio, fin, cantidadPersonas);
        }

        public async Task InsertMesa(Mesa mesa)
        {
            await _mesaBaseRepository.Add(mesa);
        }

        public async Task UpdateMesa(Mesa mesa)
        {
            await _mesaBaseRepository.Update(mesa);
        }

        public async Task DeleteMesa(int id)
        {
            await _mesaBaseRepository.Delete(id);
        }
    }
}

