using Domain.Branchs;

namespace Application.Features.Branches.Dto;

public sealed record BranchDto(
    Guid Id,
    string Name,
    Address Address,
    DateTimeOffset CreatedAt,
    string CreatedByName,       
    DateTimeOffset? UpdatedAt,
    string? UpdatedByName,     
    bool IsActive
);