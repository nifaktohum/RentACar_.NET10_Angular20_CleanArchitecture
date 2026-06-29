using Application.Behaviors;
using Domain.Repositories;
using FluentValidation;
using GenericRepository;
using MediatR;
using TS.Result;

namespace Application.Features.Users.Commands;

[Permission("Users.Delete")]
public sealed record DeleteUserCommand(
                          Guid Id
                      ) : IRequest<Result<bool>>;

public sealed class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
{
  public DeleteUserCommandValidator()
  {
    // 1. En azından ID'nin boş gelmediğinden emin olalım
    RuleFor(p => p.Id)
        .NotEmpty().WithMessage("Silinecek kullanıcı ID'si boş olamaz.");
    
  }
}

public sealed class DeleteUserCommandHandler(
                        IUserRepository _userRepo,
                        IUnitOfWork _unit
                    ) : IRequestHandler<DeleteUserCommand, Result<bool>>
{
  public async Task<Result<bool>> Handle(DeleteUserCommand _req, CancellationToken _token)
  {
    // 1. Kullanıcıyı bul
    var user = await _userRepo.FirstOrDefaultAsync(u => u.Id == _req.Id, _token);
    var userId = _userRepo.GetCurrentUserId();
    string[] roleName = ["Admin"];
    var isAdmin = await _userRepo.HasRoleAsync(userId, roleName, _token);

    if (isAdmin) return Result<bool>.Failure("Sistem Admin silinemez.");

    if (user is null) return Result<bool>.Failure("Kullanıcı bulunamadı.");
    // 2. İş Kuralı: Kendini silmeye çalışıyor mu?
    if (user.Id == userId) return Result<bool>.Failure("Kendi hesabınızı silemezsiniz.");

    // 2. İş Kuralı: Kendini silmeye çalışıyor mu? (Örn: LoggedInUserId kontrolü buraya gelebilir)

    // 3. Soft Delete işlemini başlat
    // BaseEntity'den gelen SoftDelete metodunu ve User'daki Logout işlemini birleştiriyoruz
    user.SoftDelete(userId);
    user.UpdateSecurityStamp();

    // 4. Kaydet
    await _unit.SaveChangesAsync(_token);

    return Result<bool>.Succeed(true);
  }
}