using Application.Services;
using Domain.Repositories;
using GenericRepository;
using MediatR;
using TS.Result;

namespace Application.Features.Auth.Customers.Login.Commands;

public sealed record CustomerLogoutCommand() : IRequest<Result<string>>;

public sealed class CustomerLogoutCommandHandler(
                        ICustomerRepository _customerRepo,
                        ICustomerContext _customerContext,
                        IUnitOfWork _unit
                    ) : IRequestHandler<CustomerLogoutCommand, Result<string>>
{
  public async Task<Result<string>> Handle(CustomerLogoutCommand _req, CancellationToken _token)
  {
    // 1. Giriş yapmış müşterinin ID'sini al
    var customerId = _customerContext.GetCustomerId();
    if (customerId == Guid.Empty) return Result<string>.Failure("Giriş yapmış bir müşteri bulunamadı!");

    // 2. Müşteriyi bul
    var customer = await _customerRepo.GetByExpressionAsync(c => c.Id == customerId, _token);
    if (customer is null) return Result<string>.Failure("Müşteri bulunamadı!");

    // 3. Refresh token'ı temizle
    customer.ClearRefreshToken();
    await _unit.SaveChangesAsync(_token);

    return Result<string>.Succeed("Çıkış başarılı!");
  }
}