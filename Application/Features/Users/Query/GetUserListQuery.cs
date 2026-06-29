using Application.Behaviors;
using Application.Services;
using Domain.Repositories;
using MediatR;
using TS.Result;

namespace Application.Features.Users.Query;

[Permission("Users.Read")]
public sealed record GetUserListQuery() : IRequest<Result<List<UserListDto>>>;

public sealed class GetUserListQueryHandler(
    IUserRepository _userRepo,
    IUserContext _userContext
) : IRequestHandler<GetUserListQuery, Result<List<UserListDto>>>
{
  public async Task<Result<List<UserListDto>>> Handle(GetUserListQuery _req, CancellationToken _token)
  {
    // 1. Giriş yapan kullanıcının bilgilerini al
    var currentUserId = _userContext.GetUserId();
    var isAdmin = await _userRepo.IsUserInRoleAsync(currentUserId, "Admin", _token);

    // 2. Kullanıcıları getir
    var users = isAdmin
        ? await _userRepo.GetUsersWithDetailsAsync(_token)                    // ✅ Admin: Tümü
        : await _userRepo.GetUsersByBranchIdAsync(_userContext.GetBranchId(), _token); // ✅ Diğer: Sadece kendi şubesi

    // 3. Entity'den DTO'ya dönüştür
    var userList = users.Select(user => new UserListDto(
        user.Id,
        user.FirstName,
        user.LastName,
        user.FullName,
        user.PhoneNumber,
        user.Email,
        user.Branch?.Name ?? "Şube Yok",
        user.UserRoles?.Select(ur => ur.Role?.Name ?? "Rol Tanımsız").ToList() ?? new List<string>(),
        user.IsActive
    )).ToList();

    return Result<List<UserListDto>>.Succeed(userList);
  }
}