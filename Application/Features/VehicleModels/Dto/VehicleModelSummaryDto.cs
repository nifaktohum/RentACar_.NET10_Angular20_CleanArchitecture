namespace Application.Features.VehicleModels.Dto;

public sealed record VehicleModelSummaryDto(
    Guid Id,
    string Brand,
    string Name,
    int Stock,
    int AvailableStock
);