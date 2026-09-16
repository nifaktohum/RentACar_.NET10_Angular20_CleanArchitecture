namespace Application.Features.VehicleModels.Dto;


public sealed record VehicleModelListDto(
    Guid Id,
    string Brand,
    string Name,
    string? Description,
    int Stock,
    int AvailableStock,
    bool IsInStock,
    bool IsActive,
    Guid VehicleTypeId,
    string VehicleTypeName,
    DateTimeOffset CreatedAt
);