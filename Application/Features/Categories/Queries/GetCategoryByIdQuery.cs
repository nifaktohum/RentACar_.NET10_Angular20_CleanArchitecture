using Application.Behaviors;
using Application.Features.Categories.Dto;
using Domain.Repositories;
using MediatR;
using TS.Result;

namespace Application.Features.Categories.Queries;

[Permission("Categories.Read")]
public sealed record GetCategoryByIdQuery(
    Guid Id
) : IRequest<Result<GetCategoryHierarchyDto>>;

public sealed class GetCategoryByIdQueryHandler(
                        ICategoryRepository _categoryRepo
                    ) : IRequestHandler<GetCategoryByIdQuery, Result<GetCategoryHierarchyDto>>
{
  public async Task<Result<GetCategoryHierarchyDto>> Handle(GetCategoryByIdQuery _req, CancellationToken _token)
  {
    // 1. Ana kategoriyi getir
    var category = await _categoryRepo.GetByExpressionAsync(c => c.Id == _req.Id && !c.IsDeleted, _token);
    if (category is null) return Result<GetCategoryHierarchyDto>.Failure(404, "Kategori bulunamadı.");

    // 2. Alt kategorileri getir (Repository metodu ile)
    var subCategories = await _categoryRepo.GetSubCategoriesAsync(_req.Id, _token);

    // 3. DTO'ya dönüştür
    var result = new GetCategoryHierarchyDto(
        Id: category.Id,
        Name: category.Name,
        Slug: category.Slug,
        Description: category.Description,
        DisplayOrder: category.DisplayOrder,
        IsActive: category.IsActive,
        ParentCategoryId: category.ParentCategoryId,
        SubCategories: subCategories
            .Select(sc => new CategoryHierarchySubDto(
                sc.Id,
                sc.Name,
                sc.Slug,
                sc.Description,
                sc.DisplayOrder,
                sc.ParentCategoryId,
                sc.ParentCategory?.Name,
                sc.IsActive
            )).ToList()
    );

    return Result<GetCategoryHierarchyDto>.Succeed(result);
  }
}
