using Application.Behaviors;
using Domain.Repositories;
using Domain.Repositories.Protection;
using FluentValidation;
using GenericRepository;
using MediatR;
using Microsoft.Extensions.Configuration;
using TS.Result;

namespace Application.Features.ProtectionPackages.Commands;

[Permission("ProtectionPackage.Delete")]
public sealed record DeleteProtectionPackageCommand(Guid Id) : IRequest<Result<Unit>>;

public sealed class DeleteProtectionPackageCommandValidator : AbstractValidator<DeleteProtectionPackageCommand>
{
  public DeleteProtectionPackageCommandValidator()
  {
    RuleFor(x => x.Id)
    .NotEmpty().WithMessage("Paket ID boş olamaz.")
    .NotNull().WithMessage("Paket ID null olamaz.");
  }
}

public sealed class DeleteProtectionPackageCommandHandlerf(
                            IProtectionPackageRepository _packageRepo,
                            IUnitOfWork _unit,
                            IUserRepository _userRepo,
                            IConfiguration _config
                    ) : IRequestHandler<DeleteProtectionPackageCommand, Result<Unit>>
{
  public async Task<Result<Unit>> Handle(DeleteProtectionPackageCommand _req, CancellationToken _token)
  {
    // 1. Paket var mı?
    var package = await _packageRepo.FirstOrDefaultAsync(b => b.Id == _req.Id && !b.IsDeleted, _token);

    if (package is null) return Result<Unit>.Failure(404, "Paket bulunamadı.");

    // 2. Paket zaten silinmiş mi?
    if (package.IsDeleted) return Result<Unit>.Failure(400, "Bu paket zaten silinmiş.");

    // 3. Kullanıcı ID'si
    var userId = _userRepo.GetCurrentUserId();
    if (userId == Guid.Empty) userId = Guid.Parse(_config["SeedData.AdminUserId"]!);

    // 4. Soft Delete
    package.SoftDelete(userId);

    // 5. Kaydet
    _packageRepo.Update(package);
    await _unit.SaveChangesAsync(_token);

    return Result<Unit>.Succeed(Unit.Value);


  }
}
