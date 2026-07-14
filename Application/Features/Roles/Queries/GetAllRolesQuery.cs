using Application.Behaviors;
using Application.Features.Roles.Queries.Dto;
using Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TS.Result;

namespace Application.Features.Roles.Queries;

[Permission("Roles.Read")]
public sealed record GetAllRolesQuery() : IRequest<Result<List<RolesDto>>>;

public sealed class GetAllRolesQueryHandler(
                                    IRoleRepository _roleRepo
                            ) : IRequestHandler<GetAllRolesQuery, Result<List<RolesDto>>>
{
  public async Task<Result<List<RolesDto>>> Handle(GetAllRolesQuery _req, CancellationToken _token)
  {
    var roles = await _roleRepo.GetAll().ToListAsync(_token);

    var response = roles.Select(r => new RolesDto(r.Id, r.Name, r.Description)).ToList();

    return Result<List<RolesDto>>.Succeed(response);
  }
}
