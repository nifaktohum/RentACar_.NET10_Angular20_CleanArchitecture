using Application.Behaviors;
using Domain.Repositories;
using Domain.Repositories.Protection;
using GenericRepository;
using MediatR;
using Microsoft.Extensions.Configuration;
using TS.Result;

namespace Application.Features.ProtectionPackages._ProtectionBenefits.BenefitCommands;

[Permission("ProtectionBenefit.Delete")]
public sealed record DeleteProtectionBenefitCommand(Guid Id) : IRequest<Result<Unit>>;

public sealed class DeleteProtectionBenefitCommandHandler(
                          IProtectionBenefitRepository _benefitRepo,
                          IUserRepository _userRepo,
                          IUnitOfWork _unit,
                          IConfiguration _config
                    ) : IRequestHandler<DeleteProtectionBenefitCommand, Result<Unit>>
{
  public async Task<Result<Unit>> Handle(DeleteProtectionBenefitCommand _req, CancellationToken _token)
  {
    // 1. Benefit var mı?
    var benefit = await _benefitRepo
        .FirstOrDefaultAsync(b => b.Id == _req.Id && !b.IsDeleted, _token);

    if (benefit is null)
      return Result<Unit>.Failure(404, "Benefit bulunamadı.");

    // 2. Kullanıcı ID
    var userId = _userRepo.GetCurrentUserId();
    if (userId == Guid.Empty)
      userId = Guid.Parse(_config["SeedData:AdminUserId"]!);

    // 3. Soft Delete
    benefit.SoftDelete(userId);
    _benefitRepo.Update(benefit);
    await _unit.SaveChangesAsync(_token);

    return Result<Unit>.Succeed(Unit.Value);
  }
}
