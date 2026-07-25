namespace Application.Features.Extras.Dto;

public sealed record CreateRentalExtraDto(
    Guid ExtraProductId,
    int Quantity = 1
);