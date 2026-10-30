using AutoMapper;
using Restaurante.Core.DTOs;
using Restaurante.Core.Interfaces;
using Restaurante.Services.Interfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Restaurante.Services.Services
{
    public class MesaService : IMesaService
    {
        private readonly IMesaRepository _mesaRepository;
        private readonly IMapper _mapper;

        public MesaService(IMesaRepository mesaRepository, IMapper mapper)
        {
            _mesaRepository = mesaRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<MesaDisponibleDto>> ConsultarDisponibilidadAsync(
            ConsultaDisponibilidadDto consulta,
            CancellationToken cancellationToken = default)
        {
            var mesas = await _mesaRepository.ObtenerMesasDisponiblesAsync(
                consulta.FechaHoraInicio,
                consulta.FechaHoraFin,
                consulta.CantidadPersonas,
                cancellationToken);

            return _mapper.Map<IEnumerable<MesaDisponibleDto>>(mesas);
        }
    }
}
