using Application.Features.Branches.Dto;

namespace Application.Features.Branches.Queries;

public record GetBranchesQueryResponse(
    List<BranchDto> Items,
    int TotalCount
);
