namespace Application.Features.VehicleTypes.Dto;

public record UpdateVehicleTypeDto(
    Guid Id,
    string Name,
    string? Description,
    string? Icon,
    int DisplayOrder,
    DateTimeOffset? UpdatedAt,       
    Guid? UpdatedBy,                 
    string? UpdatedByName
);
