namespace Application.Features.Customers.Profile.Dto;

public sealed record CustomerProfileDto(
    Guid Id,
    string Type,                  // "Individual" / "Corporate"
    string Email,
    string PhoneNumber,
    string Address,
    string City,
    string Country,
    string? PostalCode,

    // Bireysel
    string? FirstName,
    string? LastName,
    string? IdentityNumber,
    DateOnly? BirthDate,
    string? Gender,
    int? Age,

    // Kurumsal
    string? CompanyName,
    string? TaxNumber,
    string? TaxOffice,

    // Ehliyet
    string? DriverLicenseNumber,
    DateOnly? DriverLicenseIssueDate,
    string? DriverLicenseClass,
    bool HasValidDriverLicense,

    // Finansal
    int? FindeksScore,

    // Durum
    bool IsEmailVerified,
    bool IsPhoneVerified,
    bool IsBlacklisted,
    string? BlacklistReason,

    // İş kuralları
    bool CanRent,
    bool CanRentLuxury,

    // Audit
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);