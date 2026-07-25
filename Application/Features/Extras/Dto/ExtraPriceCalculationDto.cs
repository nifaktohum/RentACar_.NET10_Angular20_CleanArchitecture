namespace Application.Features.Extras.Dto;

public sealed record ExtraPriceCalculationDto(
    Guid ExtraProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    int RentalDayCount,
    decimal TotalPrice,
    string PriceType
);