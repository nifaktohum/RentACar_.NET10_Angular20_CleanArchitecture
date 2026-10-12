using Application.Features.Auth.Customers.Login.Dto;
using Application.Services;
using Domain.Repositories;
using FluentValidation;
using GenericRepository;
using MediatR;
using TS.Result;

namespace Application.Features.Auth.Customers.Login.Commands;

public sealed record CustomerRefreshTokenCommand(
                        string RefreshToken // Oturum süresi dolduğunda yeni Access Token almak için kullanılan yenileme jetonu (Refresh Token)
                     ) : IRequest<Result<CustomerLoginResponse>>;

public sealed class CustomerRefreshTokenCommandValidator : AbstractValidator<CustomerRefreshTokenCommand>
{
  public CustomerRefreshTokenCommandValidator()
  {
    RuleFor(p => p.RefreshToken)
        .NotEmpty().WithMessage("Refresh token zorunludur.");
  }
}

public sealed class CustomerRefreshTokenCommandHandler(
                      ICustomerRepository _customerRepo,
                      IJwtProvider _jwtProvider,
                      IUnitOfWork _unit
                  ) : IRequestHandler<CustomerRefreshTokenCommand, Result<CustomerLoginResponse>>
{
  public async Task<Result<CustomerLoginResponse>> Handle(CustomerRefreshTokenCommand _req, CancellationToken _token)
  {
    // 1. Refresh token ile müşteriyi bul
    var customer = await _customerRepo.GetByRefreshTokenAsync(_req.RefreshToken, _token);
    if (customer is null)
      return Result<CustomerLoginResponse>.Failure("Geçersiz refresh token!");

    // 2. Süresi dolmuş mu?
    if (customer.RefreshTokenExpiresAt is null || customer.RefreshTokenExpiresAt < DateTimeOffset.UtcNow)
    {
      customer.ClearRefreshToken();
      await _unit.SaveChangesAsync(_token);
      return Result<CustomerLoginResponse>.Failure("Refresh token süresi dolmuş, tekrar giriş yapın!");
    }


    // 4. ⭐ Refresh token rotasyonu (güvenlik için her refresh'te yeni token)
    string newRefreshToken = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");

    DateTimeOffset newExpiresAt = DateTimeOffset.UtcNow.AddDays(30);
    customer.SetRefreshToken(newRefreshToken, newExpiresAt);

    await _unit.SaveChangesAsync(_token);

    string newAccessToken = _jwtProvider.CreateTokenForCustomer(customer);

    // 5. Response
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