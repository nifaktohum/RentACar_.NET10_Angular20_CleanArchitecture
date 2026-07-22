using Domain.Entities.Protection;
using Domain.Repositories.Protection;
using GenericRepository;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Protection;

public sealed class BenefitCategoryRepository : Repository<BenefitCategory, AppDbContext>, IBenefitCategoryRepository
{
  private readonly AppDbContext _context;

  public BenefitCategoryRepository(AppDbContext context) : base(context)
  {
    this._context = context;
  }
  /// Aktif olan tüm kategorileri getirir (IsActive = true ve IsDeleted = false)
  public async Task<List<BenefitCategory>> GetActiveCategoriesAsync(CancellationToken cancellationToken = default)
  {
    return await Where(c => c.IsActive && !c.IsDeleted)
           .OrderBy(c => c.DisplayOrder)
           .ToListAsync(cancellationToken);
  }
  /// Slug'a göre kategori getirir
  public async Task<BenefitCategory?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
  {
    return await FirstOrDefaultAsync(c => c.Slug == slug && !c.IsDeleted, cancellationToken);
  }
  /// Kategori ve altındaki Benefit'leri birlikte getirir
  public async Task<BenefitCategory?> GetCategoryWithBenefitsAsync(Guid id, CancellationToken cancellationToken = default)
  {
    return await _context.BenefitCategories
                            .Include(c => c.Benefits
                            .Where(b => !b.IsDeleted))
                            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);
  }
}
