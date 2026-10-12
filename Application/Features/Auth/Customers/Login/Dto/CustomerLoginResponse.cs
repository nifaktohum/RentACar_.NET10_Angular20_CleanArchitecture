namespace Application.Features.Auth.Customers.Login.Dto;

public sealed record CustomerLoginResponse(
    string Token,
    string RefreshToken,
    Guid CustomerId,
    string FullName,
    string Email,
    string CustomerType,       // "Individual" / "Corporate"
    bool IsEmailVerified,
    bool IsPhoneVerified,
    bool IsBlacklisted,        
    string? BlacklistReason    
);