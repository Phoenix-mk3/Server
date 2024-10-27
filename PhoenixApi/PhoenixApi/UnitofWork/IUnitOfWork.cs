using Microsoft.EntityFrameworkCore;

namespace PhoenixApi.UnitofWork
{
    public interface IUnitOfWork : IDisposable
    {
        public Task SaveChangesAsync();
    }
}
