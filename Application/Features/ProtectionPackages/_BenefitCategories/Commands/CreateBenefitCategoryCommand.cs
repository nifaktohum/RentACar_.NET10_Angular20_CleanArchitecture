using Application.Features.ProtectionPackages._BenefitCategories.Dto;
using Domain.Entities.Protection;
using Domain.Repositories;
using Domain.Repositories.Protection;
using FluentValidation;
using GenericRepository;
using MediatR;
using Microsoft.Extensions.Configuration;
using TS.Result;

namespace Application.Features.ProtectionPackages._BenefitCategories.Commands;

public sealed record CreateBenefitCategoryCommand(
    string Name,
    string Slug,
    string? Description,
    string? Icon,
    int DisplayOrder
) : IRequest<Result<BenefitCategoryResponseDto>>;

public sealed class CreateBenefitCategoryCommandValidator : AbstractValidator<BenefitCategoryResponseDto>
{
  public CreateBenefitCategoryCommandValidator()
  {
    RuleFor(x => x.Name)
        .NotEmpty().WithMessage("Kategori adı zorunludur.")
        .MaximumLength(100).WithMessage("Kategori adı 100 karakterden uzun olamaz.");

    RuleFor(x => x.Slug)
        .NotEmpty().WithMessage("Slug zorunludur.")
        .MaximumLength(100).WithMessage("Slug 100 karakterden uzun olamaz.")
        .Matches(@"^[a-z0-9-]+$").WithMessage("Slug sadece küçük harf, rakam ve tire içerebilir.");

    RuleFor(x => x.Description)
        .MaximumLength(500).WithMessage("Açıklama 500 karakterden uzun olamaz.");

    RuleFor(x => x.Icon)
        .MaximumLength(50).WithMessage("İkon 50 karakterden uzun olamaz.");

    RuleFor(x => x.DisplayOrder)
        .GreaterThanOrEqualTo(0).WithMessage("Görüntüleme sırası 0 veya daha büyük olmalıdır.");
  }
}

public sealed class CreateBenefitCategoryCommandHandler(
                             IBenefitCategoryRepository _benefitCategoryRepo,
                             IUnitOfWork _unit,
                             IUserRepository _userRepo,
                             IConfiguration _config
                    ) : IRequestHandler<CreateBenefitCategoryCommand, Result<BenefitCategoryResponseDto>>
{
  public async Task<Result<BenefitCategoryResponseDto>> Handle(CreateBenefitCategoryCommand _req, CancellationToken _token)
  {
    // Aynı isimde kategori var mı?
    var existing = await _benefitCategoryRepo
        .FirstOrDefaultAsync(c => c.Name == _req.Name && !c.IsDeleted, _token);

    if (existing is not null)
      return Result<BenefitCategoryResponseDto>.Failure(400, $"'{_req.Name}' adında bir kategori zaten mevcut.");

    // Aynı slug var mı?
    var existingSlug = await _benefitCategoryRepo
        .FirstOrDefaultAsync(c => c.Slug == _req.Slug && !c.IsDeleted, _token);

    if (existingSlug is not null)
      return Result<BenefitCategoryResponseDto>.Failure(400, $"'{_req.Slug}' slug'ı zaten kullanımda.");

    // Kullanıcı ID
    var userId = _userRepo.GetCurrentUserId();
    if (userId == Guid.Empty) userId = Guid.Parse(_config["SeedData:AdminUserId"]!);

    // Kategori oluştur
    var category = new BenefitCategory(
        _req.Name,
        _req.Slug.ToLowerInvariant(),
        _req.Description,
        _req.Icon,
        _req.DisplayOrder,
        userId
    );

    await _benefitCategoryRepo.AddAsync(category, _token);
    await _unit.SaveChangesAsync(_token);

    // Kullanıcı adını çek
    var userNames = await _userRepo.GetUserNamesByIdsAsync(new List<Guid> { userId }, _token);
    string GetUserName(Guid id) => userNames.GetValueOrDefault(id, "Bilinmiyor");

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
      UpdatedByName = null,
      BenefitCount = 0,
      Benefits = new List<BenefitBriefDto>()
    };

    return Result<BenefitCategoryResponseDto>.Succeed(dto);

  }
}