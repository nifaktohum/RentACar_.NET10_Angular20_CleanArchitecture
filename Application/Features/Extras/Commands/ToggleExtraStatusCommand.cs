using Domain.Entities.Users;
using Domain.Repositories;
using Domain.Repositories.Extras;
using FluentValidation;
using GenericRepository;
using MediatR;
using Microsoft.Extensions.Configuration;
using TS.Result;

namespace Application.Features.Extras.Commands;

public sealed record ToggleExtraStatusCommand(Guid Id) : IRequest<Result<bool>>;

public sealed class ToggleExtraStatusCommandValidator : AbstractValidator<ToggleExtraStatusCommand>
{
  public ToggleExtraStatusCommandValidator()
  {
    RuleFor(x => x.Id)
    .NotEmpty().WithMessage("ID boş olamaz.")
    .Must(id => id != Guid.Empty).WithMessage("Geçerli bir ID giriniz.");
  }
}

public sealed class ToggleExtraStatusCommandHandler(
                        IExtraRepository _extraRepo,
                        IUserRepository _userRepo,
                        IConfiguration _config,
                        IUnitOfWork _unit
                    ) : IRequestHandler<ToggleExtraStatusCommand, Result<bool>>
{
  public async Task<Result<bool>> Handle(ToggleExtraStatusCommand _req, CancellationToken _token)
  {
    // ============================================================
    // ADIM 1: Ürün var mı kontrol et
    // ============================================================
    var extra = await _extraRepo.FirstOrDefaultAsync(e => e.Id == _req.Id, _token);
    if (extra is null)
      return Result<bool>.Failure($"'{_req.Id}' ID'li ekstra hizmet bulunamadı.");

    // ============================================================
    // ADIM 2: Aktif/Pasif durumunu değiştir (Toggle)
    // ============================================================
    if (extra.IsActive)
      extra.Deactivate();
    else
      extra.Activate();

    // ============================================================
    // ADIM 3: Metadata güncelle
    // ============================================================

    var userId = _userRepo.GetCurrentUserId();
    if (userId == Guid.Empty) userId = Guid.Parse(_config["SeedData:AdminUserId"]!);

    extra.UpdateMetadata(userId);

    _extraRepo.Update(extra);
    await _unit.SaveChangesAsync(_token);


    return Result<bool>.Succeed(extra.IsActive);
  }
}