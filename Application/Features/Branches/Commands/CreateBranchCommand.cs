using Application.Behaviors;
using Domain.Branchs;
using Domain.Repositories;
using FluentValidation;
using GenericRepository;
using MediatR;

namespace Application.Branches;

// IRequest<Guid> diyerek, bu komutun işlenmesi bittiğinde 
// geriye yeni yaratılan şubenin 'Guid' ID'sini döneceğimizi taahhüt ediyoruz.
[Permission("Branches.Create")]
public sealed record CreateBranchCommand(
    string Name,
    Address Address
) : IRequest<Guid>;

public sealed class CreateBranchCommandValidator : AbstractValidator<CreateBranchCommand>
{
  public CreateBranchCommandValidator()
  {
    // --- 1. ŞUBE ADI DENETİMLERİ ---
    RuleFor(x => x.Name)
        .NotEmpty().WithMessage("Şube adı boş geçilemez.")
        .MinimumLength(3).WithMessage("Şube adı en az 3 karakter olmalıdır.")
        .MaximumLength(150).WithMessage("Şube adı en fazla 150 karakter olabilir.");

    // --- 2. ADRES (VALUE OBJECT) İÇ ALAN DENETİMLERİ ---
    RuleFor(x => x.Address.City)
        .NotEmpty().WithMessage("Şehir alanı boş geçilemez.")
        .MaximumLength(50).WithMessage("Şehir adı en fazla 50 karakter olabilir.");

    RuleFor(x => x.Address.District)
            .NotEmpty().WithMessage("İlçe alanı boş geçilemez.")
            .MaximumLength(50).WithMessage("İlçe adı en fazla 50 karakter olabilir.");

    RuleFor(x => x.Address.FullAddress)
        .NotEmpty().WithMessage("Açık adres alanı boş geçilemez.")
        .MinimumLength(10).WithMessage("Açık adres daha açıklayıcı olmalıdır (en az 10 karakter).")
        .MaximumLength(500).WithMessage("Açık adres en fazla 500 karakter olabilir.");

    // --- 3. İLETİŞİM (TELEFON VE E-POSTA) DENETİMLERİ ---
    RuleFor(x => x.Address.Phone1)
        .NotEmpty().WithMessage("Birinci telefon numarası boş geçilemez.")
        .Matches(@"^[0-9+\s-]{10,20}$").WithMessage("Geçersiz telefon numarası formatı.");

    // Phone2 nullable (string?) olduğu için sadece dolu geldiğinde (When) kuralı işletiyoruz
    RuleFor(x => x.Address.Phone2)
        .Matches(@"^[0-9+\s-]{10,20}$").WithMessage("Geçersiz ikinci telefon numarası formatı.")
        .When(x => !string.IsNullOrEmpty(x.Address.Phone2));

    RuleFor(x => x.Address.Email)
        .NotEmpty().WithMessage("E-posta adresi boş geçilemez.")
        .EmailAddress().WithMessage("Lütfen geçerli bir e-posta adresi giriniz.")
        .MaximumLength(100).WithMessage("E-posta adresi en fazla 100 karakter olabilir.");
  }
};

public sealed class CreateBrenchCommandHandler(
                      IBranchRepository _branchRepo,
                      IUnitOfWork _unit
) : IRequestHandler<CreateBranchCommand, Guid>
{
  public async Task<Guid> Handle(CreateBranchCommand _req, CancellationToken _token)
  {

    // 1. İŞ MANTIĞI KONTROLÜ: Şube adı daha önce alınmış mı?
    bool isNameExists = await _branchRepo.AnyAsync(x => x.Name.ToLower() == _req.Name.ToLower(), _token);

    if (isNameExists)
    {
      throw new ArgumentException($"{_req.Name} şube zaten mevcut! Lütfen farklı bir isim deneyiniz.");
    }

    var branch = new Branch(_req.Name, _req.Address, Guid.Empty);

    await _branchRepo.AddAsync(branch, _token);
    await _unit.SaveChangesAsync(_token);

    return branch.Id;
  }
};



/*
    Özetle Süreç Nasıl İşliyor?
        1. new Branch(...) anı: Id otomatik olarak Guid.NewGuid() ile oluşturulur (Örn: a1b2c3...). 
        CreatedBy ise geçici olarak Guid.Empty (00000...) yapılır.

        2. _unit.SaveChangesAsync() anı: AppDbContext araya girer, Angular'dan gelen token'ı okur, 
        şubeyi kimin açtığını bulur (Örn: Kullanıcı_Ahmet_ID) ve o geçici Guid.Empty değerinin üzerine gerçek Ahmet'in ID'sini yazar.

        3. PostgreSQL anı: Veritabanına veri yazıldığında; Id kısmında otomatik üretilen benzersiz Guid, 
        CreatedBy kısmında ise şubeyi gerçekten ekleyen personelin Guid'i tertemiz bir şekilde durur.
*/