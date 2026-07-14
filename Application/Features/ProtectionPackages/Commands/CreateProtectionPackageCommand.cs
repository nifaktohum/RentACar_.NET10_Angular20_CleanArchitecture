using Application.Features.ProtectionPackages.Dto;
using Domain.Entities.Protection;
using Domain.Protection;
using Domain.Repositories;
using Domain.Repositories.Protection;
using FluentValidation;
using GenericRepository;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TS.Result;

namespace Application.Features.ProtectionPackages.Commands;

public sealed record CreateProtectionPackageCommand(
    string Name,
    string? Description,
    string? Icon,
    int DisplayOrder,
    bool IsRecommended,
    int StarRating,
    ProtectionLevel ProtectionLevel,
    DeductibleType DeductibleType,
    List<Guid> BenefitIds,
    CreateProtectionPricingRequestDto Pricing,
    bool IsActive = true  
) : IRequest<Result<ProtectionPackageDto>>;

public sealed class CreateProtectionPackageCommandValidator : AbstractValidator<CreateProtectionPackageCommand>
{
  public CreateProtectionPackageCommandValidator()
  {
    RuleFor(x => x.Name)
        .NotEmpty().WithMessage("Paket adı zorunludur.")
        .MaximumLength(100).WithMessage("Paket adı 100 karakterden uzun olamaz.");

    RuleFor(x => x.Description)
        .MaximumLength(500).WithMessage("Açıklama 500 karakterden uzun olamaz.");

    RuleFor(x => x.Icon)
        .MaximumLength(50).WithMessage("İkon adı 50 karakterden uzun olamaz.");

    RuleFor(x => x.DisplayOrder)
        .GreaterThanOrEqualTo(0).WithMessage("Görüntüleme sırası 0 veya daha büyük olmalıdır.");

    RuleFor(x => x.StarRating)
        .InclusiveBetween(1, 5).WithMessage("Yıldız derecesi 1 ile 5 arasında olmalıdır.");

    RuleFor(x => x.ProtectionLevel)
        .IsInEnum().WithMessage("Geçerli bir koruma seviyesi seçiniz.");

    RuleFor(x => x.DeductibleType)
        .IsInEnum().WithMessage("Geçerli bir muafiyet tipi seçiniz.");

    RuleFor(x => x.BenefitIds)
        .NotNull().WithMessage("Benefit listesi boş olamaz.")
        .Must(ids => ids.Any()).WithMessage("En az bir benefit seçilmelidir.");

    RuleFor(x => x.Pricing)
        .NotNull().WithMessage("Fiyat bilgisi zorunludur.");

    // ✅ IsActive validasyonu eklendi
    RuleFor(x => x.IsActive)
        .NotNull().WithMessage("Aktiflik durumu belirtilmelidir.");

    When(x => x.Pricing is not null, () =>
    {
      RuleFor(x => x.Pricing.DailyPrice)
              .GreaterThanOrEqualTo(0).WithMessage("Günlük fiyat 0 veya daha büyük olmalıdır.")
              .When(x => x.Pricing.DailyPrice.HasValue);

      RuleFor(x => x.Pricing.DeductibleAmount)
              .GreaterThanOrEqualTo(0).WithMessage("Muafiyet tutarı 0 veya daha büyük olmalıdır.")
              .When(x => x.Pricing.DeductibleAmount.HasValue);
    });
  }
}

