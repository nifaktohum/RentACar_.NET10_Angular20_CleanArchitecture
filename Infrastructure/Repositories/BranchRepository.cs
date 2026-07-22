using Domain.Entities.Branchs;
using Domain.Repositories;
using GenericRepository;
using Infrastructure.Context;

namespace Infrastructure.Repositories;

internal sealed class BranchRepository : Repository<Branch, AppDbContext>, IBranchRepository
{
  public BranchRepository(AppDbContext context) : base(context)
  {
  }
}
