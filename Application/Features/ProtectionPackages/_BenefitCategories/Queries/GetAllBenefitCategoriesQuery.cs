using Application.Features.ProtectionPackages._BenefitCategories.Dto;
using Domain.Repositories;
using Domain.Repositories.Protection;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TS.Result;

namespace Application.Features.ProtectionPackages._BenefitCategories.Queries;

public sealed record GetAllBenefitCategoriesQuery : IRequest<Result<List<BenefitCategoryResponseDto>>>;

public sealed class GetAllBenefitCategoriesQueryHandler(
                        IBenefitCategoryRepository benefitCategoryRepo,
                        IUserRepository _userRepo
                    ) : IRequestHandler<GetAllBenefitCategoriesQuery, Result<List<BenefitCategoryResponseDto>>>
{
  public async Task<Result<List<BenefitCategoryResponseDto>>> Handle(GetAllBenefitCategoriesQuery _req, CancellationToken _token)
  {
    var categories = await benefitCategoryRepo
                              .Where(c => !c.IsDeleted)
                              .Include(c => c.Benefits)
                              .OrderBy(c => c.DisplayOrder)
                              .ToListAsync(_token);

    var userIds = categories
            .SelectMany(c => new[] { c.CreatedBy }
            .Concat(c.UpdatedBy.HasValue ? new[] { c.UpdatedBy.Value } : Array.Empty<Guid>()))
            .Distinct()
            .ToList();

    var userNames = await _userRepo.GetUserNamesByIdsAsync(userIds, _token);
    string GetUserName(Guid id) => userNames.GetValueOrDefault(id, "Bilinmiyor");

    var dtos = categories.Select(c =>
           {
             var benefits = c.Benefits?
              .Where(b => !b.IsDeleted)
              .OrderBy(b => b.DisplayOrder)
              .Select(b => new BenefitBriefDto
              {
                Id = b.Id,
                Name = b.Name,
                Icon = b.Icon,
                DisplayOrder = b.DisplayOrder,
                IsActive = b.IsActive
              })
              .ToList() ?? new List<BenefitBriefDto>();

             return new BenefitCategoryResponseDto
             {
               Id = c.Id,
               Name = c.Name,
               Slug = c.Slug,
               Description = c.Description,
               Icon = c.Icon,
               DisplayOrder = c.DisplayOrder,
               IsActive = c.IsActive,
               CreatedAt = c.CreatedAt,
               CreatedBy = c.CreatedBy,
               CreatedByName = GetUserName(c.CreatedBy),
               UpdatedAt = c.UpdatedAt,
               UpdatedBy = c.UpdatedBy,
               UpdatedByName = c.UpdatedBy.HasValue ? GetUserName(c.UpdatedBy.Value) : null,
               BenefitCount = benefits.Count,
               Benefits = benefits
             };
           }).ToList();

    return Result<List<BenefitCategoryResponseDto>>.Succeed(dtos);
  }
}