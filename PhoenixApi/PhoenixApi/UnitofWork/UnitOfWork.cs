using Microsoft.EntityFrameworkCore;
using PhoenixApi.Data;

namespace PhoenixApi.UnitofWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApiDbContext _context;
        private bool _disposed = false;
        public UnitOfWork(ApiDbContext context)
        {
            _context = context;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
        protected virtual void Dispose(bool dispose) 
        {
            if (!_disposed)
            {
                if (dispose)
                {
                    _context.Dispose();
                }
                _disposed = true;
            }
        }
    }
}
