namespace Application.Features.Vehicles.Dto;

public sealed record VehicleTypeUpdateDto(
    Guid Id,
    string Name,
    string? Description,
    string? Icon,
    int DisplayOrder,
    bool IsActive  // ✅ BaseEntity'den, zorunlu
);
// UpdatedBy backend'den gelecek
