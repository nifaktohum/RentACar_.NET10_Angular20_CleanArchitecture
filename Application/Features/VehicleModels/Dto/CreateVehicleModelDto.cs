namespace Application.Features.VehicleModels.Dto;

public sealed record CreateVehicleModelDto(
    Guid Id,
    string Brand,
    string Name,
    string? Description,
    int Stock,
    int AvailableStock,
    bool IsInStock,
    Guid VehicleTypeId,
    string? VehicleTypeName,
    DateTimeOffset? CreatedAt,
    Guid? CreatedBy,
    string? CreatedByName
);
