using Microsoft.EntityFrameworkCore;

namespace PhoenixApi.UnitofWork
{
    public interface IUnitOfWork : IDisposable
    {
        DbContext Context { get; }
        public Task SaveChangesAsync();
    }
}
