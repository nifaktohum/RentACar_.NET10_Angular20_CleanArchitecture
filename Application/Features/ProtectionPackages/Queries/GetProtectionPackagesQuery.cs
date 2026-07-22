using Application.Behaviors;
using Application.Features.ProtectionPackages.Dto;
using Domain.Repositories;
using Domain.Repositories.Protection;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TS.Result;

namespace Application.Features.ProtectionPackages.Queries;

[Permission("ProtectionPackage.Read")]
public sealed record GetProtectionPackagesQuery(
                          bool? OnlyActive = null,
                          bool? OnlyRecommended = null
                      ) : IRequest<Result<List<ProtectionPackageDto>>>;

public sealed class GetProtectionPackagesQueryHandler(
                            IProtectionPackageRepository _packageRepo,
                            IUserRepository _userRepo
                    ) : IRequestHandler<GetProtectionPackagesQuery, Result<List<ProtectionPackageDto>>>
{
  public async Task<Result<List<ProtectionPackageDto>>> Handle(GetProtectionPackagesQuery _req, CancellationToken _token)
  {
    // ✅ ignoreFilters kullanma veya false yap!
    var query = _packageRepo.GetAll(ignoreFilters: false);  // false ile silinmiş paketler gelmez, true ile gelir.

    if (_req.OnlyActive == true)
      query = query.Where(p => p.IsActive);

    if (_req.OnlyRecommended == true)
      query = query.Where(p => p.IsRecommended);

    var packages = await query
        .OrderBy(p => p.DisplayOrder)
        .Include(p => p.Benefits)
        .Include(p => p.Pricing)
        .ToListAsync(_token);

    #region  CreatedByName: "";
    // // sistemdeki tüm kullanıcı ID'lerini bir havuzda topluyoruz.
    var userIds = new List<Guid>();

    foreach (var p in packages)
    {
      // Ana paketin oluşturucusu ve güncelleyicisi
      userIds.Add(p.CreatedBy);
      if (p.UpdatedBy.HasValue)
      {
        userIds.Add(p.UpdatedBy.Value);

        // Paket içerisindeki her bir avantajın (Benefit) sorumlusu
        foreach (var b in p.Benefits)
        {
          userIds.Add(b.CreatedBy);
          if (b.UpdatedBy.HasValue) userIds.Add(b.UpdatedBy.Value);
        }
        // Paket içerisindeki her bir fiyatlandırmanın (Pricing) sorumlusu
        foreach (var pr in p.Pricing)
        {
          userIds.Add(pr.CreatedBy);
          if (pr.UpdatedBy.HasValue) userIds.Add(pr.UpdatedBy.Value);
        }
      }
    }

    // gereksiz sorgu atmamak için Distinct() ile ID'leri tekilleştiriyoruz.
    var distinctUserIds = userIds.Distinct().ToList();
    // tüm kullanıcıların isimlerini (FullName) bir Dictionary olarak çekiyoruz.
    var userNames = await _userRepo.GetUserNamesByIdsAsync(distinctUserIds, _token);

    // Eğer ID'ye ait isim bulunamazsa "Bilinmiyor" döndürerek hata almamızı engelliyoruz.
    string GetUserName(Guid userId) => userNames.GetValueOrDefault(userId, "Bilinmiyor");

    #endregion 


    // 5. DTO'ya dönüştür
    var dtos = packages.Select(p => new ProtectionPackageDto(
        Id: p.Id,
        Name: p.Name,
        Description: p.Description,
        Icon: p.Icon,
        DisplayOrder: p.DisplayOrder,
        IsRecommended: p.IsRecommended,
        StarRating: p.StarRating,
        ProtectionLevel: p.ProtectionLevel,
        DeductibleType: p.DeductibleType,
        Benefits: p.Benefits.Select(b => new ProtectionBenefitDto(
            Id: b.Id,
            Name: b.Name,
            Description: b.Description,
            Icon: b.Icon,
            DisplayOrder: b.DisplayOrder,
            Category: b.Category?.Name ?? "Bilinmiyor",
            CategoryId: b.CategoryId,
            IsActive: b.IsActive,
            CreatedAt: b.CreatedAt,
            CreatedBy: b.CreatedBy,
            CreatedByName: GetUserName(b.CreatedBy),
            UpdatedAt: b.UpdatedAt,
            UpdatedBy: b.UpdatedBy,
            UpdatedByName: b.UpdatedBy.HasValue ? GetUserName(b.UpdatedBy.Value) : null
        )).ToList().AsReadOnly(),
        Pricing: p.Pricing.Select(pr => new ProtectionPricingDto(
            Id: pr.Id,
            ProtectionPackageId: pr.ProtectionPackageId,
            DailyPrice: pr.DailyPrice,
            DeductibleAmount: pr.DeductibleAmount,
            IsDefault: pr.IsDefault,
            ValidityStart: pr.ValidityStart,
            ValidityEnd: pr.ValidityEnd,
            IsCurrentlyValid: pr.IsCurrentlyValid(),
            IsActive: pr.IsActive,
            CreatedAt: pr.CreatedAt,
            CreatedBy: pr.CreatedBy,
            CreatedByName: GetUserName(pr.CreatedBy),
            UpdatedAt: pr.UpdatedAt,
            UpdatedBy: pr.UpdatedBy,
            UpdatedByName: pr.UpdatedBy.HasValue ? GetUserName(pr.UpdatedBy.Value) : null
        )).ToList(),
        IsActive: p.IsActive,
        CreatedAt: p.CreatedAt,
        CreatedBy: p.CreatedBy,
        CreatedByName: GetUserName(p.CreatedBy),
        UpdatedAt: p.UpdatedAt,
        UpdatedBy: p.UpdatedBy,
        UpdatedByName: p.UpdatedBy.HasValue ? GetUserName(p.UpdatedBy.Value) : null
    )).ToList();

    return Result<List<ProtectionPackageDto>>.Succeed(dtos);
  }
}