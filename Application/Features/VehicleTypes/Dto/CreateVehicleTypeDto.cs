namespace Application.Features.VehicleTypes.Dto;

public record CreateVehicleTypeDto(
    Guid Id,
    string Name,
    string? Description,
    string? Icon,
    int DisplayOrder,
    DateTimeOffset CreatedAt
);