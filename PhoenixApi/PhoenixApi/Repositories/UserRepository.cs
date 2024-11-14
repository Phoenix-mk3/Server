using Microsoft.EntityFrameworkCore;
using PhoenixApi.Data;
using PhoenixApi.Models;
using PhoenixApi.Repositories.Base;

namespace PhoenixApi.Repositories
{
    public interface IUserRepository : IRepository<User, Guid>
    {
        Task<User> GetUserByClientIdAsync(Guid clientId);
    }
    public class UserRepository(ApiDbContext context): RepositoryBase<User, Guid>(context), IUserRepository
    {
        public async Task<User> GetUserByClientIdAsync(Guid clientId)
        {
            User? user = await _dbSet.FirstOrDefaultAsync(u => u.ClientId == clientId && u.IsActive);
            return user;
        }
    }
}
