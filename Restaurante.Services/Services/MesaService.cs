using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Restaurante.Core.Entities;
using Restaurante.Core.Exceptions;
using Restaurante.Infrastructure.Data;
using Restaurante.Services.Validators;
using System.Linq;
using System.Net;
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
        private readonly IBaseRepository<Mesa> _baseRepository;
        private readonly IMapper _mapper;
        private readonly RestauranteContext _context;
        private readonly GuardarMesaValidator _validator;

        public MesaService(IMesaRepository mesaRepository, IMapper mapper, RestauranteContext context, GuardarMesaValidator validator, IBaseRepository<Mesa> baseRepository)
        {
            _mesaRepository = mesaRepository;
            _baseRepository = baseRepository;
            _mapper = mapper;
            _context = context;
            _validator = validator;
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

        public async Task<IEnumerable<MesaDetalleDto>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
        {
            var entities = (await _baseRepository.GetAll()).OrderBy(x => x.Id);
            return entities.Select(Mapear).ToList();
        }

        public async Task<MesaDetalleDto> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
            => Mapear(await BuscarAsync(id, cancellationToken));

        public async Task<MesaDetalleDto> CrearAsync(GuardarMesaDto dto, CancellationToken cancellationToken = default)
        {
            await _validator.ValidateAndThrowAsync(dto, cancellationToken);
            await ValidarUnicoAsync(dto, null, cancellationToken);
            var entity = new Mesa();
            entity.Numero = dto.Numero;
            entity.Capacidad = dto.Capacidad;
            await _baseRepository.Add(entity);
            return Mapear(entity);
        }

        public async Task<MesaDetalleDto> ActualizarAsync(int id, GuardarMesaDto dto, CancellationToken cancellationToken = default)
        {
            await _validator.ValidateAndThrowAsync(dto, cancellationToken);
            var entity = await BuscarAsync(id, cancellationToken);
            await ValidarUnicoAsync(dto, id, cancellationToken);

            if (await _context.Reservas.AnyAsync(r => r.MesaId == id &&
                r.Estado == EstadoReserva.Confirmada && r.FechaHoraFin > DateTime.UtcNow &&
                r.CantidadPersonas > dto.Capacidad, cancellationToken))
                throw new BusinessException("La capacidad no puede ser menor que la cantidad de personas de una reserva vigente.", HttpStatusCode.Conflict);

            entity.Numero = dto.Numero;
            entity.Capacidad = dto.Capacidad;
            await _baseRepository.Update(entity);
            return Mapear(entity);
        }

        public async Task EliminarAsync(int id, CancellationToken cancellationToken = default)
        {
            await BuscarAsync(id, cancellationToken);
            if (await _context.Reservas.AnyAsync(r => r.MesaId == id, cancellationToken))
                throw new BusinessException("No se puede eliminar: existen registros relacionados.", HttpStatusCode.Conflict);
            await _baseRepository.Delete(id);
        }

        private async Task<Mesa> BuscarAsync(int id, CancellationToken cancellationToken)
            => await _baseRepository.GetById(id)
                ?? throw new BusinessException("Mesa no existe.", HttpStatusCode.NotFound);

        private async Task ValidarUnicoAsync(GuardarMesaDto dto, int? excluirId, CancellationToken cancellationToken)
        {
            if (await _context.Mesas.AnyAsync(x => x.Numero == dto.Numero &&
                (!excluirId.HasValue || x.Id != excluirId.Value), cancellationToken))
                throw new BusinessException("Ya existe un registro con ese número de mesa.", HttpStatusCode.Conflict);
        }

        private static MesaDetalleDto Mapear(Mesa entity) => new() { Id = entity.Id, Numero = entity.Numero, Capacidad = entity.Capacidad };
    }
}
