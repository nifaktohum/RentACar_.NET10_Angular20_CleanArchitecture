using Application.Services;
using Domain.Repositories;
using FluentValidation;
using GenericRepository;
using MediatR;
using TS.Result;

namespace Application.Features.Login.Commands;

public sealed record EmailResetCodeCommand(string Email) : IRequest<Result<string>>;



public sealed class EmailResetCodeCommandValidation : AbstractValidator<EmailResetCodeCommand>
{
  public EmailResetCodeCommandValidation()
  {
    // Email alanı için geçerlilik kurallarını mühürlüyoruz
    RuleFor(p => p.Email)
        // Alanın tamamen boş veya sadece boşluklardan oluşmasını engeller
        .NotEmpty().WithMessage("E-posta adresi boş olamaz.")

        // Alanın null gelmesini engeller
        .NotNull().WithMessage("E-posta adresi zorunludur.")

        // Gelen metnin dünya standartlarında geçerli bir e-posta formatı olup olmadığını kontrol eder
        .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.");
  }
}

public sealed class ForgotPasswordCommandHandler(
                      IUserRepository _userRepo,
                      IEmailService _emailService,
                      IUnitOfWork _uow // 👈 Değişiklikleri DB'ye kaydetmek için UnitOfWork ekledik kanka
) : IRequestHandler<EmailResetCodeCommand, Result<string>>
{
  public async Task<Result<string>> Handle(EmailResetCodeCommand _req, CancellationToken _token)
  {
    // 1. Kullanıcıyı buluyoruz
    var user = await _userRepo.FirstOrDefaultAsync(u => u.Email == _req.Email, _token);

    if (user is null)
    {
      // 🧪 TEST AMAÇLI: Olmayan kullanıcıda hata dönüp Angular tarafını test ediyoruz
      return Result<string>.Failure(404, "Bu e-posta adresi ile kayıtlı bir kullanıcı bulunamadı kanka.");

      // 🛡️ CANLI (PRODUCTION) İÇİN:
      // return Result<string>.Succeed("E-posta adresiniz sistemde mevcutsa sıfırlama kodu gönderilecektir.");
    }

    // 2. 6 haneli rastgele bir doğrulama kodu üretiyoruz (100000 - 999999 arası tam 6 hane yapar)
    string resetCode = new Random().Next(100000, 999999).ToString();

    // 3. Kullanıcı nesnesine kodları ve süre sınırını (30 dk) işliyoruz
    user.PasswordResetCode = resetCode;
    user.ResetCodeExpires = DateTime.UtcNow.AddMinutes(30);

    // 4. Veritabanında güncelliyoruz ve UnitOfWork ile değişiklikleri kaydediyoruz
    _userRepo.Update(user);
    await _uow.SaveChangesAsync(_token); // 👈 Burası önemli, yoksa kod veritabanına yazılmaz!
    // 🔥 Kodu tam ortadan ikiye bölüp araya tire fırlatıyoruz kanka (Örn: 123456 -> 123-456)
    string formattedCode = $"{resetCode[..3]}-{resetCode[3..]}";
    // 5. 🔥 Smtp4dev'e maili şak diye fırlatıyoruz!
    await _emailService.SendEmailAsync(
        to: user.Email,
        subject: "Rent A Car - Şifre Sıfırlama Kodu",
        body: $"<h3>Şifre Sıfırlama Talebi</h3>" +
              $"<p>Şifre sıfırlama kodun: <strong>{formattedCode}</strong></p>" +
              $"<p>Bu kod <strong>30 dakika</strong> geçerlidir. Eğer bu talebi siz yapmadıysanız lütfen dikkate almayınız.</p>"
    );

    // 6. Sonucu pürüzsüzce dönüyoruz kanka
    return Result<string>.Succeed("Sıfırlama kodu e-posta adresinize başarıyla gönderildi kanka.");
  }
}
