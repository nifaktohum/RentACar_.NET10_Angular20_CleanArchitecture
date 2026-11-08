namespace Application.Features.Vehicles.Dto;

public sealed record VehicleReportDto(
    Guid Id,
    string Brand,
    string Model,
    string Year,
    string Plate,
    string VinNumber,               // 🆕 Eklendi (Şasi No)
    string Color,
    string FuelType,
    string Transmission,
    int SeatCount,
    int DoorCount,
    decimal DailyPrice,
    int CurrentKilometer,           // 🆕 Eklendi (Güncel KM)
    int Stock,                      // VehicleModel.Stock (Araç Modeli Stoğu)
    bool IsAvailable,
    string VehicleTypeName,
    string? MainImageUrl,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    string Status                   // "Müsait", "Kirada", "Bakımda", "Serviste", "Pasif"
);