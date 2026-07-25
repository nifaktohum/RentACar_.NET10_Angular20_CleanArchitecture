using Domain.Repositories;
using Domain.Repositories.Extras;
using FluentValidation;
using GenericRepository;
using MediatR;
using Microsoft.Extensions.Configuration;
using TS.Result;

namespace Application.Features.Extras.Commands;

public sealed record DeleteExtraCommand(Guid Id) : IRequest<Result<Unit>>;

public sealed class DeleteExtraCommandValidator : AbstractValidator<DeleteExtraCommand>
{
  public DeleteExtraCommandValidator()
  {
    RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID boş olamaz.")
            .Must(id => id != Guid.Empty).WithMessage("Geçerli bir ID giriniz.");
  }
}

public sealed class DeleteExtraCommandHandler(
                        IExtraRepository extraRepo,
                        IUnitOfWork unitOfWork,
                        IUserRepository _userRepo,
                        IConfiguration _config
                    ) : IRequestHandler<DeleteExtraCommand, Result<Unit>>
{
  public async Task<Result<Unit>> Handle(DeleteExtraCommand _req, CancellationToken _token)
  {
    // ============================================================
    // ADIM 1: Ürün var mı kontrol et
    // ============================================================
    var extra = await extraRepo.FirstOrDefaultAsync(e => e.Id == _req.Id, _token);
    if (extra is null)
      return Result<Unit>.Failure($"'{_req.Id}' ID'li ekstra hizmet bulunamadı.");

    // ============================================================
    // ADIM 2: Soft delete yap
    // ============================================================
    var userId = _userRepo.GetCurrentUserId();
    if (userId == Guid.Empty) userId = Guid.Parse(_config["SeedData:AdminUserId"]!);

    extra.SoftDelete(userId);

    // ============================================================
    // ADIM 3: Repository'ye güncelle ve kaydet
    // ============================================================
    extraRepo.Update(extra);
    await unitOfWork.SaveChangesAsync(_token);

    // ============================================================
    // ADIM 4: Başarılı sonucu döndür
    // ============================================================
    return Result<Unit>.Succeed(Unit.Value);
  }
}
