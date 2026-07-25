using Application.Features.Extras.Dto;
using Domain.Entities.Extras.Enum;
using Domain.Repositories;
using Domain.Repositories.Extras;
using FluentValidation;
using GenericRepository;
using MediatR;
using Microsoft.Extensions.Configuration;
using TS.Result;

namespace Application.Features.Extras.Commands;

public sealed record UpdateExtraCommand(
    Guid Id,
    string Name,
    string? Description,
    string? Icon,
    decimal Price,
    PriceType PriceType,
    ExtraCategory Category,
    int DisplayOrder,
    bool IsRecommended,
    int? MinAge,
    string? AgeRange,
    int? StockLimit,
    bool IsActive
) : IRequest<Result<ExtraDto>>;

public sealed class UpdateExtraCommandValidator : AbstractValidator<UpdateExtraCommand>
{
  public UpdateExtraCommandValidator()
  {
    RuleFor(x => x.Id)
        .NotEmpty().WithMessage("Paket ID boş olamaz.");
    // ============================================================
    // Name - Zorunlu ve maksimum 100 karakter
    // ============================================================
    RuleFor(x => x.Name)
        .NotEmpty().WithMessage("Ürün adı boş olamaz.")
        .MaximumLength(100).WithMessage("Ürün adı en fazla 100 karakter olabilir.");

    // ============================================================
    // Description - Maksimum 500 karakter (opsiyonel)
    // ============================================================
    RuleFor(x => x.Description)
        .MaximumLength(500).WithMessage("Açıklama en fazla 500 karakter olabilir.");

    // ============================================================
    // Icon - Maksimum 50 karakter (opsiyonel)
    // ============================================================
    RuleFor(x => x.Icon)
        .MaximumLength(50).WithMessage("İkon kodu en fazla 50 karakter olabilir.");

    // ============================================================
    // Price - 0'dan büyük olmalı
    // ============================================================
    RuleFor(x => x.Price)
        .GreaterThan(0).WithMessage("Fiyat 0'dan büyük olmalıdır.");

    // ============================================================
    // PriceType - Geçerli bir enum değeri olmalı
    // ============================================================
    RuleFor(x => x.PriceType)
        .IsInEnum().WithMessage("Geçersiz fiyat tipi. (Daily=1, Rental=2)");

    // ============================================================
    // Category - Geçerli bir enum değeri olmalı
    // ============================================================
    RuleFor(x => x.Category)
        .IsInEnum().WithMessage("Geçersiz kategori. (Guarantee=1, Driver=2, Seat=3, Other=4)");

    // ============================================================
    // DisplayOrder - 0'dan küçük olamaz
    // ============================================================
    RuleFor(x => x.DisplayOrder)
        .GreaterThanOrEqualTo(0).WithMessage("Görüntüleme sırası 0'dan küçük olamaz.");

    // ============================================================
    // MinAge - Varsa 18-99 arası olmalı
    // ============================================================
    RuleFor(x => x.MinAge)
        .GreaterThanOrEqualTo(18).When(x => x.MinAge.HasValue)
        .WithMessage("Minimum yaş 18'den küçük olamaz.")
        .LessThanOrEqualTo(99).When(x => x.MinAge.HasValue)
        .WithMessage("Minimum yaş 99'dan büyük olamaz.");

    // ============================================================
    // AgeRange - Varsa maksimum 50 karakter
    // ============================================================
    RuleFor(x => x.AgeRange)
        .MaximumLength(50).WithMessage("Yaş aralığı en fazla 50 karakter olabilir.");

    // ============================================================
    // StockLimit - Varsa 0'dan büyük olmalı
    // ============================================================
    RuleFor(x => x.StockLimit)
        .GreaterThanOrEqualTo(0).When(x => x.StockLimit.HasValue)
        .WithMessage("Stok limiti 0'dan küçük olamaz.");
  }
}

public sealed class UpdateExtraCommandHandler(
                        IExtraRepository _extraRepo,
                        IUserRepository _userRepo,
                        IConfiguration _config,
                        IUnitOfWork unitOfWork
                    ) : IRequestHandler<UpdateExtraCommand, Result<ExtraDto>>
{
  public async Task<Result<ExtraDto>> Handle(UpdateExtraCommand _req, CancellationToken _token)
  {
    // ADIM 1: Ekstra hizmet var mı kontrol et
    var extra = await _extraRepo.FirstOrDefaultAsync(e => e.Id == _req.Id, _token);
    if (extra is null)
      return Result<ExtraDto>.Failure($"'{_req.Id}' ID'li ekstra hizmet bulunamadı.");

    // ADIM 2: Aynı isimde başka bir hizmet var mı kontrol et (kendisi hariç)
    var exists = await _extraRepo.AnyAsync(e => e.Name == _req.Name && e.Id != _req.Id && !e.IsDeleted, _token);
    if (exists)
      return Result<ExtraDto>.Failure($"'{_req.Name}' adında bir ekstra hizmet zaten mevcut.");

    // ADIM 3: Entity'yi güncelle (Domain Encapsulation)
    extra.UpdateDetails(
        name: _req.Name,
        description: _req.Description,
        icon: _req.Icon,
        price: _req.Price,
        priceType: _req.PriceType,
        category: _req.Category,
        displayOrder: _req.DisplayOrder,
        isRecommended: _req.IsRecommended,
        minAge: _req.MinAge,
        ageRange: _req.AgeRange,
        stockLimit: _req.StockLimit
    );

    // Aktiflik durumunu güncelle
    if (_req.IsActive != extra.IsActive)
    {
      if (_req.IsActive)
        extra.Activate();
      else
        extra.Deactivate();
    }

    // Değişiklikleri kaydet (EF Core Change Tracking kullanıyorsa Update(extra) opsiyoneldir)
    await unitOfWork.SaveChangesAsync(_token);

    var userId = _userRepo.GetCurrentUserId();
    if (userId == Guid.Empty) userId = Guid.Parse(_config["SeedData:AdminUserId"]!);

    var userName = await _userRepo.GetUserNamesByIdsAsync(new List<Guid> { userId }, _token);
    string GetUserName(Guid id) => userName.GetValueOrDefault(id, "Bilinmiyor");

    // ADIM 4: DTO Dönüşü (İsterseniz burada Mapper kullanabilirsiniz)
    var dto = new ExtraDto(
        Id: extra.Id,
        Name: extra.Name,
        Description: extra.Description,
        Icon: extra.Icon,
        Price: extra.Price,
        PriceType: extra.PriceType.ToString(),
        Category: extra.Category.ToString(),
        DisplayOrder: extra.DisplayOrder,
        IsRecommended: extra.IsRecommended,
        MinAge: extra.MinAge,
        AgeRange: extra.AgeRange,
        StockLimit: extra.StockLimit,
        IsActive: extra.IsActive,
        CreatedAt: extra.CreatedAt,
        CreatedBy: extra.CreatedBy,
        CreatedByName: GetUserName(extra.CreatedBy),
        UpdatedAt: extra.UpdatedAt,
        UpdatedBy: extra.UpdatedBy,
        UpdatedByName: extra.UpdatedBy.HasValue ? GetUserName(extra.UpdatedBy.Value) : null
    // HasValue "İçinde veri var mı?" diye kontrol eder, 
    // Value ise "Var olduğunu bildiğim için artık o verinin kendisine ihtiyacım var" deyip içeriği alır.
    );

    return Result<ExtraDto>.Succeed(dto);
  }
}

