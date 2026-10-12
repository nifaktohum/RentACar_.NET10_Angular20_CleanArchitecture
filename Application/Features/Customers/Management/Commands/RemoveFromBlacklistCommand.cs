using Application.Behaviors;
using Application.Services;
using Domain.Repositories;
using GenericRepository;
using MediatR;
using TS.Result;

namespace Application.Features.Customers.Management.Commands;

[Permission("Customers.Blacklist")]
public sealed record RemoveFromBlacklistCommand(Guid CustomerId) : IRequest<Result<string>>;

public sealed class RemoveFromBlacklistCommandHandler(
                        ICustomerRepository _customerRepo,
                        IUserContext _userContext,
                        IUnitOfWork _unit
                    ) : IRequestHandler<RemoveFromBlacklistCommand, Result<string>>
{
  public async Task<Result<string>> Handle(RemoveFromBlacklistCommand _req, CancellationToken _token)
  {
    var staffId = _userContext.GetUserId();
    if (staffId == Guid.Empty) return Result<string>.Failure("Yetkili kullanıcı bulunamadı!");

    var customer = await _customerRepo.GetByExpressionAsync(c => c.Id == _req.CustomerId, _token);
    if (customer is null) return Result<string>.Failure("Müşteri bulunamadı!");

    if (!customer.IsBlacklisted) return Result<string>.Failure("Müşteri zaten kara listede değil!");

    customer.RemoveFromBlacklist(staffId);
    _customerRepo.Update(customer);
    
    await _unit.SaveChangesAsync(_token);

    return Result<string>.Succeed($"{customer.Email} kara listeden çıkarıldı.");
  }
}
