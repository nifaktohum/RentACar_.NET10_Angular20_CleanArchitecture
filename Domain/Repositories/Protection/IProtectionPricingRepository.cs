using Domain.Entities.Protection;
using Domain.Protection;
using GenericRepository;

namespace Domain.Repositories.Protection;

// ✅ PRICING REPOSITORY!
public interface IProtectionPricingRepository : IRepository<ProtectionPricing>
{
  Task<ProtectionPricing?> GetDefaultPricingByPackageAsync(Guid packageId, CancellationToken cancellationToken = default);
  Task<List<ProtectionPricing>> GetActivePricingsByPackageAsync(Guid packageId, CancellationToken cancellationToken = default);
}
