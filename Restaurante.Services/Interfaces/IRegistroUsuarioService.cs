using Restaurante.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Restaurante.Services.Interfaces
{
    public interface IRegistroUsuarioService
    {
        Task<UsuarioDetalleDto> RegistrarAsync(
           RegistroUsuarioDto dto,
           CancellationToken cancellationToken = default);
    }
}

