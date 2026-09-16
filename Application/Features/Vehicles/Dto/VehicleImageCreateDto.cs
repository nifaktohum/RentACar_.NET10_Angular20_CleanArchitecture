namespace Application.Features.Vehicles.Dto;

public sealed record VehicleImageCreateDto(
    string ImageUrl,
    bool IsMain = false,
    string? Description = null
);
// VehicleId backend'den gelecek
// DisplayOrder backend'de hesaplanacak
// CreatedBy backend'den gelecek