namespace Application.Features.Vehicles.Dto;

public sealed record VehicleListDto(
    Guid Id,
    string Brand,
    string Model,
    string Description,
    string Year,
    string Plate,
    string Color,
    string VinNumber,           // 🆕 Eklendi (Admin arama/filtreleme için)
    string FuelType,
    string Transmission,
    string TractionType,        // 🆕 Eklendi
    int SeatCount,
    int DoorCount,
    decimal DailyPrice,
    int CurrentKilometer,       // 🆕 Eklendi
    string Status,              // 🆕 Eklendi (Available, Rented, Maintenance vb.)
    bool IsAvailable,           // Computed property (Status == Available)
    string? VehicleTypeName,
    string? MainImageUrl,
    bool IsActive,
    DateTimeOffset? CreatedAt   // Nullable yaptık (bazı yerlerde null gelebilir)
);