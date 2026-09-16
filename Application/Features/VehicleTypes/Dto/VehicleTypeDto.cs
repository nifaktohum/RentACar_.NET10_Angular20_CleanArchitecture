namespace Application.Features.VehicleTypes.Dto;

public record VehicleTypeDto(
    Guid Id,
    string Name,
    string? Description,
    string? Icon,
    int DisplayOrder,
    bool IsActive,
    int VehicleModelCount,
    DateTimeOffset CreatedAt
);
