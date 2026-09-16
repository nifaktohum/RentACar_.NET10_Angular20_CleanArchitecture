namespace Application.Features.Vehicles.Dto;

public sealed record VehicleDetailDto(
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
    bool IsActive,              
    DateTimeOffset? CreatedAt,
    Guid? CreatedBy,
    string? CreatedByName,
    DateTimeOffset? UpdatedAt,
    Guid? UpdatedBy,
    string? UpdatedByName
);