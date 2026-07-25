namespace Application.Features.Extras.Dto;

public sealed record RentalExtraDto(
    Guid Id,
    Guid RentalId,
    Guid ExtraProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal TotalPrice,
    string PriceType,         // "Daily" veya "Rental"
    DateTimeOffset CreatedAt,
    Guid CreatedBy
);