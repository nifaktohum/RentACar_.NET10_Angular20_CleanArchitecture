using Application.Services;
using Domain.Repositories;
using FluentValidation;
using GenericRepository;
using MediatR;
using TS.Result;

namespace Application.Features.Auth.PasswordReset.Commands;

public sealed record SendEmailPasswordResetCodeCommand(string Email) : IRequest<Result<string>>;

public sealed class SendEmailPasswordResetCodeCommandValidator : AbstractValidator<SendEmailPasswordResetCodeCommand>
{
  public SendEmailPasswordResetCodeCommandValidator()
  {
    RuleFor(p => p.Email)
        .NotEmpty().WithMessage("E-posta adresi zorunludur.")
        .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.");
  }
}

public sealed class SendEmailPasswordResetCodeCommandHandler(
                        ICustomerRepository _customerRepo,
                        IEmailService _emailService,
                        IUnitOfWork _unit
                    ) : IRequestHandler<SendEmailPasswordResetCodeCommand, Result<string>>
{
  public async Task<Result<string>> Handle(SendEmailPasswordResetCodeCommand _req, CancellationToken _token)
  {
    // 1. Email ile müşteriyi bul
    var customer = await _customerRepo.GetByEmailAsync(_req.Email, _token);

    // ⭐ GÜVENLİK: Email yoksa bile "gönderildi" mesajı dön (email enumeration engeli)
    if (customer is null)
      return Result<string>.Succeed("Eğer bu e-posta sistemde kayıtlıysa, sıfırlama kodu gönderildi.");

    // 2. Kara listedeyse şifre sıfırlama engellenebilir (opsiyonel)
    // İstersen burada blacklist kontrolü ekleyebilirsin, ama sıfırlamaya izin vermek daha iyi
    // çünkü kara listeye alınan müşteri belki itiraz edecek.

    // 3. 6 haneli kod üret
    string code = Random.Shared.Next(100000, 999999).ToString();
    DateTimeOffset expiresAt = DateTimeOffset.UtcNow.AddMinutes(5);

    // 4. Entity'ye kaydet
    customer.SetPasswordResetCode(code, expiresAt);
    await _unit.SaveChangesAsync(_token);

    // 5. Email gönder (mevcut SendVerificationCodeAsync'i kullanabiliriz)
    await _emailService.SendVerificationCodeAsync(customer.Email, code, _token);

    return Result<string>.Succeed("Şifre sıfırlama kodu e-posta adresinize gönderildi.");
  }

}