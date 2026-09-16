namespace Application.Features.Vehicles.Dto;

public sealed record VehicleReportDto(
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
    decimal DailyPrice,
    int Stock,
    bool IsAvailable,
    string VehicleTypeName,
    string? MainImageUrl,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    string Status // "Aktif", "Pasif", "Dolu", "Müsait" gibi
);