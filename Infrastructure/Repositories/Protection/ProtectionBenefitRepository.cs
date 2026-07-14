using Domain.Entities.Protection;
using Domain.Protection;
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

  public async Task<ProtectionBenefit?> GetByIdBenefitsAsync(Guid id, CancellationToken cancellationToken = default)
  {
    return await _context.ProtectionBenefits
        .Where(b => b.Id == id && !b.IsDeleted)
        .FirstOrDefaultAsync(cancellationToken);
  }

  public async Task<List<ProtectionBenefit>> GetAllBenefitsAsync(CancellationToken cancellationToken = default)
  {
    return await _context.ProtectionBenefits
            .Where(b => b.IsActive)
            .OrderBy(b => b.DisplayOrder)
            .ToListAsync(cancellationToken);
  }

  // Bana sadece şu kategorideki (örneğin: Lastik veya Cam) özellikleri getir
  public async Task<List<ProtectionBenefit>> GetBenefitsByCategoryAsync(BenefitCategory category, CancellationToken cancellationToken = default)
  {
    return await _context.ProtectionBenefits
            .Where(b => b.Category == category && b.IsActive)
            .OrderBy(b => b.DisplayOrder)
            .ToListAsync(cancellationToken);
  }
}
