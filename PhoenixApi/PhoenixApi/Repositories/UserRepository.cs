using PhoenixApi.Data;
using PhoenixApi.Models;
using PhoenixApi.Repositories.Base;

namespace PhoenixApi.Repositories
{
    public interface IUserRepository : IRepository<User, Guid>
    {

    }
    public class UserRepository(ApiDbContext context): RepositoryBase<User, Guid>(context), IUserRepository
    {

    }
}
