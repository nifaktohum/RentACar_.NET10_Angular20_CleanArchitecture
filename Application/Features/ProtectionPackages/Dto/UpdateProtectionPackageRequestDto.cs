using Domain.Entities.Protection;

namespace Application.Features.ProtectionPackages.Dto;

public sealed record UpdateProtectionPackageRequestDto(
    Guid Id,
    string Name,
    string? Description,
    string? Icon,
    int DisplayOrder,
    bool IsRecommended,
    int StarRating,
    ProtectionLevel ProtectionLevel,
    DeductibleType DeductibleType,
    List<Guid> BenefitIds,
    UpdateProtectionPricingRequestDto Pricing,
    bool IsActive
);

public sealed record UpdateProtectionPricingRequestDto(
    Guid? PricingId,
    decimal? DailyPrice,
    decimal? DeductibleAmount,
    bool IsDefault,
    DateTimeOffset? ValidityStart,
    DateTimeOffset? ValidityEnd,
    bool IsActive
);
