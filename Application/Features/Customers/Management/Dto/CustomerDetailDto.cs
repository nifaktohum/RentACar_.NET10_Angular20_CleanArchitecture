namespace Application.Features.Customers.Management.Dto;

public sealed record CustomerDetailDto(
    // ==================== KİMLİK ====================
    Guid Id,
    string Type,                  // "Individual" / "Corporate"

    // ==================== BİREYSEL ====================
    string? FirstName,
    string? LastName,
    string? IdentityNumber,
    DateOnly? BirthDate,
    string? Gender,
    int? Age,

    // ==================== KURUMSAL ====================
    string? CompanyName,
    string? TaxNumber,
    string? TaxOffice,

    // ==================== İLETİŞİM ====================
    string Email,
    string PhoneNumber,
    string Address,
    string City,
    string Country,
    string? PostalCode,

    // ==================== SÜRÜCÜ BELGESİ ====================
    string? DriverLicenseNumber,
    DateOnly? DriverLicenseIssueDate,
    string? DriverLicenseClass,
    bool HasValidDriverLicense,

    // ==================== FİNANSAL ====================
    int? FindeksScore,

    // ==================== DURUM ====================
    bool IsEmailVerified,
    bool IsPhoneVerified,
    bool IsBlacklisted,
    string? BlacklistReason,
    bool IsActive,

    // ==================== İŞ KURALLARI (Angular guard için) ====================
    bool CanRent,
    bool CanRentLuxury,

    // ==================== AUDIT ====================
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);