using Domain.Roles;
using GenericRepository;

namespace Domain.Repositories;

public interface IRoleRepository: IRepository<Role>
{
  Task<Role?> GetByNameAsync(string name, CancellationToken _token = default);
}
