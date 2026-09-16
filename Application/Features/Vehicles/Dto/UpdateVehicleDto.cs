namespace Application.Features.Vehicles.Dto;

public sealed record UpdateVehicleDto(
    Guid Id,
    string Brand,
    string Model,
    string Year,
    string Plate,
    string Color,
    Guid VehicleModelId,
    string FuelType,
    string Transmission,
    int SeatCount,
    int DoorCount,
    int? MinAge,
    decimal DailyPrice,
    string? Description,
    string? ImageUrl,
    bool IsActive,
    DateTimeOffset? CreatedAt,        // Oluşturulma tarihi
    Guid? CreatedBy,                  // Oluşturanın ID'si
    string? CreatedByName,            // Audit: Oluşturanın görünen adı
    DateTimeOffset? UpdatedAt,       // Son güncelleme tarihi
    Guid? UpdatedBy,                 // Güncelleyenin ID'si
    string? UpdatedByName
);
