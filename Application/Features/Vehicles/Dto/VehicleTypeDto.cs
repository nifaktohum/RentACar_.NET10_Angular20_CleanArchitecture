namespace Application.Features.Vehicles.Dto;

public sealed record VehicleTypeDto(
    Guid Id,
    string Name,
    string? Description,
    string? Icon,
    int DisplayOrder,
    int VehicleCount,
    bool IsActive,           // ✅ BaseEntity'den
    DateTimeOffset CreatedAt // ✅ BaseEntity'den
);