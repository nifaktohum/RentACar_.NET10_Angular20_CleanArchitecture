using Application.Behaviors;
using Application.Services;
using Domain.Repositories;
using GenericRepository;
using MediatR;
using TS.Result;

namespace Application.Features.Login.Commands;

public sealed record LogoutAllDevicesCommand: IRequest<Result<string>>;

public sealed class LogoutAllDevicesCommandHandler(
                        IUserContext _userContext,
                        IUserRepository _userRepo,
                        IUnitOfWork _unit
) : IRequestHandler<LogoutAllDevicesCommand, Result<string>>
{
  public async Task<Result<string>> Handle(LogoutAllDevicesCommand _req, CancellationToken _token)
  {
    // 1. Akıllı UserContext üzerinden istek atan kullanıcının ID'sini tereyağından kıl çeker gibi alıyoruz
    Guid userId = _userContext.GetUserId();

    // 2. Kullanıcıyı veritabanından çekiyoruz
    var user = await _userRepo.FirstOrDefaultAsync(u => u.Id == userId, _token);
    if (user is null) { return Result<string>.Failure(statusCode: 404, "Kullanıcı bulunamadı kanka."); }

    // Bu satır çalıştığı an veritabanındaki SecurityStamp değişiyor ve bekleyen tüm sıfırlama kodları uçuyor!
    user.LogoutAllDevices();
    await _unit.SaveChangesAsync(_token);

    // 5. İşlem başarılı, Angular tarafına müjdeyi veriyoruz
    return Result<string>.Succeed("Tüm cihazlardan başarıyla çıkış yapıldı. Diğer tüm açık oturumlar sonlandırıldı.");
  }
}

