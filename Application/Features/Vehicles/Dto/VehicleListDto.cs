namespace Application.Features.Vehicles.Dto;

public sealed record VehicleListDto(
    Guid Id,
    string Brand,
    string Model,
    string Description,
    string Year,
    string Plate,
    string Color,
    string FuelType,
    string Transmission,
    int SeatCount,
    int DoorCount,
    decimal DailyPrice,
    bool IsAvailable,
    string? VehicleTypeName,
    string? MainImageUrl,
    bool IsActive,
    DateTimeOffset CreatedAt
);