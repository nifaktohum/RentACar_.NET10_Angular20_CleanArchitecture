using Application.Services;
using Domain.Repositories;
using GenericRepository;
using MediatR;
using TS.Result;

namespace Application.Features.Auth.Verification;

public sealed record SendSmsVerificationCodeCommand() : IRequest<Result<string>>;

public sealed class SendSmsVerificationCodeCommandHandler(
                        ICustomerRepository _customerRepo,
                        ICustomerContext _customerContext,
                        ISmsService _smsService,
                        IUnitOfWork _unit
                    ) : IRequestHandler<SendSmsVerificationCodeCommand, Result<string>>
{
  public async Task<Result<string>> Handle(SendSmsVerificationCodeCommand _req, CancellationToken _token)
  {
    // 1. Giriş yapmış müşteriyi bul
    var customerId = _customerContext.GetCustomerId();
    if (customerId == Guid.Empty) return Result<string>.Failure("Giriş yapmış bir müşteri bulunamadı!");

    var customer = await _customerRepo.GetByExpressionAsync(c => c.Id == customerId, _token);
    if (customer is null) return Result<string>.Failure("Müşteri bulunamadı!");

    if (customer.IsPhoneVerified) return Result<string>.Failure("Telefon numaranız zaten doğrulanmış!");

    // 2. 6 haneli kod üret
    string code = Random.Shared.Next(100000, 999999).ToString();
    DateTimeOffset expiresAt = DateTimeOffset.UtcNow.AddMinutes(5);

    // 3. Entity'ye kaydet
    customer.SetPhoneVerificationCode(code, expiresAt);
    _customerRepo.Update(customer);
    
    await _unit.SaveChangesAsync(_token);

    // 4. SMS gönder (mesajı kendimiz oluşturuyoruz)
    string message = $"RentACar dogrulama kodunuz: {code}\nKod 5 dakika gecerlidir.";
    bool isSent = await _smsService.SendSmsAsync(customer.PhoneNumber, message, _token);

    if (!isSent) return Result<string>.Failure("SMS gönderilemedi, lütfen tekrar deneyin!");

    return Result<string>.Succeed("Doğrulama kodu telefon numaranıza gönderildi.");

    throw new NotImplementedException();
  }
}