using Application.Services;
using Domain.Repositories;
using FluentValidation;
using GenericRepository;
using MediatR;
using TS.Result;

namespace Application.Features.Auth.PasswordReset.Commands;

public sealed record SendSmsPasswordResetCodeCommand(string PhoneNumber) : IRequest<Result<string>>;

public sealed class SendSmsPasswordResetCodeCommandValidator : AbstractValidator<SendSmsPasswordResetCodeCommand>
{
  public SendSmsPasswordResetCodeCommandValidator()
  {
    RuleFor(p => p.PhoneNumber)
        .NotEmpty().WithMessage("Telefon numarası zorunludur.")
        .Matches(@"^\+?[1-9]\d{1,14}$")
            .WithMessage("Geçerli bir uluslararası telefon numarası giriniz. (Örn: +90555...)");
  }
}

public sealed class SendSmsPasswordResetCodeCommandHandler(
                        ICustomerRepository _customerRepo,
                        ISmsService _smsService,
                        IUnitOfWork _unit
                    ) : IRequestHandler<SendSmsPasswordResetCodeCommand, Result<string>>
{
  public async Task<Result<string>> Handle(SendSmsPasswordResetCodeCommand _req, CancellationToken _token)
  {
    // 1. Telefon numarası ile müşteriyi bul
    var customer = await _customerRepo.GetByPhoneNumberAsync(_req.PhoneNumber, _token);

    // ⭐ Güvenlik: Yoksa da başarı mesajı dön
    if (customer is null) return Result<string>.Succeed("Telefon sistemde kayıtlı, sıfırlama kodu gönderildi.");

    // 2. 6 haneli kod üret
    string code = Random.Shared.Next(100000, 999999).ToString();
    DateTimeOffset expiresAt = DateTimeOffset.UtcNow.AddMinutes(5);

    // 3. Yeni SmsResetCode alanına kaydet (PhoneVerificationCode'u bozmuyoruz!)
    customer.SetSmsResetCode(code, expiresAt);
    _customerRepo.Update(customer);
    
    await _unit.SaveChangesAsync(_token);

    // 4. SMS gönder
    string formattedCode = $"{code[..3]}-{code[3..]}";
    string message = $"RentACar sifre sifirlama kodunuz: {formattedCode}. Bu kod 5 dakika gecerlidir.";
    bool isSent = await _smsService.SendSmsAsync(customer.PhoneNumber, message, _token);

    if (!isSent) return Result<string>.Failure("SMS gönderilemedi, lütfen tekrar deneyin!");

    return Result<string>.Succeed("Şifre sıfırlama kodu telefon numaranıza gönderildi.");
  }
}