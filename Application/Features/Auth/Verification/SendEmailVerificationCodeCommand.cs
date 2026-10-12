using Application.Services;
using Domain.Repositories;
using GenericRepository;
using MediatR;
using Microsoft.Extensions.Logging;
using TS.Result;

namespace Application.Features.Auth.Verification;

public sealed record SendEmailVerificationCodeCommand() : IRequest<Result<string>>;

public sealed class SendEmailVerificationCodeCommandHandler(
                        ICustomerRepository _customerRepo,
                        ICustomerContext _customerContext,
                        IEmailService _emailService,
                        IUnitOfWork _unit,
                        ILogger<SendEmailVerificationCodeCommandHandler> _logger
                    ) : IRequestHandler<SendEmailVerificationCodeCommand, Result<string>>
{
  public async Task<Result<string>> Handle(SendEmailVerificationCodeCommand req, CancellationToken token)
  {
    // 1. Giriş yapmış müşteriyi doğrula
    var customerId = _customerContext.GetCustomerId();
    if (customerId == Guid.Empty) return Result<string>.Failure("Giriş yapmış bir müşteri bulunamadı!");

    var customer = await _customerRepo.GetByExpressionAsync(c => c.Id == customerId, token);
    if (customer is null) return Result<string>.Failure("Müşteri bulunamadı!");

    if (customer.IsEmailVerified) return Result<string>.Failure("E-posta adresiniz zaten doğrulanmış!");

    // 2. 6 haneli kod üret (15 dakika geçerlilik süresi)
    string code = Random.Shared.Next(100000, 999999).ToString();
    DateTimeOffset expiresAt = DateTimeOffset.UtcNow.AddMinutes(5);

    // 3. Entity durumunu güncelle ve ÖNCE veritabanına kaydet
    customer.SetEmailVerificationCode(code, expiresAt);
    
    _customerRepo.Update(customer);
    await _unit.SaveChangesAsync(token);

    // 4. Veritabanı kaydı başarılı olduktan sonra e-postayı fırlat
    bool isEmailSent = await _emailService.SendVerificationCodeAsync(customer.Email, code, token);

    if (!isEmailSent)
    {
      _logger.LogError($"{customer.Email} adresine doğrulama kodu gönderilemedi.");
      return Result<string>.Failure(500, "E-posta sağlayıcısından kaynaklı bir hata oluştu, kod gönderilemedi.");
    }

    _logger.LogInformation($"{customer.Email} adresine doğrulama kodu başarıyla gönderildi.");

    return Result<string>.Succeed("Doğrulama kodu e-posta adresinize gönderildi.");
  }
}