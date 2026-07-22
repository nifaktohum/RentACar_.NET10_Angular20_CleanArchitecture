using Application.Features.ProtectionPackages._BenefitCategories.Dto;
using Domain.Repositories;
using Domain.Repositories.Protection;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TS.Result;

namespace Application.Features.ProtectionPackages._BenefitCategories.Queries;

public sealed record GetBenefitCategoryByIdQuery(Guid Id) : IRequest<Result<BenefitCategoryResponseDto>>;

public sealed class GetBenefitCategoryByIdValidator : AbstractValidator<GetBenefitCategoryByIdQuery>
{
  public GetBenefitCategoryByIdValidator()
  {
    RuleFor(x => x.Id)
        .NotEmpty().WithMessage("Kategori ID boş olamaz.");
  }
}

public sealed class GetBenefitCategoryByIdQueryHandler(
                        IBenefitCategoryRepository benefitCategoryRepo,
                        IUserRepository _userRepo
                    ) : IRequestHandler<GetBenefitCategoryByIdQuery, Result<BenefitCategoryResponseDto>>
{
  public async Task<Result<BenefitCategoryResponseDto>> Handle(GetBenefitCategoryByIdQuery _req, CancellationToken _token)
  {
    var category = await benefitCategoryRepo
                            .Where(c => c.Id == _req.Id && !c.IsDeleted)
                            .Include(c => c.Benefits)
                            .FirstOrDefaultAsync(_token);

    if (category is null)
      return Result<BenefitCategoryResponseDto>.Failure(404, "Kategori bulunamadı.");

    var userIds = new List<Guid> { category.CreatedBy };
    if (category.UpdatedBy.HasValue) userIds.Add(category.UpdatedBy.Value);

    var userNames = await _userRepo.GetUserNamesByIdsAsync(userIds, _token);
    string GetUserName(Guid id) => userNames.GetValueOrDefault(id, "Bilinmiyor");

    var benefits = category.Benefits?
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

    var dto = new BenefitCategoryResponseDto
    {
      Id = category.Id,
      Name = category.Name,
      Slug = category.Slug,
      Description = category.Description,
      Icon = category.Icon,
      DisplayOrder = category.DisplayOrder,
      IsActive = category.IsActive,
      CreatedAt = category.CreatedAt,
      CreatedBy = category.CreatedBy,
      CreatedByName = GetUserName(category.CreatedBy),
      UpdatedAt = category.UpdatedAt,
      UpdatedBy = category.UpdatedBy,
      UpdatedByName = category.UpdatedBy.HasValue ? GetUserName(category.UpdatedBy.Value) : null,
      BenefitCount = benefits.Count,
      Benefits = benefits
    };


    return Result<BenefitCategoryResponseDto>.Succeed(dto);
  }
}