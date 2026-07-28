namespace Application.Features.Extras.Dto;

public sealed record ExtraSummaryDto(
    Guid Id,
    string Name,
    decimal Price,
    string? Icon,
    string PriceType,
    string Category,
    int DisplayOrder,
    int? StockLimit,
    bool IsRecommended,
    bool IsActive,
    DateTimeOffset CreatedAt,
    string? CreatedByName
);