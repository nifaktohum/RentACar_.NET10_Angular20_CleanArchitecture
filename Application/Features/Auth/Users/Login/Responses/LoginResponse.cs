namespace Application.Features.Auth.Users.Login.Responses;

public sealed record LoginResponse(
    string Token,
    Guid UserId,
    string FullName,
    string Email,
    List<string> Roles,
    string BranchName,
    List<string> Permissions);