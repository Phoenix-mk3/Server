using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhoenixApi.Data;
using PhoenixApi.UnitofWork;

namespace PhoenixApi.Repositories.Base
{
    public class RepositoryBase<T, TKey> : IRepository<T, TKey> where T : class
    {
        protected readonly ApiDbContext _context;
        protected readonly DbSet<T> _dbSet;
        public RepositoryBase(ApiDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public virtual async Task<T?> GetByIdAsync(TKey id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public async Task UpdateAsync(TKey id, T entity)
        {
            var existingEntity = await _dbSet.FindAsync(id) ?? throw new KeyNotFoundException("Entity not found");
            _context.Entry(existingEntity).CurrentValues.SetValues(entity);
        }

        public async Task DeleteAsync(TKey id)
        {
            var entity = await _dbSet.FindAsync(id) ?? throw new KeyNotFoundException("Entity not found");
            _dbSet.Remove(entity);
        }
    }
}
