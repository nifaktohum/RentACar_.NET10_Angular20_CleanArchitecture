using Application.Behaviors;
using Application.Features.Permissions.Commands;
using Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TS.Result;

namespace Application.Features.Permissions.Queries;

// Güvenlik mühürünü de unutmayalım kanka, bu sorguyu sadece okuma yetkisi olanlar atsın
[Permission("Permissions.Read")]
public sealed record GetPermissionsByRoleIdQuery(Guid RoleId) : IRequest<Result<List<RolePermissionQueryDto>>>
{
  public bool IsAdmin { get; set; }
}

public sealed class GetPermissionsByRoleIdQueryHandler(
    IPermissionRepository _permissionRepo,
    IConfiguration _configuration
) : IRequestHandler<GetPermissionsByRoleIdQuery, Result<List<RolePermissionQueryDto>>>
{
  public async Task<Result<List<RolePermissionQueryDto>>> Handle(
      GetPermissionsByRoleIdQuery request,
      CancellationToken cancellationToken)
  {
    var allPermissions = await _permissionRepo
        .AsQueryable()
        .IgnoreQueryFilters()
        .Include(x => x.PermissionRoles)
        .ToListAsync(cancellationToken);

    var adminRoleId = Guid.Parse(_configuration["SeedData:AdminRoleId"]!);
    var isAdminRole = request.RoleId == adminRoleId;

    var result = new List<RolePermissionQueryDto>();

      Console.WriteLine($"RoleId: {request.RoleId}");
      Console.WriteLine($"Permission Count: {allPermissions.Count}");
    foreach (var permission in allPermissions)
    {
      Console.WriteLine( $"{permission.Name} => {permission.PermissionRoles.Count}" );
      bool hasRole = permission.PermissionRoles
          .Any(pr => pr.RoleId == request.RoleId);

      result.Add(new RolePermissionQueryDto(
          Id: permission.Id,
          Name: permission.Name,
          Description: permission.Description,

          // Admin ise tüm permissionlar açık görünür
          IsActive: isAdminRole || hasRole,

          // Sadece Admin rolünde switchler kilitli
          IsSystemDisabled: isAdminRole
      ));
    }

    return Result<List<RolePermissionQueryDto>>.Succeed(
        result.OrderBy(x => x.Name).ToList()
    );
  }
}