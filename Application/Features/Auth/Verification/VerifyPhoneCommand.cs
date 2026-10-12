using Application.Features.Auth.Customers.Login.Dto;
using Application.Services;
using Domain.Repositories;
using FluentValidation;
using GenericRepository;
using MediatR;
using TS.Result;

namespace Application.Features.Auth.Verification;

public sealed record VerifyPhoneCommand(string Code) : IRequest<Result<CustomerLoginResponse>>;

public sealed class VerifyPhoneCommandValidator : AbstractValidator<VerifyPhoneCommand>
{
  public VerifyPhoneCommandValidator()
  {
    RuleFor(p => p.Code)
        .NotEmpty().WithMessage("Doğrulama kodu zorunludur.")
        .Length(6).WithMessage("Doğrulama kodu 6 haneli olmalıdır.")
        .Matches(@"^[0-9]+$").WithMessage("Doğrulama kodu sadece rakamlardan oluşmalıdır.");
  }
}

public sealed class VerifyPhoneCommandHandler(
                        ICustomerRepository _customerRepo,
                        ICustomerContext _customerContext,
                        IUnitOfWork _unit,
                        IJwtProvider _jwtProvider
                    ) : IRequestHandler<VerifyPhoneCommand, Result<CustomerLoginResponse>>
{
  public async Task<Result<CustomerLoginResponse>> Handle(VerifyPhoneCommand _req, CancellationToken _token)
  {
    var customerId = _customerContext.GetCustomerId();
    if (customerId == Guid.Empty) return Result<CustomerLoginResponse>.Failure("Giriş yapmış bir müşteri bulunamadı!");

    var customer = await _customerRepo.GetByExpressionAsync(c => c.Id == customerId, _token);
    if (customer is null) return Result<CustomerLoginResponse>.Failure("Müşteri bulunamadı!");

    if (customer.IsPhoneVerified) return Result<CustomerLoginResponse>.Failure("Telefon numaranız zaten doğrulanmış!");

    if (string.IsNullOrEmpty(customer.PhoneVerificationCode))  // ⚠️ Entity'ye eklenecek
      return Result<CustomerLoginResponse>.Failure("Önce doğrulama kodu talep edin!");

    if (customer.PhoneVerificationCode != _req.Code)  // ⚠️ Entity'ye eklenecek
      return Result<CustomerLoginResponse>.Failure("Doğrulama kodu hatalı!");

    if (customer.PhoneVerificationCodeExpiresAt is null ||
     customer.PhoneVerificationCodeExpiresAt < DateTimeOffset.UtcNow)
      return Result<CustomerLoginResponse>.Failure("Doğrulama kodunun süresi dolmuş. Yeni kod talep edin!");

    customer.VerifyPhone(Guid.Empty);

    // ⭐ YENİ: Yeni token üret (isEmailVerified=true claim'i ile)
    var newAccessToken = _jwtProvider.CreateTokenForCustomer(customer);

    // 4. ⭐ YENİ REFRESH TOKEN ÜRET (rotation)
    string newRefreshToken = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
    DateTimeOffset newExpiresAt = DateTimeOffset.UtcNow.AddDays(30);
    customer.SetRefreshToken(newRefreshToken, newExpiresAt);


    _customerRepo.Update(customer);
    await _unit.SaveChangesAsync(_token);

    // 5. ⭐ CustomerLoginResponse DÖN (Angular'ın beklediği format)
    var fullName = customer.Type == Domain.Entities.Customers.CustomerType.Individual
        ? $"{customer.FirstName} {customer.LastName}".Trim()
        : customer.CompanyName ?? "Kurumsal Müşteri";

    var response = new CustomerLoginResponse(
    Token: newAccessToken,
    RefreshToken: newRefreshToken,
    CustomerId: customer.Id,
    FullName: fullName,
    Email: customer.Email,
    CustomerType: customer.Type.ToString(),
    IsEmailVerified: customer.IsEmailVerified,
    IsPhoneVerified: customer.IsPhoneVerified,
    IsBlacklisted: customer.IsBlacklisted,
    BlacklistReason: customer.BlacklistReason
);

    return Result<CustomerLoginResponse>.Succeed(response);
  }
}