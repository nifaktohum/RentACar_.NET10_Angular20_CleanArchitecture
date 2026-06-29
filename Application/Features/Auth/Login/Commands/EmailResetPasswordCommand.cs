using Application.Services;
using Domain.Repositories;
using FluentValidation;
using GenericRepository;
using MediatR;
using TS.Result;

namespace Application.Features.Login.Commands;

// // 2. Aşama: Kod ve yeni şifreyi alan sıfırlama nesnesi
public sealed record EmailResetPasswordCommand(
    string Email,
    string Code,
    string NewPassword
) : IRequest<Result<string>>;


public sealed class EmailResetPasswordCommandValidation : AbstractValidator<EmailResetPasswordCommand>
{
  public EmailResetPasswordCommandValidation()
  {
    // 1. Email Alanı Kuralları
    RuleFor(p => p.Email)
        .NotEmpty().WithMessage("E-posta adresi boş olamaz.")
        .NotNull().WithMessage("E-posta adresi zorunludur.")
        .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.");

    // 2. Sıfırlama Kodu (Code) Alanı Kuralları
    RuleFor(p => p.Code)
        .NotEmpty().WithMessage("Sifre sıfırlama kodu boş olamaz.")
        .NotNull().WithMessage("Şifre sıfırlama kodu zorunludur.");

    // 3. Yeni Şifre (NewPassword) Alanı Kuralları
    RuleFor(p => p.NewPassword)
        .NotEmpty().WithMessage("Yeni şifre alanı boş olamaz.")
        .NotNull().WithMessage("Yeni şifre alanı zorunludur.")
        .MinimumLength(6).WithMessage("Yeni şifre en az 6 karakter olmalıdır.");
  }
}

// // 2. AŞAMA HANDLER: Kodu doğrulayıp şifreyi hash'leme
public sealed class ResetPasswordCommandHandler(
                      IUserRepository _userRepo,
                      IPasswordHasher _passwordHasher, // 👈 Yeni şifreyi hashlemek için altyapındaki servisi çağırıyoruz
                      IUnitOfWork _uow
) : IRequestHandler<EmailResetPasswordCommand, Result<string>>
{
  public async Task<Result<string>> Handle(EmailResetPasswordCommand _req, CancellationToken _token)
  {
    // 1. Kullanıcıyı e-posta adresinden buluyoruz
    var user = await _userRepo.FirstOrDefaultAsync(u => u.Email == _req.Email, _token);
    if (user is null)
    {
      return Result<string>.Failure(404, "Kullanıcı bulunamadı.");
    }

    // 2. Kod doğruluğunu kontrol ediyoruz kanka
    if (user.PasswordResetCode != _req.Code)
    {
      return Result<string>.Failure(400, "Girdiğiniz sıfırlama kodu hatalı kanka.");
    }

    // 3. Kodun süresi geçmiş mi (30 dk sınırımız vardı ya) ona bakıyoruz
    if (user.ResetCodeExpires < DateTime.UtcNow)
    {
      return Result<string>.Failure(400, "Sıfırlama kodunun süresi dolmuş. Lütfen yeniden kod isteyin.");
    }

    // 🔥 🔥 🔥 YENİ GÜVENLİK KONTROLÜ: Şifre eski şifreyle aynı olamaz!
    // Metodunun adı VerifyPassword, CheckPassword veya Verify olabilir kanka, bendekini senin servise göre ayarla:
    bool isSameWithOldPassword = _passwordHasher.VerifyPassword(_req.NewPassword, user.PasswordHash);

    if (isSameWithOldPassword)
    {
      return Result<string>.Failure(400, "Yeni şifreniz, eski şifrenizle aynı olamaz kanka. Lütfen farklı bir şifre belirleyin.");
    }

    // 4. 🔥 HER ŞEY YOLUNDA! Yeni şifreyi hash'leyip güncelliyoruz
    // Altyapındaki IPasswordHasher servisinin metoduna göre burayı (Generate, HashPassword vb.) düzenleyebilirsin kanka
    string hashedPassword = _passwordHasher.HashPassword(_req.NewPassword);

    user.UpdatePassword(hashedPassword); // Senin User tablandaki şifre alanının adı

    // 5. Kullanılmış kodu ve süreyi güvenlik amacıyla sıfırlıyoruz ki aynı kod tekrar kullanılmasın!
    user.PasswordResetCode = null;
    user.ResetCodeExpires = null;

    // 6. Veritabanına mühürlüyoruz kanka
    _userRepo.Update(user);
    await _uow.SaveChangesAsync(_token);

    return Result<string>.Succeed("Şifreniz başarıyla sıfırlandı! Yeni şifrenizle giriş yapabilirsiniz.");
  }
}