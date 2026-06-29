using Application.Behaviors;
using Domain.Repositories;
using FluentValidation;
using GenericRepository;
using MediatR;

namespace Application.Features.Branches.Commands;

// Silinecek şubenin ID'sini zarfın içine koyuyoruz.
[Permission("Branches.Delete")]
public sealed record DeleteBranchCommand(Guid Id) : IRequest<Unit>;

public sealed class DeleteBranchCommandValidator : AbstractValidator<DeleteBranchCommand>
{
  public DeleteBranchCommandValidator()
  {
    RuleFor(x => x.Id)
        .NotEmpty().WithMessage("Silinecek şube bilgisi eksik.");
  }
};


public sealed class DeleteBranchCommandHandler(
                            IBranchRepository _branchRepo,
                            IUserRepository _userRepo,
                            IUnitOfWork _unit
                        ) : IRequestHandler<DeleteBranchCommand, Unit>
{
  public async Task<Unit> Handle(DeleteBranchCommand _req, CancellationToken _token)
  {
    // 1. ADIM: Silinecek şube veritabanında var mı ve zaten silinmiş mi kontrol ediyoruz.
    // Eğer daha önce silinmişse (IsDeleted == true) onu getirmemesi için FirstOrDefaultAsync içinde filtreleyebiliriz.
    var branch = await _branchRepo.FirstOrDefaultAsync(x => x.Id == _req.Id && !x.IsDeleted, _token, isTrackingActive: true);
    var userId = _userRepo.GetCurrentUserId();

    if (branch is null)
    {
      throw new KeyNotFoundException("Silinmek istenen şube sistemde bulunamadı veya daha önce silinmiş.");
    }

    // 2. ADIM: SENİN BÜYÜK SİHİRBAZLIK METODUN ÇALIŞIYOR!
    // BaseEntity içerisine yazdığın:
    // IsDeleted = true, DeletedAt = UtcNow, IsActive = false kuralları TEK SEFERDE mühürleniyor.
    // NOT: İleride 'Guid.Empty' yerine token'dan gelen gerçek kullanıcı ID'sini geçeceksin.
    branch.SoftDelete(userId);

    // 3. ADIM: EF Core Tracking mekanizması nesneyi takip ettiği için durumun değiştiğini bilir.
    // Temiz kod ve niyet belirleme açısından repository üzerindeki Update'i tetikliyoruz.
    _branchRepo.Update(branch);

    // 4. ADIM: Mühürleme anı! Veritabanına değişiklikleri yansıtıyoruz.
    await _unit.SaveChangesAsync(_token);

    // İş bitti, veri dönmüyoruz, MediatR'ı selamlıyoruz.
    return Unit.Value;
  }
}
