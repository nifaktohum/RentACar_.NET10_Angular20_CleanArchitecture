using Domain.Repositories;
using Domain.Repositories.Protection;
using FluentValidation;
using GenericRepository;
using MediatR;
using Microsoft.Extensions.Configuration;
using TS.Result;

namespace Application.Features.ProtectionPackages._BenefitCategories.Commands;

public sealed record DeleteBenefitCategoryCommand(Guid Id) : IRequest<Result<Unit>>;

public sealed class DeleteBenefitCategoryCommandValidator : AbstractValidator<DeleteBenefitCategoryCommand>
{
  public DeleteBenefitCategoryCommandValidator()
  {
    RuleFor(x => x.Id)
        .NotEmpty().WithMessage("Kategori ID boş olamaz.");
  }
}

public sealed class DeleteBenefitCategoryCommandHandler(
                        IBenefitCategoryRepository _benefitCategoryRepo,
                        IUnitOfWork _unit,
                        IUserRepository _userRepo,
                        IConfiguration _config
                    ) : IRequestHandler<DeleteBenefitCategoryCommand, Result<Unit>>
{
  public async Task<Result<Unit>> Handle(DeleteBenefitCategoryCommand _req, CancellationToken _token)
  {
    // Kategori var mı?
    var category = await _benefitCategoryRepo
        .FirstOrDefaultAsync(c => c.Id == _req.Id && !c.IsDeleted, _token);

    if (category is null)
      return Result<Unit>.Failure(404, "Kategori bulunamadı.");

    // Bu kategoriye bağlı benefit var mı?
    var hasBenefits = category.Benefits?.Any(b => !b.IsDeleted) ?? false;
    if (hasBenefits)
      return Result<Unit>.Failure(400, "Bu kategoriye bağlı benefit'ler var. Önce benefit'leri silin.");

    // Kullanıcı ID
    var userId = _userRepo.GetCurrentUserId();
    if (userId == Guid.Empty)
      userId = Guid.Parse(_config["SeedData:AdminUserId"]!);

    // Soft Delete
    category.SoftDelete(userId);

    _benefitCategoryRepo.Update(category);
    await _unit.SaveChangesAsync(_token);

    return Result<Unit>.Succeed(Unit.Value);
  }
}