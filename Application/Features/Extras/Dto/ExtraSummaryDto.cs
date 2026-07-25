namespace Application.Features.Extras.Dto;

public sealed record ExtraSummaryDto(
    Guid Id,
    string Name,
    decimal Price,
    string PriceType,
    string Category,
    int DisplayOrder,
    bool IsRecommended,
    bool IsActive,
    DateTimeOffset CreatedAt,
    string? CreatedByName
);