namespace Application.Features.Profiles.Queries;

public sealed record GetUserProfileDto(
    Guid Id,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    string PhoneNumber
);