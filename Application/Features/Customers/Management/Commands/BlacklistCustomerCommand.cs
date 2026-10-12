using Application.Behaviors;
using Application.Services;
using Domain.Repositories;
using FluentValidation;
using GenericRepository;
using MediatR;
using TS.Result;

namespace Application.Features.Customers.Management.Commands;

[Permission("Customers.Blacklist")]
public sealed record BlacklistCustomerCommand(
                          Guid CustomerId,
                          string Reason
                      ) : IRequest<Result<string>>;

public sealed class BlacklistCustomerCommandValidator : AbstractValidator<BlacklistCustomerCommand>
{
  public BlacklistCustomerCommandValidator()
  {
    RuleFor(p => p.CustomerId)
        .NotEmpty().WithMessage("Müşteri ID zorunludur.");

    RuleFor(p => p.Reason)
        .NotEmpty().WithMessage("Kara liste sebebi zorunludur.")
        .MaximumLength(500).WithMessage("Kara liste sebebi en fazla 500 karakter olabilir.");
  }
}

public sealed class BlacklistCustomerCommandHandler(
                        ICustomerRepository _customerRepo,
                        IUserContext _userContext,
                        IUnitOfWork _unit
                    ) : IRequestHandler<BlacklistCustomerCommand, Result<string>>
{
  public async Task<Result<string>> Handle(BlacklistCustomerCommand _req, CancellationToken _token)
  {

    // 1. Personelin ID'sini al (kim kara listeye aldı?)
    var staffId = _userContext.GetUserId();
    if (staffId == Guid.Empty) return Result<string>.Failure("Yetkili kullanıcı bulunamadı!");

    // 2. Müşteriyi bul
    var customer = await _customerRepo.GetByExpressionAsync(c => c.Id == _req.CustomerId, _token);
    if (customer is null) return Result<string>.Failure("Müşteri bulunamadı!");

    // 3. Zaten kara listede mi?
    if (customer.IsBlacklisted) return Result<string>.Failure("Müşteri zaten kara listede!");

    // 4. Domain metodu ile kara listeye al
    customer.Blacklist(_req.Reason, staffId);

    _customerRepo.Update(customer);
    // 5. Kaydet
    await _unit.SaveChangesAsync(_token);

    return Result<string>.Succeed($"{customer.Email} kara listeye alındı.");
  }
}