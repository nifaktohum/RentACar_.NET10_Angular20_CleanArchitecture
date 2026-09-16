namespace Application.Features.Vehicles.Dto;

public sealed record VehicleSummaryDto(
    Guid Id,
    string Brand,
    string Model,
    string Year,
    string Plate,
    string Color,
    Guid VehicleTypeId,
    string VehicleTypeName,
    string? MainImageUrl,
    decimal DailyPrice,
    int Stock,
    bool IsAvailable,
    bool IsActive
);