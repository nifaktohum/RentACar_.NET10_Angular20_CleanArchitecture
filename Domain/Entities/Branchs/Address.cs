namespace Domain.Entities.Branchs;

public sealed record Address(
    string City,
    string District,
    string FullAddress,
    string Phone1,
    string? Phone2,
    string Email
);