using FluentValidation;
using Restaurante.Core.DTOs;
using Restaurante.Core.Entities;
using Restaurante.Core.Exceptions;
using Restaurante.Core.Interfaces;
using Restaurante.Infrastructure.Data;
using Restaurante.Infrastructure.Repositories;
using Restaurante.Services.Interfaces;
using Restaurante.Services.Validators;
using System.Net;

namespace Restaurante.Services.Services;

public class ClienteService : IClienteService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBaseRepository<Cliente> _clienteCrud;
    private readonly GuardarClienteValidator _validator;

    public ClienteService(IUnitOfWork unitOfWork, GuardarClienteValidator validator)
    {
        _unitOfWork = unitOfWork;
        _clienteCrud = unitOfWork.Clientes;
        _validator = validator;
    }

    public ClienteService(IBaseRepository<Cliente> repository, RestauranteContext context, GuardarClienteValidator validator)
        : this(new UnitOfWork(context), validator)
    {
        _clienteCrud = repository;
    }

    public async Task<IEnumerable<ClienteDetalleDto>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
    {
        var entities = (await _clienteCrud.GetAll()).OrderBy(x => x.Id);
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
        await _clienteCrud.Add(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
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
        await _clienteCrud.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Mapear(entity);
    }

    public async Task EliminarAsync(int id, CancellationToken cancellationToken = default)
    {
        await BuscarAsync(id, cancellationToken);
        if (await _unitOfWork.Clientes.TieneRegistrosAsociadosAsync(id, cancellationToken))
            throw new BusinessException("No se puede eliminar: existen registros relacionados.", HttpStatusCode.Conflict);
        await _clienteCrud.Delete(id);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Cliente> BuscarAsync(int id, CancellationToken cancellationToken)
        => await _clienteCrud.GetById(id)
            ?? throw new BusinessException("Cliente no existe.", HttpStatusCode.NotFound);

    private async Task ValidarUnicoAsync(GuardarClienteDto dto, int? excluirId, CancellationToken cancellationToken)
    {
        if (await _unitOfWork.Clientes.ExisteTelefonoAsync(dto.Telefono.Trim(), excluirId, cancellationToken))
            throw new BusinessException("Ya existe un registro con ese teléfono.", HttpStatusCode.Conflict);
    }

    private static ClienteDetalleDto Mapear(Cliente entity) => new() { Id = entity.Id, Nombre = entity.Nombre, Telefono = entity.Telefono, Correo = entity.Correo, ContadorSanciones = entity.ContadorSanciones, Bloqueado = entity.Bloqueado, FechaBloqueo = entity.FechaBloqueo, BloqueadoHasta = entity.BloqueadoHasta };

}
