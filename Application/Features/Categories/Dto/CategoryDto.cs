using Microsoft.Extensions.Primitives;

namespace Application.Features.Categories.Dto;

public sealed record CategoryDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    int? DisplayOrder,
    Guid? ParentCategoryId,
    string? ParentCategoryName
);
