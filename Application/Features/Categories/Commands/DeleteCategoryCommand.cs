using Application.Behaviors;
using Domain.Repositories;
using GenericRepository;
using MediatR;
using TS.Result;

namespace Application.Features.Categories.Commands;

[Permission("Categories.Delete")]
public sealed record DeleteCategoryCommand(
    Guid Id
) : IRequest<Result<Unit>>;

public sealed class DeleteCategoryCommandHandler(
                            ICategoryRepository _categoryRepo,
                            IUserRepository _userRepo,
                            IUnitOfWork _unit
                    ) : IRequestHandler<DeleteCategoryCommand, Result<Unit>>
{
  public async Task<Result<Unit>> Handle(DeleteCategoryCommand _req, CancellationToken _token)
  {
    // 1. Kategori var mı?
    var category = await _categoryRepo.GetByExpressionWithTrackingAsync(c => c.Id == _req.Id, _token);
    if (category is null) return Result<Unit>.Failure(404, "Kategori bulunamadı.");

    // 2. Alt kategorisi var mı?
    var hasSubCategories = await _categoryRepo.HasSubCategoriesAsync(_req.Id, _token);
    if (hasSubCategories) return Result<Unit>.Failure(400, "Bu kategorinin alt kategorileri var. Önce alt kategorileri silin.");

    // 3. Soft Delete
    var deletedBy = _userRepo.GetCurrentUserId();
    if (deletedBy == Guid.Empty) deletedBy = Guid.Parse("00000000-0000-0000-0000-000000000001");

    category.SoftDelete(deletedBy);

    await _unit.SaveChangesAsync(_token);

    return Result<Unit>.Succeed(Unit.Value);

  }
}
