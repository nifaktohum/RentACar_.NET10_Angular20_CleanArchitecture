public record UserDto(
    Guid Id,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    string PhoneNumber,
    string BranchName,        // ⬅️ Nullable DEĞİL!
    Guid BranchId,            // ⬅️ Nullable DEĞİL!
    List<string> RoleNames,   // ⬅️ Nullable DEĞİL! (En az 1 rol)
    bool IsActive,
    DateTimeOffset CreatedAt
);