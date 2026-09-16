namespace Application.Features.VehicleModels.Dto;

public sealed record VehicleModelStockDto(
    Guid Id,
    string Name,
    string Brand,
    string? Description,
    int Stock,
    int AvailableStock,
    bool IsInStock
);