namespace Application.Features.VehicleModels.Dto;

public sealed record UpdateVehicleModelDto(
    Guid Id,
    string Brand,
    string Name,
    string? Description,
    int Stock,
    int AvailableStock,
    bool IsInStock,
    Guid VehicleTypeId,
    string VehicleTypeName,
    DateTimeOffset? UpdatedAt,
    Guid? UpdatedBy,
    string? UpdatedByName
);
