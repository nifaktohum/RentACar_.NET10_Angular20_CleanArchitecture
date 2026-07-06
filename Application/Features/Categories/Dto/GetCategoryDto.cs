using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Features.Categories.Dto;

public sealed record GetCategoryHierarchyDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    int? DisplayOrder,
    bool IsActive,
    Guid? ParentCategoryId,
    List<CategoryHierarchySubDto> SubCategories
);

public sealed record CategoryHierarchySubDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    int? DisplayOrder,
    Guid? ParentCategoryId,
    string? ParentCategoryName,
    bool IsActive
);