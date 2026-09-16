namespace Application.Features.Vehicles.Dto;

public sealed record VehicleTypeCreateDto(
    string Name,
    string? Description,
    string? Icon,
    int DisplayOrder = 0
);
// IsActive default true gelecek (backend'de)
// CreatedBy backend'den gelecek
