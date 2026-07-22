using Application.Features.ProtectionPackages._BenefitCategories.Dto;
using Domain.Repositories;
using Domain.Repositories.Protection;
using FluentValidation;
using GenericRepository;
using MediatR;
using Microsoft.Extensions.Configuration;
using TS.Result;

namespace Application.Features.ProtectionPackages._BenefitCategories.Commands;

public sealed record UpdateBenefitCategoryCommand(
                          Guid Id,
                          string Name,
                          string Slug,
                          string? Description,
                          string? Icon,
                          int DisplayOrder
                      ) : IRequest<Result<BenefitCategoryResponseDto>>;

public sealed class UpdateBenefitCategoryCommandValidator: AbstractValidator<UpdateBenefitCategoryCommand>
{
  public UpdateBenefitCategoryCommandValidator()
  {
    RuleFor(x => x.Id)
        .NotEmpty().WithMessage("Kategori ID zorunludur.");

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

public sealed class UpdateBenefitCategoryCommandHandler(
                                  IBenefitCategoryRepository _benefitCategoryRepo,
                                  IUnitOfWork _unit,
                                  IUserRepository _userRepo,
                                  IConfiguration _config
                    ) : IRequestHandler<UpdateBenefitCategoryCommand, Result<BenefitCategoryResponseDto>>
{
  public async Task<Result<BenefitCategoryResponseDto>> Handle(UpdateBenefitCategoryCommand _req, CancellationToken _token)
  {
    // Kategori var mı?
    var category = await _benefitCategoryRepo
        .FirstOrDefaultAsync(c => c.Id == _req.Id && !c.IsDeleted, _token);

    if (category is null) 
        return Result<BenefitCategoryResponseDto>.Failure(404, "Kategori bulunamadı.");

    // Aynı isimde başka kategori var mı?
    var existing = await _benefitCategoryRepo
        .FirstOrDefaultAsync(c => c.Name == _req.Name && c.Id != _req.Id && !c.IsDeleted, _token);

    if (existing is not null) 
        return Result<BenefitCategoryResponseDto>.Failure(400, $"'{_req.Name}' adında bir kategori zaten mevcut.");

    // Aynı slug başka kategoride var mı?
    var existingSlug = await _benefitCategoryRepo
        .FirstOrDefaultAsync(c => c.Slug == _req.Slug && c.Id != _req.Id && !c.IsDeleted, _token);

    if (existingSlug is not null)
      return Result<BenefitCategoryResponseDto>.Failure(400, $"'{_req.Slug}' slug'ı zaten kullanımda.");

    // Kullanıcı ID
    var userId = _userRepo.GetCurrentUserId();
    if (userId == Guid.Empty) userId = Guid.Parse(_config["SeedData:AdminUserId"]!);

    // Kategoriyi güncelle
    category.UpdateDetails(
        _req.Name,
        _req.Slug.ToLowerInvariant(),
        _req.Description,
        _req.Icon,
        _req.DisplayOrder
    );

    category.UpdateMetadata(userId);

    _benefitCategoryRepo.Update(category);
    await _unit.SaveChangesAsync(_token);

    // Kullanıcı adlarını çek
    var userIds = new List<Guid> { category.CreatedBy };
    if (category.UpdatedBy.HasValue) userIds.Add(category.UpdatedBy.Value);

    var userNames = await _userRepo.GetUserNamesByIdsAsync(userIds, _token);
    string GetUserName(Guid id) => userNames.GetValueOrDefault(id, "Bilinmiyor");

    // Kategoriye bağlı benefit'leri getir
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
      BenefitCount = benefits.Count, // ← Bu kategoriye 3 benefit bağlı!
      Benefits = benefits            // ← Hangi benefit'ler?
    };

    /*
        "benefitCount": 3,     
        "benefits": [            
                      { "id": "1", "name": "Lastik Cam Far" },
                      { "id": "2", "name": "Lastik Değişim" },
                      { "id": "3", "name": "Lastik Tamiri" }
                    ]
    */


    return Result<BenefitCategoryResponseDto>.Succeed(dto);
  }
}