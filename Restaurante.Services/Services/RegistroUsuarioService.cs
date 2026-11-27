
using Microsoft.AspNetCore.Identity;
using Restaurante.Core.DTOs;
using Restaurante.Core.Entities;
using Restaurante.Core.Interfaces;
using Restaurante.Services.Interfaces;

namespace Restaurante.Services.Services
{
    public class RegistroUsuarioService : IRegistroUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IClienteRepository _clienteRepository;
        private readonly PasswordHasher<Usuario> _passwordHasher;

        public RegistroUsuarioService(
            IUsuarioRepository usuarioRepository,
            IClienteRepository clienteRepository)
        {
            _usuarioRepository = usuarioRepository;
            _clienteRepository = clienteRepository;
            _passwordHasher = new PasswordHasher<Usuario>();
        }

        public async Task RegistrarAsync(
            RegistroUsuarioDto dto,
            CancellationToken cancellationToken = default)
        {
            // Aquí implementaremos el registro.
            throw new NotImplementedException();
        }
    }
}
