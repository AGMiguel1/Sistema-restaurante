using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Restaurante.Core.DTOs;
using Restaurante.Core.Entities;
using Restaurante.Core.Exceptions;
using Restaurante.Core.Interfaces;
using Restaurante.Infrastructure.Data;
using Restaurante.Services.Interfaces;
using Restaurante.Services.Validators;
using System.Net;

namespace Restaurante.Services.Services;

public class ClienteService : IClienteService
{
    private readonly IBaseRepository<Cliente> _repository;
    private readonly RestauranteContext _context;
    private readonly GuardarClienteValidator _validator;

    public ClienteService(IBaseRepository<Cliente> repository, RestauranteContext context, GuardarClienteValidator validator)
    {
        _repository = repository;
        _context = context;
        _validator = validator;
    }

    public async Task<IEnumerable<ClienteDetalleDto>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
    {
        var entities = (await _repository.GetAll()).OrderBy(x => x.Id);
        return entities.Select(Mapear).ToList();
    }

    public async Task<ClienteDetalleDto> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
        => Mapear(await BuscarAsync(id, cancellationToken));

    public async Task<ClienteDetalleDto> CrearAsync(GuardarClienteDto dto, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(dto, cancellationToken);
        await ValidarUnicoAsync(dto, null, cancellationToken);
        var entity = new Cliente();
        entity.Nombre = dto.Nombre.Trim();
        entity.Telefono = dto.Telefono.Trim();
        entity.Correo = string.IsNullOrWhiteSpace(dto.Correo) ? null : dto.Correo.Trim();
        await _repository.Add(entity);
        return Mapear(entity);
    }

    public async Task<ClienteDetalleDto> ActualizarAsync(int id, GuardarClienteDto dto, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(dto, cancellationToken);
        var entity = await BuscarAsync(id, cancellationToken);
        await ValidarUnicoAsync(dto, id, cancellationToken);

        entity.Nombre = dto.Nombre.Trim();
        entity.Telefono = dto.Telefono.Trim();
        entity.Correo = string.IsNullOrWhiteSpace(dto.Correo) ? null : dto.Correo.Trim();
        await _repository.Update(entity);
        return Mapear(entity);
    }

    public async Task EliminarAsync(int id, CancellationToken cancellationToken = default)
    {
        await BuscarAsync(id, cancellationToken);
        if ((await _context.Reservas.AnyAsync(r => r.ClienteId == id, cancellationToken) || await _context.Sanciones.AnyAsync(s => s.ClienteId == id, cancellationToken)))
            throw new BusinessException("No se puede eliminar: existen registros relacionados.", HttpStatusCode.Conflict);
        await _repository.Delete(id);
    }

    private async Task<Cliente> BuscarAsync(int id, CancellationToken cancellationToken)
        => await _repository.GetById(id)
            ?? throw new BusinessException("Cliente no existe.", HttpStatusCode.NotFound);

    private async Task ValidarUnicoAsync(GuardarClienteDto dto, int? excluirId, CancellationToken cancellationToken)
    {
        if (await _context.Clientes.AnyAsync(x => x.Telefono == dto.Telefono.Trim() &&
            (!excluirId.HasValue || x.Id != excluirId.Value), cancellationToken))
            throw new BusinessException("Ya existe un registro con ese teléfono.", HttpStatusCode.Conflict);
    }

    private static ClienteDetalleDto Mapear(Cliente entity) => new() { Id = entity.Id, Nombre = entity.Nombre, Telefono = entity.Telefono, Correo = entity.Correo, ContadorSanciones = entity.ContadorSanciones, Bloqueado = entity.Bloqueado, FechaBloqueo = entity.FechaBloqueo, BloqueadoHasta = entity.BloqueadoHasta };

}
