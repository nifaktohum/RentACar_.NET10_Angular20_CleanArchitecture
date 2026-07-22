using Domain.Repositories;
using Domain.Repositories.Protection;
using GenericRepository;
using MediatR;
using Microsoft.Extensions.Configuration;
using TS.Result;

namespace Application.Features.ProtectionPackages.Commands;

public sealed record ToggleProtectionPackageStatusCommand( Guid Id, bool IsActive ) : IRequest<Result<Unit>>;

public sealed class ToggleProtectionPackageStatusCommandHandler(
                        IProtectionPackageRepository _packageRepo,
                        IUnitOfWork _unit,
                        IUserRepository _userRepo,
                        IConfiguration _config
                    ) : IRequestHandler<ToggleProtectionPackageStatusCommand, Result<Unit>>
{
  public async Task<Result<Unit>> Handle(ToggleProtectionPackageStatusCommand _req, CancellationToken _token)
  {
    // 1. Paket var mı?
    var package = await _packageRepo
        .FirstOrDefaultAsync(p => p.Id == _req.Id && !p.IsDeleted, _token);

    if (package is null) return Result<Unit>.Failure(404, "Paket bulunamadı.");

    // 2. Kullanıcı ID
    var userId = _userRepo.GetCurrentUserId();
    if (userId == Guid.Empty) userId = Guid.Parse(_config["SeedData:AdminUserId"]!);

    // 3. Durumu değiştir
    package.SetActiveStatus(_req.IsActive);
    package.UpdateMetadata(userId);

    // 4. Kaydet
    _packageRepo.Update(package);
    await _unit.SaveChangesAsync(_token);

    return Result<Unit>.Succeed(Unit.Value);


    throw new NotImplementedException();
  }
}