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
        public DbContext Context => _context;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
        protected virtual void Dispose(bool disposing) 
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
                _disposed = true;
            }
        }
    }
}
