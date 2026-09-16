namespace Application.Features.Vehicles.Dto;

public sealed record VehicleDto(
    Guid Id,
    string Brand,
    string Model,
    string Year,
    string Plate,
    string Color,
    Guid VehicleTypeId,
    string VehicleTypeName,
    string FuelType,
    string Transmission,
    int SeatCount,
    int DoorCount,
    int? MinAge,
    decimal DailyPrice,
    int Stock,
    bool IsAvailable,
    string? Description,
    string? ImageUrl,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);