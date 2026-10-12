using Application.Features.Auth.Customers.Login.Dto;
using Application.Services;
using Domain.Repositories;
using FluentValidation;
using GenericRepository;
using MediatR;
using TS.Result;

namespace Application.Features.Auth.Customers.Login.Commands;

public sealed record CustomerLoginCommand(
                          string Email,      // Müşterinin sisteme kayıtlı e-posta adresi
                          string Password,   // Müşterinin hesap şifresi
                          bool RememberMe    // Oturumun uzun süreli açık kalıp kalmayacağını belirten bayrak (Beni Hatırla)
                      ) : IRequest<Result<CustomerLoginResponse>>;

public sealed class CustomerLoginCommandValidator : AbstractValidator<CustomerLoginCommand>
{
  public CustomerLoginCommandValidator()
  {
    RuleFor(p => p.Email)
        .NotEmpty().WithMessage("E-posta adresi zorunludur.")
        .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.");

    RuleFor(p => p.Password)
        .NotEmpty().WithMessage("Şifre zorunludur.")
        .MinimumLength(6).WithMessage("Şifre en az 6 karakter olmalıdır.");
  }
}

public sealed class CustomerLoginCommandHandler(
                      ICustomerRepository _customerRepo,
                      IPasswordHasher _passwordHasher,
                      IJwtProvider _jwtProvider,
                      IUnitOfWork _unit
                  ) : IRequestHandler<CustomerLoginCommand, Result<CustomerLoginResponse>>
{
  public async Task<Result<CustomerLoginResponse>> Handle(CustomerLoginCommand _req, CancellationToken _token)
  {

    // 1. Email ile müşteriyi bul
    var customer = await _customerRepo.GetByEmailAsync(_req.Email, _token);
    if (customer is null) return Result<CustomerLoginResponse>.Failure("E-posta adresi veya şifre hatalı!");

    // 3. Şifre doğru mu?
    bool isPasswordVerified = _passwordHasher.VerifyPassword(_req.Password, customer.PasswordHash);
    if (!isPasswordVerified) return Result<CustomerLoginResponse>.Failure("E-posta adresi veya şifre hatalı!");

    // SecurityStamp yoksa üret (ilk login veya eski kayıt)
    if (string.IsNullOrEmpty(customer.SecurityStamp))
    {
      customer.UpdateSecurityStamp();
      // SaveChanges aşağıda toplu yapılacak
    }

    // 4. JWT token üret
    string token = _jwtProvider.CreateTokenForCustomer(customer, _req.RememberMe);

    // 5. Refresh token üret ve DB'ye kaydet
    string refreshToken = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N"); // 64 karakter
    DateTime refreshTokenExpiresAt = _req.RememberMe
                  ? DateTime.UtcNow.AddDays(30)   // Beni hatırla → 30 gün
                  : DateTime.UtcNow.AddDays(7);   // Normal → 7 gün

    customer.SetRefreshToken(refreshToken, refreshTokenExpiresAt);
  
    await _unit.SaveChangesAsync(_token);

    // 5. Response hazırla
    var fullName = customer.Type == Domain.Entities.Customers.CustomerType.Individual
                      ? $"{customer.FirstName} {customer.LastName}".Trim()
                      : customer.CompanyName ?? "Kurumsal Müşteri";

    var response = new CustomerLoginResponse(
     Token: token,
     RefreshToken: refreshToken,
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