namespace Application.Features.Customers.Management.Dto;

public sealed record CustomerListDto(
    Guid Id,
    string Type,
    string Email,
    string PhoneNumber,
    string? FirstName,
    string? LastName,
    string? CompanyName,
    string City,
    string Country,
    bool IsEmailVerified,
    bool IsPhoneVerified,
    bool IsBlacklisted,
    int? FindeksScore,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);
