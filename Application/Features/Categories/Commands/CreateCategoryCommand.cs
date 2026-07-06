using Application.Behaviors;
using Application.Features.Categories.Dto;
using Domain.Categories;
using Domain.Repositories;
using GenericRepository;
using MediatR;
using TS.Result;

namespace Application.Features.Categories.Commands;

[Permission("Categories.Create")]
public sealed record CreateCategoryCommand(
                            string Name,
                            string Slug,
                            string? Description,
                            int? DisplayOrder,
                            Guid? ParentCategoryId
                      ) : IRequest<Result<CategoryDto>>;

public sealed class CreateCategoryCommandHandler(
                                  ICategoryRepository _categoryRepo,
                                  IUserRepository _userRepo,
                                  IUnitOfWork _unit
                                  ) : IRequestHandler<CreateCategoryCommand, Result<CategoryDto>>
{
  public async Task<Result<CategoryDto>> Handle(CreateCategoryCommand _req, CancellationToken _token)
  {
    // ✅ Giriş yapan kullanıcının ID'sini al
    var createdBy = _userRepo.GetCurrentUserId();
    // Slug kontrolü
    var existingCategory = await _categoryRepo.GetBySlugAsync(_req.Slug, _token);
    if (existingCategory is not null)
    {
      return Result<CategoryDto>.Failure(400, "Bu slug zaten kullanımda.");
    }

    // Parent kategori kontrolü
    if (_req.ParentCategoryId.HasValue)
    {
      var parentExists = await _categoryRepo.AnyAsync(c => c.Id == _req.ParentCategoryId.Value, _token);
      if (!parentExists)
      {
        return Result<CategoryDto>.Failure(404, "Belirtilen üst kategori bulunamadı.");
      }
    }
 
    var category = new Category(
        name: _req.Name,
        slug: _req.Slug.ToLowerInvariant(),
        createdBy: createdBy,
        description: _req.Description,
        displayOrder: _req.DisplayOrder,
        parentCategoryId: _req.ParentCategoryId
    );

    await _categoryRepo.AddAsync(category, _token);
    await _unit.SaveChangesAsync(_token);

    var response = new CategoryDto(
     Id: category.Id,
     Name: category.Name,
     Slug: category.Slug,
     Description: category.Description,
     DisplayOrder: category.DisplayOrder,
     ParentCategoryId: category.ParentCategoryId,
     ParentCategoryName: category.ParentCategory?.Name,
     IsActive: category.IsActive
    );

    return Result<CategoryDto>.Succeed(response);
  }
}