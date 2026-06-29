public record UserListDto(
    Guid Id,
    string FirstName,
    string LastName,
    string FullName,
    string PhoneNumber,
    string Email,
    string BranchName,
    List<string> RoleNames,
    bool IsActive
);