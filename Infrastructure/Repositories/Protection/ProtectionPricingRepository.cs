using Domain.Entities.Protection;
using Domain.Repositories.Protection;
using GenericRepository;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Protection;

public sealed class ProtectionPricingRepository : Repository<ProtectionPricing, AppDbContext>, IProtectionPricingRepository
{
  private readonly AppDbContext _context;

  public ProtectionPricingRepository(AppDbContext context) : base(context)
  {
    this._context = context;
  }

  public async Task<List<ProtectionPricing>> GetActivePricingsByPackageAsync(Guid packageId, CancellationToken cancellationToken = default)
  {
    var now = DateTimeOffset.UtcNow;

    return await _context.ProtectionPricings
        .Where(p =>
            p.ProtectionPackageId == packageId &&
            p.IsActive &&
            p.ValidityStart <= now &&
            (!p.ValidityEnd.HasValue || p.ValidityEnd >= now))
        .ToListAsync(cancellationToken);
  }

  public async Task<ProtectionPricing?> GetDefaultPricingByPackageAsync(Guid packageId, CancellationToken cancellationToken = default)
  {
    return await _context.ProtectionPricings
     .FirstOrDefaultAsync(p => p.ProtectionPackageId == packageId &&
                              p.IsDefault &&
                              p.IsActive,
                              cancellationToken);
  }
}