public sealed class CreateProtectionPackageCommandHandler(
    IProtectionPackageRepository _packageRepository,
    IProtectionBenefitRepository _benefitRepository,
    IUnitOfWork _unitOfWork,
    IUserRepository _userRepository,
    IConfiguration _config
) : IRequestHandler<CreateProtectionPackageCommand, Result<ProtectionPackageDto>>
{
  public async Task<Result<ProtectionPackageDto>> Handle(CreateProtectionPackageCommand _req, CancellationToken _token)
  {
    // 1. Benefit'leri getir
    var benefitIds = _req.BenefitIds.Distinct().ToList();
    var benefits = await _benefitRepository
        .Where(b => benefitIds.Contains(b.Id) && b.IsActive && !b.IsDeleted)
        .ToListAsync(_token);

    // 2. Tüm benefit'ler bulundu mu kontrol et
    if (benefits.Count != benefitIds.Count)
    {
      var existingIds = benefits.Select(b => b.Id).ToList();
      var missingIds = benefitIds.Except(existingIds).ToList();

      return Result<ProtectionPackageDto>.Failure(
          400, $"Aşağıdaki benefit ID'leri bulunamadı: {string.Join(", ", missingIds)}"
      );
    }

    // 3. Aynı isimde paket var mı?
    var existingPackage = await _packageRepository
        .FirstOrDefaultAsync(p => p.Name == _req.Name && !p.IsDeleted, _token);

    if (existingPackage is not null)
    {
      return Result<ProtectionPackageDto>.Failure(
          400, $"'{_req.Name}' adında bir paket zaten mevcut."
      );
    }

    // 4. Kullanıcı ID'si
    var userId = _userRepository.GetCurrentUserId();
    if (userId == Guid.Empty)
      userId = Guid.Parse(_config["SeedData.AdminUserId"]!);

    // 5. Paketi oluştur
    var package = new ProtectionPackage(
        name: _req.Name,
        description: _req.Description,
        icon: _req.Icon,
        displayOrder: _req.DisplayOrder,
        isRecommended: _req.IsRecommended,
        starRating: _req.StarRating,
        protectionLevel: _req.ProtectionLevel,
        deductibleType: _req.DeductibleType,
        createdBy: userId,
        isActive: _req.IsActive
    );

    // 6. Benefit'leri pakete ekle
    foreach (var benefit in benefits)
    {
      _packageRepository.Attach(benefit);
      package.AddBenefit(benefit);
    }

    // 7. Fiyatlandırmayı oluştur ve ekle
    var validityStart = _req.Pricing.ValidityStart ?? DateTimeOffset.UtcNow;

    var pricing = new ProtectionPricing(
        protectionPackageId: package.Id,
        dailyPrice: _req.Pricing.DailyPrice,
        deductibleAmount: _req.Pricing.DeductibleAmount,
        isDefault: _req.Pricing.IsDefault,
        validityStart: validityStart,
        validityEnd: _req.Pricing.ValidityEnd,
        createdBy: userId
    );

    package.AddPricing(pricing);

    // 8. Veritabanına kaydet
    await _packageRepository.AddAsync(package, _token);
    await _unitOfWork.SaveChangesAsync(_token);

    #region  CreatedByName: ""
    // 8. Kullanıcı adlarını çek
    var userIds = new List<Guid> { package.CreatedBy };
    if (package.UpdatedBy.HasValue) userIds.Add(package.UpdatedBy.Value);

    foreach (var b in package.Benefits)
    {
      userIds.Add(b.CreatedBy);
      if (b.UpdatedBy.HasValue) userIds.Add(b.UpdatedBy.Value);
    }

    foreach (var p in package.Pricing)
    {
      userIds.Add(p.CreatedBy);
      if (p.UpdatedBy.HasValue) userIds.Add(p.UpdatedBy.Value);
    }

    var distinctUserIds = userIds.Distinct().ToList();
    var userNames = await _userRepository.GetUserNamesByIdsAsync(distinctUserIds, _token);
    string GetUserName(Guid id) => userNames.GetValueOrDefault(id, "Bilinmiyor");

    #endregion

    // 9. Response oluştur
    var response = new ProtectionPackageDto(
        Id: package.Id,
        Name: package.Name,
        Description: package.Description,
        Icon: package.Icon,
        DisplayOrder: package.DisplayOrder,
        IsRecommended: package.IsRecommended,
        StarRating: package.StarRating,
        ProtectionLevel: package.ProtectionLevel,
        DeductibleType: package.DeductibleType,
        Benefits: package.Benefits.Select(b => new ProtectionBenefitDto(
            Id: b.Id,
            Name: b.Name,
            Description: b.Description,
            Icon: b.Icon,
            DisplayOrder: b.DisplayOrder,
            Category: b.Category.ToString(),
            IsActive: b.IsActive,
            CreatedAt: b.CreatedAt,
            CreatedBy: b.CreatedBy,
            CreatedByName: GetUserName(b.CreatedBy), // Servis katmanından doldurulacak
            UpdatedAt: b.UpdatedAt,
            UpdatedBy: b.UpdatedBy,
            UpdatedByName: b.UpdatedBy.HasValue ? GetUserName(b.UpdatedBy.Value) : null
        )).ToList().AsReadOnly(),
        Pricing: package.Pricing.Select(p => new ProtectionPricingDto(
            Id: p.Id,
            ProtectionPackageId: p.ProtectionPackageId,
            DailyPrice: p.DailyPrice,
            DeductibleAmount: p.DeductibleAmount,
            IsDefault: p.IsDefault,
            ValidityStart: p.ValidityStart,
            ValidityEnd: p.ValidityEnd,
            IsCurrentlyValid: p.IsCurrentlyValid(),
            IsActive: p.IsActive,
            CreatedAt: p.CreatedAt,
            CreatedBy: p.CreatedBy,
            CreatedByName: GetUserName(p.CreatedBy), // Servis katmanından doldurulacak
            UpdatedAt: p.UpdatedAt,
            UpdatedBy: p.UpdatedBy,
            UpdatedByName: p.UpdatedBy.HasValue ? GetUserName(p.UpdatedBy.Value) : null
        )).ToList(),
        IsActive: package.IsActive,
        CreatedAt: package.CreatedAt,
        CreatedBy: package.CreatedBy,
        CreatedByName: GetUserName(package.CreatedBy), // Servis katmanından doldurulacak
        UpdatedAt: package.UpdatedAt,
        UpdatedBy: package.UpdatedBy,
        UpdatedByName: package.UpdatedBy.HasValue ? GetUserName(package.UpdatedBy.Value) : null
    );

    return Result<ProtectionPackageDto>.Succeed(response);
  }
}