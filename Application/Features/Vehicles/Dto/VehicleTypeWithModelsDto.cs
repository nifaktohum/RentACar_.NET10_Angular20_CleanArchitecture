namespace Application.Features.VehicleModels.Dto;

public sealed record VehicleTypeWithModelsDto(
    Guid Id,
    string Name,
    string? Description,
    string? Icon,
    int DisplayOrder,
    int TotalModelCount,
    List<VehicleModelSummaryDto> Models,
    bool IsActive,
    DateTimeOffset CreatedAt
);