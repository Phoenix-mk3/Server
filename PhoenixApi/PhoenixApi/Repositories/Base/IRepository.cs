using Microsoft.AspNetCore.Mvc;

namespace PhoenixApi.Repositories.Base
{
    public interface IRepository<T, TKey> where T : class
    {
        public Task<IEnumerable<T>> GetAllAsync();
        public Task<T?> GetByIdAsync(TKey id);
        public Task AddAsync(T entity);
        public Task UpdateAsync(TKey id, T entity);
        public Task DeleteAsync(TKey id);

    }
}
