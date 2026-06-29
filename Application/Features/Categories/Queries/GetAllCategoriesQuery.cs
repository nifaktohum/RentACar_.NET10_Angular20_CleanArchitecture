using Application.Behaviors;
using Application.Features.Categories.Dto;
using Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TS.Result;

namespace Application.Features.Categories.Queries;

[Permission("Categories.Read")]
public sealed record GetAllCategoriesQuery() : IRequest<Result<List<GetCategoryHierarchyDto>>>;
public sealed class GetAllCategoriesQueryHandler(
                            ICategoryRepository categoryRepo
                    ) : IRequestHandler<GetAllCategoriesQuery, Result<List<GetCategoryHierarchyDto>>>
{
  public async Task<Result<List<GetCategoryHierarchyDto>>> Handle(GetAllCategoriesQuery _req, CancellationToken _token)
  {
    // ✅ Repository metodu kullan
    var categories = await categoryRepo.GetCategoryHierarchyAsync(_token);


    // DTO'ya dönüştür
    var result = categories.Select(c => new GetCategoryHierarchyDto(
        c.Id,
        c.Name,
        c.Slug,
        c.Description,
        c.DisplayOrder,
        c.IsActive,
        c.ParentCategoryId,
        c.SubCategories
            .Where(sc => !sc.IsDeleted)
            .OrderBy(sc => sc.DisplayOrder)
            .Select(sc => new CategoryHierarchySubDto(
                sc.Id,
                sc.Name,
                sc.Slug,
                sc.DisplayOrder,
                sc.ParentCategoryId,
                sc.ParentCategory?.Name,  // ✅ Artık gelir
                sc.IsActive
            )).ToList()
    )).ToList();

    return Result<List<GetCategoryHierarchyDto>>.Succeed(result);
  }
}