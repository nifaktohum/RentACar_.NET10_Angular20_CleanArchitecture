namespace Application.Features.Vehicles.Dto;

public sealed record VehicleImageDto(
    Guid Id,
    string ImageUrl,
    int DisplayOrder,
    bool IsMain,
    string? Description,
    bool IsActive,
    DateTimeOffset? CreatedAt,
    Guid? CreatedBy,
    string? CreatedByName,
    DateTimeOffset? UpdatedAt,
    Guid? UpdatedBy,
    string? UpdatedByName
);
