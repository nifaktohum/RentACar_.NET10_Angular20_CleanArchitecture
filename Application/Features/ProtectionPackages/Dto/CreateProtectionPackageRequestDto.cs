using Domain.Protection;

namespace Application.Features.ProtectionPackages.Dto;

public sealed record CreateProtectionPackageRequestDto(
    Guid Id,
    string Name,
    string? Description,
    string? Icon,
    int DisplayOrder,
    bool IsRecommended,
    int StarRating,
    string ProtectionLevel,      // ✅ Enum string olarak!
    string DeductibleType,       // ✅ Enum string olarak!
    List<Guid> BenefitIds,
    CreateProtectionPricingRequestDto Pricing,
    bool IsActive,
    DateTimeOffset CreatedAt,                        // Oluşturulma tarihi
    Guid CreatedBy,                                  // Oluşturan kullanıcının ID'si
    string CreatedByName                          // Oluşturan kullanıcının görünen adı

);

public sealed record CreateProtectionPricingRequestDto(
    decimal? DailyPrice,
    decimal? DeductibleAmount,
    bool IsDefault,
    DateTimeOffset? ValidityStart,
    DateTimeOffset? ValidityEnd
);
