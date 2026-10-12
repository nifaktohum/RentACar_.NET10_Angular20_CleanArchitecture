using Application.Behaviors;
using Application.Services;
using Domain.Repositories;
using FluentValidation;
using GenericRepository;
using MediatR;
using TS.Result;

namespace Application.Features.Customers.Management.Commands;

[Permission("Customers.Update")]
public sealed record UpdateFindeksScoreCommand(
                          Guid CustomerId,
                          int Score
                      ) : IRequest<Result<string>>;

public sealed class UpdateFindeksScoreCommandValidator : AbstractValidator<UpdateFindeksScoreCommand>
{
  public UpdateFindeksScoreCommandValidator()
  {
    RuleFor(p => p.CustomerId)
        .NotEmpty().WithMessage("Müşteri ID zorunludur.");

    RuleFor(p => p.Score)
        .InclusiveBetween(0, 1900).WithMessage("Findeks skoru 0-1900 arasında olmalıdır.");
  }
}

public sealed class UpdateFindeksScoreCommandHandler(
                        ICustomerRepository _customerRepo,
                        IUserContext _userContext,
                        IUnitOfWork _unit
                    ) : IRequestHandler<UpdateFindeksScoreCommand, Result<string>>
{
  public async Task<Result<string>> Handle(UpdateFindeksScoreCommand _req, CancellationToken _token)
  {
    var staffId = _userContext.GetUserId();
    if (staffId == Guid.Empty) return Result<string>.Failure("Yetkili kullanıcı bulunamadı!");

    var customer = await _customerRepo.GetByExpressionAsync(c => c.Id == _req.CustomerId, _token);
    if (customer is null) return Result<string>.Failure("Müşteri bulunamadı!");

    if (customer.IsBlacklisted) return Result<string>.Failure("Bu müşteri kara listede!");
    if (!customer.IsActive) return Result<string>.Failure("Bu müşteri Pasif'e çekilmis!");


    // Domain metodu validasyonu zaten yapıyor (0-1900) — FluentValidation çift kontrol
    customer.UpdateFindeksScore(_req.Score, staffId);
    _customerRepo.Update(customer);

    await _unit.SaveChangesAsync(_token);

    return Result<string>.Succeed($"Findeks skoru {_req.Score} olarak güncellendi.");
  }
}