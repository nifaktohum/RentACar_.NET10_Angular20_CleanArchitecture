using Application.Features.VehicleImages.Dto;

namespace Application.Features.Vehicles.Dto;

public sealed record CreateVehicleDto(
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
    bool IsAvailable,
    string? Description,
    Guid VehicleModelId,
    string? VehicleModelName,
    string? VehicleTypeName,
List<VehicleImageDto> Images,
    DateTimeOffset? CreatedAt,        // Oluşturulma tarihi
    Guid? CreatedBy,                  // Oluşturanın ID'si
    string? CreatedByName
);