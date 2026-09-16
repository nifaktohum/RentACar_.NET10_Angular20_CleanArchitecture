namespace Application.Features.VehicleTypes.Dto;

public sealed record VehicleTypeDetailDto(
    Guid Id,
    string Name,
    string? Description,
    string? Icon,
    int DisplayOrder,
    bool IsActive,
    int VehicleModelCount,  // ✅ Sadece sayı
    DateTimeOffset? CreatedAt,
    Guid? CreatedBy,
    string? CreatedByName,
    DateTimeOffset? UpdatedAt,
    Guid? UpdatedBy,
    string? UpdatedByName
);
