using System.Net;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Restaurante.Core.DTOs;
using Restaurante.Core.Entities;
using Restaurante.Core.Enums;
using Restaurante.Core.Exceptions;
using Restaurante.Core.Interfaces;
using Restaurante.Services.Interfaces;
using Restaurante.Services.Validators;

namespace Restaurante.Services.Services;

public class RegistroUsuarioService(
    IUnitOfWork unitOfWork,
    RegistroUsuarioValidator validator) : IRegistroUsuarioService
{
    public async Task<UsuarioDetalleDto> RegistrarAsync(
        RegistroUsuarioDto dto,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(dto, cancellationToken);

        if (await unitOfWork.Clientes.GetById(dto.ClienteId) is null)
            throw new BusinessException("Cliente no existe.", HttpStatusCode.NotFound);

        var correo = dto.Correo.Trim();
        await VerificarCuentaDisponibleAsync(dto.ClienteId, correo, cancellationToken);

        var usuario = new Usuario
        {
            ClienteId = dto.ClienteId,
            Correo = correo,
            Rol = nameof(RoleType.Consumer),
            PasswordHash = Usuario.PrefijoPasswordSinHash + dto.Password
        };

        await unitOfWork.Usuarios.Add(usuario);
        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            await VerificarCuentaDisponibleAsync(dto.ClienteId, correo, cancellationToken);
            throw;
        }

        return new UsuarioDetalleDto(usuario.Id, usuario.ClienteId, usuario.Correo, usuario.Rol);
    }

    private async Task VerificarCuentaDisponibleAsync(int clienteId, string correo, CancellationToken cancellationToken)
    {
        if (await unitOfWork.Usuarios.ObtenerPorCorreoAsync(correo, cancellationToken) is not null)
            throw new BusinessException("Ya existe una cuenta con ese correo.", HttpStatusCode.Conflict);

        if (await unitOfWork.Usuarios.ExisteParaClienteAsync(clienteId, cancellationToken))
            throw new BusinessException("El cliente ya tiene una cuenta.", HttpStatusCode.Conflict);
    }
}
