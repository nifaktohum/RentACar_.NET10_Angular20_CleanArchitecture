using Application.Behaviors;
using Domain.Branchs;
using Domain.Repositories;
using FluentValidation;
using GenericRepository;
using MediatR;

namespace Application.Features.Branches.Commands;

[Permission("Branches.Update")]
public sealed record UpdateBranchCommand(
    Guid Id, // Güncellenecek şubenin anahtarı şart!
    string Name,
    Address Address,
    bool IsActive
) : IRequest<Unit>;

public sealed class UpdateBranchCommandValidator : AbstractValidator<UpdateBranchCommand>
{
  public UpdateBranchCommandValidator()
  {
    // ID alanı mutlaka dolu ve geçerli bir şube ID'si olmalı
    RuleFor(x => x.Id)
        .NotEmpty().WithMessage("Güncellenecek şube bilgisi eksik.");

    RuleFor(x => x.Name)
        .NotEmpty().WithMessage("Şube adı boş geçilemez.")
        .MinimumLength(3).WithMessage("Şube adı en az 3 karakter olmalıdır.")
        .MaximumLength(150).WithMessage("Şube adı en fazla 150 karakter olabilir.");

    RuleFor(x => x.Address.City)
        .NotEmpty().WithMessage("Şehir alanı boş geçilemez.")
        .MaximumLength(50).WithMessage("Şehir adı en fazla 50 karakter olabilir.");

    RuleFor(x => x.Address.District)
        .NotEmpty().WithMessage("İlçe alanı boş geçilemez.")
        .MaximumLength(50).WithMessage("İlçe adı en fazla 50 karakter olabilir.");

    RuleFor(x => x.Address.FullAddress)
        .NotEmpty().WithMessage("Açık adres alanı boş geçilemez.")
        .MinimumLength(10).WithMessage("Açık adres en az 10 karakter होना olmalıdır.")
        .MaximumLength(500).WithMessage("Açık adres en fazla 500 karakter olabilir.");

    RuleFor(x => x.Address.Phone1)
        .NotEmpty().WithMessage("Birinci telefon numarası boş geçilemez.")
        .Matches(@"^[0-9+\s-]{10,20}$").WithMessage("Geçersiz telefon numarası formatı.");

    RuleFor(x => x.Address.Phone2)
        .Matches(@"^[0-9+\s-]{10,20}$").WithMessage("Geçersiz ikinci telefon numarası formatı.")
        .When(x => !string.IsNullOrEmpty(x.Address.Phone2));

    RuleFor(x => x.Address.Email)
        .NotEmpty().WithMessage("E-posta adresi boş geçilemez.")
        .EmailAddress().WithMessage("Lütfen geçerli bir e-posta adresi giriniz.")
        .MaximumLength(100).WithMessage("E-posta adresi en fazla 100 karakter olabilir.");
  }
};

// Sınıf isminde "Branch" standardını koruyoruz ve Primary Constructor kullanıyoruz
public sealed class UpdateBranchCommandHandler(
    IBranchRepository _branchRepo,
    IUnitOfWork _unit
) : IRequestHandler<UpdateBranchCommand, Unit>
{
  public async Task<Unit> Handle(UpdateBranchCommand _req, CancellationToken _token)
  {
    // 1. ADIM: Güncellenecek şubeyi veritabanından buluyoruz (Tracking aktif olmalı)
    var branch = await _branchRepo.FirstOrDefaultAsync(x => x.Id == _req.Id, _token, isTrackingActive: true);

    if (branch is null)
    {
      throw new KeyNotFoundException("Güncellenmek istenen şube sistemde bulunamadı.");
    }

    // 2. ADIM: Benzersizlik kontrolü (Kendi eski adını bu kontrolden muaf tutuyoruz)
    bool isNameExists = await _branchRepo.AnyAsync(x => x.Name.ToLower() == _req.Name.ToLower() && x.Id != _req.Id, _token);

    if (isNameExists)
    {
      throw new ArgumentException($"{_req.Name} ismiyle başka bir şube zaten mevcut! Lütfen farklı bir isim deneyiniz.");
    }


    // 4. ADIM: İŞTE O ŞALTER KONTROLÜ!
    // Angular'dan gelen IsActive durumu ile veritabanındaki mevcut durum farklı mı diye bakıyoruz.
    if (_req.IsActive != branch.IsActive)
    {
      if (_req.IsActive)
      {
        branch.Activate(); // Senin BaseEntity içindeki "IsActive = true" yapan metot!
      }
      else
      {
        branch.Deactivate(); // Senin BaseEntity içindeki "IsActive = false" yapan metot!
      }
    }

    // 5. ADIM: İŞTE SENİN YAZDIĞIN BEHAVIORS (METOTLAR) BURADA ÇALIŞIYOR!
    // Dışarıdan doğrudan atama yapmadan, senin yazdığın güvenli kapılardan geçiyoruz:
    branch.SetName(_req.Name);          // Senin yazdığın metot!
    branch.SetAddress(_req.Address);  // Senin yazdığın metot!

    // 6. ADIM: Senin BaseEntity içine yazdığın zaman damgası metodunu tetikliyoruz.
    // NOT: İleride buraya 'Guid.Empty' yerine token'dan (HttpContext) gelen gerçek kullanıcı ID'sini geçeceksin.
    branch.UpdateMetadata(Guid.Empty);

    // 7. ADIM: Repository üzerinden nesneyi güncelliyoruz
    _branchRepo.Update(branch);

    // 8. ADIM: Mühürleme anı! Değişiklikleri PostgreSQL'e kaydediyoruz.
    await _unit.SaveChangesAsync(_token);

    // İşlem başarıyla bitti, MediatR'ı selamlıyoruz.
    return Unit.Value;
  }
}