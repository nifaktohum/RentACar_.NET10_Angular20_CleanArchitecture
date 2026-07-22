using Application.Features.Categories.Dto;
using Domain.Entities.Categories;
using Domain.Repositories;
using MediatR;
using TS.Result;

namespace Application.Features.Categories.Queries;

public sealed record GetCategoryHierarchyQuery : IRequest<Result<List<GetCategoryHierarchyDto>>>;

public sealed class GetCategoryHierarchyQueryHandler(
                            ICategoryRepository _categoryRepo
                    ): IRequestHandler<GetCategoryHierarchyQuery, Result<List<GetCategoryHierarchyDto>>>
{
  public async Task<Result<List<GetCategoryHierarchyDto>>> Handle(GetCategoryHierarchyQuery _req, CancellationToken _token)
  {
    // Tüm aktif kategorileri çek (Sadece IsDeleted == false olanlar)
    var allCategories = await _categoryRepo.GetCategoryHierarchyAsync(_token);

    // Ana kategorileri filtrele (ParentCategoryId == null olanlar)
    var rootCategories = allCategories
        .Where(c => c.ParentCategoryId == null)
        .Select(c => MapToDto(c, allCategories)) // allCategories'i buraya gönder
        .ToList();

    return Result<List<GetCategoryHierarchyDto>>.Succeed(rootCategories);
  }

  private GetCategoryHierarchyDto MapToDto(Category category, List<Category> allCategories)
  {
    return new GetCategoryHierarchyDto(
        Id: category.Id,
        Name: category.Name,
        Slug: category.Slug,
        Description: category.Description,
        DisplayOrder: category.DisplayOrder,
        IsActive: true,
        ParentCategoryId: category.ParentCategoryId,
        // Alt kategorileri recursive olarak bul
        SubCategories: allCategories
            .Where(c => c.ParentCategoryId == category.Id)
            .Select(sub => new CategoryHierarchySubDto(
                sub.Id, sub.Name, sub.Slug, sub.Description, sub.DisplayOrder,
                sub.ParentCategoryId, category.Name, true
            )).ToList()
    );
  }
}