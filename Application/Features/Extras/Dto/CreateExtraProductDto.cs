using Domain.Entities.Extras.Enum;

namespace Application.Features.Extras.Dto;

public sealed record CreateExtraProductDto(
    string Name,
    string? Description,
    string? Icon,
    decimal Price,
    PriceType PriceType,
    ExtraCategory Category,
    int DisplayOrder,
    bool IsRecommended,
    int? MinAge,
    string? AgeRange,
    int? StockLimit,
    bool IsActive
);