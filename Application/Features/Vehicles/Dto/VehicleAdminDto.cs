namespace Application.Features.Vehicles.Dto;

public sealed record VehicleAdminDto(
    Guid Id,
    string Brand,
    string Model,
    string Year,
    string Plate,
    string Color,
    string FuelType,
    string Transmission,
    int SeatCount,
    int DoorCount,
    int? MinAge,
    decimal DailyPrice,
    int Stock,
    bool IsAvailable,
    string? Description,
    string VehicleTypeName,
    int ImageCount,
    bool IsDeleted,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);