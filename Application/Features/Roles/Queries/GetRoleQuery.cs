using Application.Behaviors;
using Application.Features.Roles.Queries.Dto;
using Domain.Repositories;
using FluentValidation;
using MediatR;
using TS.Result;

namespace Application.Features.Roles.Queries;

[Permission("Roles.Read")]
public sealed record GetRoleQuery(Guid Id) : IRequest<Result<RolesDto>>;

public sealed class GetRoleQueryValidation : AbstractValidator<GetRoleQuery>
{
  public GetRoleQueryValidation()
  {
    RuleFor(r => r.Id)
        .NotEmpty().WithMessage("Sorgulanacak rolün Id bilgisi boş olamaz!");
  }
}

public sealed class GetRoleQueryHandler(
                              IRoleRepository _roleRepo
                    ) : IRequestHandler<GetRoleQuery, Result<RolesDto>>
{
  public async Task<Result<RolesDto>> Handle(GetRoleQuery _req, CancellationToken _token)
  {
    var role = await _roleRepo.FirstOrDefaultAsync(r => r.Id == _req.Id, _token);

    var response = new RolesDto(role.Id, role.Name, role.Description);

    return Result<RolesDto>.Succeed(response);
  }
}