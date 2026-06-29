using Application.Behaviors;
using Application.Common.Helpers;
using MediatR;
using TS.Result;

namespace Application.Features.Permissions.Queries;

[Permission("Permissions.Read")]
public sealed record GetAllPermissionsQuery() : IRequest<Result<List<string>>>;

public sealed class GetAllPermissionsQueryHandler : IRequestHandler<GetAllPermissionsQuery, Result<List<string>>>
{
  public async Task<Result<List<string>>> Handle(GetAllPermissionsQuery _req, CancellationToken _token)
  {
    var permissions = PermissionLoader.GetAllPermissions();
    return Result<List<string>>.Succeed(permissions);
  }
}
