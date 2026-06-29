using Application.Behaviors;
using Domain.Repositories;
using Domain.Roles;
using MediatR;
using TS.Result;

namespace Application.Features.Users.Query;

[Permission("Users.Read")]
public record GetUserByIdQuery(Guid Id) : IRequest<Result<UserDetailDto>>;

public sealed class GetUserByIdQueryHandler(
                          IUserRepository _userRepo
                    ): IRequestHandler<GetUserByIdQuery, Result<UserDetailDto>>
{
  public async Task<Result<UserDetailDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
  {
    // 1. Kullanıcıyı Include'larla birlikte getir
    var user = await _userRepo.GetUserWithDetailsByIdAsync(request.Id, cancellationToken);
    // 2. Kullanıcı yoksa hata döndür
    if (user is null) return Result<UserDetailDto>.Failure("Kullanıcı bulunamadı.");

    // 2. İzinleri getir
    var permissions = await _userRepo.GetUserPermissionsAsync(user.Id, cancellationToken);
    var auditIds = new List<Guid> { user.CreatedBy };
    var userNames = await _userRepo.GetUserNamesByIdsAsync(auditIds, cancellationToken);

    // 3. Entity'den DTO'ya dönüştür
    // 3. DTO'ya dönüştür
    var dto = new UserDetailDto(
        user.Id,
        user.FullName,
        user.Email,
        user.PhoneNumber,
        user.Branch?.Name ?? "Şube Yok",
        user.BranchId ?? Guid.Empty,
        user.IsActive,
        user.IsDeleted,
        user.CreatedAt,
        user.CreatedBy,
        userNames.GetValueOrDefault(user.CreatedBy) ?? "Sistem",
        user.UpdatedAt,
        user.UpdatedBy ?? Guid.Empty,
        userNames.GetValueOrDefault(user.UpdatedBy ?? Guid.Empty) ?? "Sistem",
        user.UserRoles.Select(ur => new UserRoleDto(
            ur.RoleId,
            ur.Role.Name,
            ur.Role.Description
        )).ToList(),
        permissions.Select(p => new UserPermissionDto(
            Guid.NewGuid(),
            p,
            null
        )).ToList()
    );

    return Result<UserDetailDto>.Succeed(dto);
  }
}
