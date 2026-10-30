using Microsoft.EntityFrameworkCore;
using Restaurante.Core.Entities;
using Restaurante.Core.Interfaces;
using Restaurante.Infrastructure.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Restaurante.Infrastructure.Repositories
{
    public class ReservaRepository : BaseRepository<Reserva>, IReservaRepository
    {
        public ReservaRepository(RestauranteContext context) : base(context)
        {
        }

        // Comprueba si existen reservas que se cruzan con el horario (excluyendo canceladas)
        public async Task<bool> ExisteCruceHorarioAsync(
            int mesaId,
            DateTime inicio,
            DateTime fin,
            int? excluirReservaId = null)
        {
            var consulta = _context.Reservas
                .Where(r => r.MesaId == mesaId)
                .Where(r => r.Estado != EstadoReserva.Cancelada)
                .Where(r =>
                    r.FechaHoraInicio < fin &&
                    r.FechaHoraFin > inicio);

            if (excluirReservaId.HasValue)
            {
                consulta = consulta.Where(r =>
                    r.Id != excluirReservaId.Value);
            }

            return await consulta.AnyAsync();
        }

        // Obtiene todas las reservas con sus relaciones (Cliente, Mesa, Sanción)
        public async Task<IEnumerable<Reserva>> GetAllWithDetailsAsync()
        {
            return await _context.Reservas
                .Include(r => r.Cliente)
                .Include(r => r.Mesa)
                .Include(r => r.Sancion)
                .ToListAsync();
        }

        // Obtiene una reserva con sus detalles (Cliente, Mesa, Sanción)
        public async Task<Reserva?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Reservas
                .Include(r => r.Cliente)
                .Include(r => r.Mesa)
                .Include(r => r.Sancion)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        // Verifica si la reserva ya tiene una sanción registrada
        public async Task<bool> TieneSancionAsync(int reservaId)
        {
            return await _context.Sanciones
                .AnyAsync(s => s.ReservaId == reservaId);
        }

        // Busca un cliente por teléfono o correo para validar bloqueo o reusar datos
        public async Task<Cliente?> BuscarClientePorTelefonoOCorreoAsync(string telefono, string? correo)
        {
            var consulta = _context.Clientes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(correo))
            {
                return await consulta
                    .OrderByDescending(c => c.Id)
                    .FirstOrDefaultAsync(c => c.Telefono == telefono || c.Correo == correo);
            }

            return await consulta
                .OrderByDescending(c => c.Id)
                .FirstOrDefaultAsync(c => c.Telefono == telefono);
        }
    }
}
