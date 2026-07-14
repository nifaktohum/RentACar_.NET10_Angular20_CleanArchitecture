namespace Application.Features.Profiles.Dto;

public sealed record GetUserProfileDto(
    Guid Id,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    string PhoneNumber
);