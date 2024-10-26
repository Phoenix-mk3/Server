using Microsoft.AspNetCore.Mvc;

namespace PhoenixApi.Repositories
{
    public interface IRepository<T> where T : class
    {
        public Task<ActionResult<IEnumerable<T>>> Get();
        public Task<ActionResult<T>> Create(T entity);
        public Task<IActionResult> Update(int id, T entity);
        public Task<IActionResult> Delete(int id);
    }
}
