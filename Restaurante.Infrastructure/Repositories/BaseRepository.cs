using Microsoft.EntityFrameworkCore;
using Restaurante.Core.Entities;
using Restaurante.Core.Interfaces;
using Restaurante.Infrastructure.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Restaurante.Infrastructure.Repositories
{
    public class BaseRepository<T> : IBaseRepository<T> where T : BaseEntity
    {
        protected readonly RestauranteContext _context;
        protected readonly DbSet<T> _entities;

        public BaseRepository(RestauranteContext context)
        {
            _context = context;
            _entities = context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAll()
        {
            return await _entities.ToListAsync();
        }

        public async Task<T?> GetById(int id)
        {
            return await _entities.FindAsync(id);
        }

        public async Task Add(T entity)
        {
            await _entities.AddAsync(entity);
        }

        public Task Update(T entity)
        {
            if (_context.Entry(entity).State == EntityState.Detached)
                _entities.Update(entity);

            return Task.CompletedTask;
        }

        public async Task Delete(int id)
        {
            var entity = await GetById(id);
            if (entity != null)
            {
                _entities.Remove(entity);
            }
        }
    }
}
