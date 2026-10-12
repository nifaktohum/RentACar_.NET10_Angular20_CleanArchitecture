using Application.Services;
using Domain.Repositories;
using GenericRepository;
using MediatR;
using TS.Result;

namespace Application.Features.Auth.Customers.Login.Commands;

public sealed record CustomerLogoutAllDevicesCommand() : IRequest<Result<string>>;

public sealed class CustomerLogoutAllDevicesCommandHandler(
                        ICustomerRepository _customerRepo,
                        ICustomerContext _customerContext,
                        IUnitOfWork _unit
                    ) : IRequestHandler<CustomerLogoutAllDevicesCommand, Result<string>>
{
  public async Task<Result<string>> Handle(CustomerLogoutAllDevicesCommand _req, CancellationToken _token)
  {
    // 1. Giriş yapmış müşteriyi bul
    var customerId = _customerContext.GetCustomerId();
    if (customerId == Guid.Empty) return Result<string>.Failure("Giriş yapmış bir müşteri bulunamadı!");

    var customer = await _customerRepo.GetByExpressionAsync(c => c.Id == customerId, _token);
    if (customer is null) return Result<string>.Failure("Müşteri bulunamadı!");

    // 2. Domain metodu ile tüm oturumları sonlandır
    customer.LogoutAllDevices();
    await _unit.SaveChangesAsync(_token);

    return Result<string>.Succeed("Tüm cihazlardan başarıyla çıkış yapıldı.");
  }
}