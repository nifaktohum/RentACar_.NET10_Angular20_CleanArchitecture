using Application.Services;
using Domain.Repositories;
using FluentValidation;
using GenericRepository;
using MediatR;
using TS.Result;

namespace Application.Features.Auth.PasswordReset.Commands;

public sealed record ResetPasswordByEmailCommand(
                                string Email,
                                string Code,
                                string NewPassword
                                ) : IRequest<Result<string>>;

public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordByEmailCommand>
{
  public ResetPasswordCommandValidator()
  {
    RuleFor(p => p.Email)
        .NotEmpty().WithMessage("E-posta adresi zorunludur.")
        .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.");

    RuleFor(p => p.Code)
        .NotEmpty().WithMessage("Doğrulama kodu zorunludur.")
        .Length(6).WithMessage("Doğrulama kodu 6 haneli olmalıdır.")
        .Matches(@"^[0-9]+$").WithMessage("Doğrulama kodu sadece rakamlardan oluşmalıdır.");

    RuleFor(p => p.NewPassword)
        .NotEmpty().WithMessage("Yeni şifre zorunludur.")
        .MinimumLength(6).WithMessage("Şifre en az 6 karakter olmalıdır.")
        .Matches(@"[A-Z]").WithMessage("Şifre en az bir büyük harf içermelidir.")
        .Matches(@"[a-z]").WithMessage("Şifre en az bir küçük harf içermelidir.")
        .Matches(@"[0-9]").WithMessage("Şifre en az bir rakam içermelidir.");
  }
}

public sealed class ResetPasswordCommandHandler(
                        ICustomerRepository _customerRepo,
                        IPasswordHasher _passwordHasher,
                        IUnitOfWork _unit
                    ) : IRequestHandler<ResetPasswordByEmailCommand, Result<string>>
{
  public async Task<Result<string>> Handle(ResetPasswordByEmailCommand _req, CancellationToken _token)
  {
    // 1. Email ile müşteriyi bul
    var customer = await _customerRepo.GetByEmailAsync(_req.Email, _token);
    if (customer is null)
      return Result<string>.Failure("Geçersiz veya süresi dolmuş kod!");  // ⭐ Güvenlik

    // 2. Kod var mı?
    if (string.IsNullOrEmpty(customer.PasswordResetCode))
      return Result<string>.Failure("Önce şifre sıfırlama kodu talep edin!");

    // 3. Kod doğru mu?
    if (customer.PasswordResetCode != _req.Code)
      return Result<string>.Failure("Doğrulama kodu hatalı!");

    // 4. Süre geçerli mi?
    if (customer.PasswordResetCodeExpiresAt is null ||
        customer.PasswordResetCodeExpiresAt < DateTimeOffset.UtcNow)
      return Result<string>.Failure("Doğrulama kodunun süresi dolmuş. Yeni kod talep edin!");

    // 3. ⭐ Eski şifre kontrolü
    // bool isSameWithOldPassword = _passwordHasher.VerifyPassword(_req.NewPassword, customer.PasswordHash);
    // if (isSameWithOldPassword) return Result<string>.Failure("Yeni şifreniz, eski şifrenizle aynı olamaz!");

    // 5. Yeni şifreyi hash'le
    string newPasswordHash = _passwordHasher.HashPassword(_req.NewPassword);

    // 6. Şifreyi güncelle (updatedBy = Guid.Empty çünkü self-service)
    customer.UpdatePassword(newPasswordHash, Guid.Empty);

    // 7. Kodu temizle
    customer.ClearPasswordResetCode();

    // 8. ⭐ GÜVENLİK: Tüm oturumları sonlandır (refresh token'ı temizle)
    customer.ClearRefreshToken();
    customer.UpdateSecurityStamp();

    await _unit.SaveChangesAsync(_token);

    return Result<string>.Succeed("Şifreniz başarıyla güncellendi. Tekrar giriş yapabilirsiniz.");
  }
}
