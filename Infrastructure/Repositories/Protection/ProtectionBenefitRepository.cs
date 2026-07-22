using Domain.Entities.Protection;
using Domain.Repositories.Protection;
using GenericRepository;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Protection;

public sealed class ProtectionBenefitRepository : Repository<ProtectionBenefit, AppDbContext>, IProtectionBenefitRepository
{
  private readonly AppDbContext _context;

  public ProtectionBenefitRepository(AppDbContext context) : base(context)
  {
    this._context = context;
  }

  public void Attach(object entity)
  {
    _context.Attach(entity);
  }

  public async Task<ProtectionBenefit?> GetByIdBenefitAsync(Guid id, CancellationToken cancellationToken = default)
  {
    return await _context.ProtectionBenefits
        .Where(b => b.Id == id && !b.IsDeleted)
        .Include(b => b.Category)
        .FirstOrDefaultAsync(cancellationToken);
  }

  public async Task<List<ProtectionBenefit>> GetBenefitsByIdsAsync(List<Guid> ids, CancellationToken cancellationToken = default)
  {
    return await Where(b => ids.Contains(b.Id) && b.IsActive && !b.IsDeleted)
       .Include(b => b.Category) 
       .OrderBy(b => b.DisplayOrder)
       .ToListAsync(cancellationToken);
  }

  public async Task<List<ProtectionBenefit>> GetAllBenefitsAsync(CancellationToken cancellationToken = default)
  {
    return await _context.ProtectionBenefits
            .Where(b => b.IsActive)
            .Include(b => b.Category)
            .OrderBy(b => b.DisplayOrder)
            .ToListAsync(cancellationToken);
  }

  // Bana sadece şu kategorideki (örneğin: Lastik veya Cam) özellikleri getir
  public async Task<List<ProtectionBenefit>> GetBenefitsByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
  {
    return await Where(b => b.CategoryId == categoryId && b.IsActive && !b.IsDeleted)
                .Include(b => b.Category)
                .OrderBy(b => b.DisplayOrder)
                .ToListAsync(cancellationToken);
  }

  public async Task<List<ProtectionBenefit>> GetActiveBenefitsAsync(CancellationToken cancellationToken = default)
  {
    return await Where(b => b.IsActive && !b.IsDeleted)
          .Include(b => b.Category)
          .OrderBy(b => b.DisplayOrder)
          .ToListAsync(cancellationToken);
  }

}
