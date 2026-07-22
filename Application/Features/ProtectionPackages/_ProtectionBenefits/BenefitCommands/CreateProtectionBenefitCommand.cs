using Application.Features.ProtectionPackages.Dto;
using Domain.Repositories;
using Domain.Repositories.Protection;
using FluentValidation;
using GenericRepository;
using MediatR;
using Microsoft.Extensions.Configuration;
using TS.Result;
using Application.Behaviors;
using Domain.Entities.Protection;

namespace Application.Features.ProtectionPackages._ProtectionBenefits.BenefitCommands;

[Permission("ProtectionBenefit.Create")]
public sealed record CreateProtectionBenefitCommand(
                                    string Name,
                                    string? Description,
                                    string? Icon,
                                    int DisplayOrder,
                                    Guid CategoryId
                      ) : IRequest<Result<ProtectionBenefitDto>>;

public sealed class CreateProtectionBenefitValidator : AbstractValidator<CreateProtectionBenefitCommand>
{
  public CreateProtectionBenefitValidator()
  {
    RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Benefit adı zorunludur.")
            .MaximumLength(100).WithMessage("Benefit adı 100 karakterden uzun olamaz.");

    RuleFor(x => x.Description)
        .MaximumLength(500).WithMessage("Açıklama 500 karakterden uzun olamaz.");

    RuleFor(x => x.Icon)
        .MaximumLength(50).WithMessage("İkon adı 50 karakterden uzun olamaz.");

    RuleFor(x => x.DisplayOrder)
        .GreaterThanOrEqualTo(0).WithMessage("Görüntüleme sırası 0 veya daha büyük olmalıdır.");

    RuleFor(x => x.CategoryId)
         .NotEmpty().WithMessage("Kategori ID zorunludur.")
         .Must(id => id != Guid.Empty).WithMessage("Geçerli bir kategori ID'si giriniz.");
  }
}

public sealed class CreateProtectionBenefitCommandHandler(
                            IProtectionBenefitRepository _benefitRepo,
                            IUserRepository _userRepo,
                            IBenefitCategoryRepository _categoryRepo,
                            IUnitOfWork _unit,
                            IConfiguration _config
                      ) : IRequestHandler<CreateProtectionBenefitCommand, Result<ProtectionBenefitDto>>
{
  public async Task<Result<ProtectionBenefitDto>> Handle(CreateProtectionBenefitCommand _req, CancellationToken _token)
  {
    // 1. ✅ Kategori var mı?
    var category = await _categoryRepo
        .FirstOrDefaultAsync(c => c.Id == _req.CategoryId && !c.IsDeleted, _token);

    if (category is null)
      return Result<ProtectionBenefitDto>.Failure(400, "Belirtilen kategori bulunamadı.");

    // 2. Aynı isimde benefit var mı?
    var existing = await _benefitRepo
        .FirstOrDefaultAsync(b => b.Name == _req.Name && !b.IsDeleted, _token);

    if (existing is not null)
      return Result<ProtectionBenefitDto>.Failure(400, $"'{_req.Name}' adında bir benefit zaten mevcut.");

    // 3. Kullanıcı ID
    var userId = _userRepo.GetCurrentUserId();
    if (userId == Guid.Empty)
      userId = Guid.Parse(_config["SeedData:AdminUserId"]!);

    // 3. Benefit oluştur
    var benefit = new  ProtectionBenefit(
        name: _req.Name,
        description: _req.Description,
        icon: _req.Icon,
        displayOrder: _req.DisplayOrder,
        categoryId: _req.CategoryId,
        createdBy: userId
    );



    await _benefitRepo.AddAsync(benefit, _token);
    await _unit.SaveChangesAsync(_token);

    #region  CreatedByName: ""
    // 8. Kullanıcı adlarını çek
    var userIds = new List<Guid> { benefit.CreatedBy };
    if (benefit.UpdatedBy.HasValue) userIds.Add(benefit.UpdatedBy.Value);


    var distinctUserIds = userIds.Distinct().ToList();
    var userNames = await _userRepo.GetUserNamesByIdsAsync(distinctUserIds, _token);
    string GetUserName(Guid id) => userNames.GetValueOrDefault(id, "Bilinmiyor");

    #endregion

    // 4. Response
    var dto = new ProtectionBenefitDto(
        Id: benefit.Id,
        Name: benefit.Name,
        Description: benefit.Description,
        Icon: benefit.Icon,
        DisplayOrder: benefit.DisplayOrder,
        Category: category.Name,
        CategoryId: benefit.CategoryId,
        IsActive: benefit.IsActive,
        CreatedAt: benefit.CreatedAt,
        CreatedBy: benefit.CreatedBy,
        CreatedByName: GetUserName(benefit.CreatedBy),
        UpdatedAt: benefit.UpdatedAt,
        UpdatedBy: benefit.UpdatedBy,
        UpdatedByName: benefit.UpdatedBy.HasValue ? GetUserName(benefit.CreatedBy) : null
    );

    return Result<ProtectionBenefitDto>.Succeed(dto);
  }
}
