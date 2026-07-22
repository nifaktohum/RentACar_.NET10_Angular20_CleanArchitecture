using Domain.Repositories;
using Domain.Entities.Roles;
using GenericRepository;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class RoleRepository : Repository<Role, AppDbContext>, IRoleRepository
{
  private readonly AppDbContext _context;
  public RoleRepository(AppDbContext context) : base(context)
  {
    _context = context;
  }

  public async Task<Role?> GetByNameAsync(string name, CancellationToken _token = default)
  {
    return await _context.Roles.FirstOrDefaultAsync(r => r.Name == name, _token);
  }
}
