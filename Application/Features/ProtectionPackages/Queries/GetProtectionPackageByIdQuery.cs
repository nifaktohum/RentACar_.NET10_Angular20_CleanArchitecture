using Application.Behaviors;
using Application.Features.ProtectionPackages.Dto;
using Domain.Repositories;
using Domain.Repositories.Protection;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TS.Result;

namespace Application.Features.ProtectionPackages.Queries;

[Permission("ProtectionPackage.Read")]
public sealed record GetProtectionPackageByIdQuery(Guid Id): IRequest<Result<ProtectionPackageDto>>;

public sealed class GetProtectionPackageByIdQueryHandler(
                          IProtectionPackageRepository _packageRepo,
                          IUserRepository _userRepo
                    ) : IRequestHandler<GetProtectionPackageByIdQuery, Result<ProtectionPackageDto>>
{
  public async Task<Result<ProtectionPackageDto>> Handle(GetProtectionPackageByIdQuery _req, CancellationToken _token)
  {
    // 1. Paketi getir (Benefits ve Pricing ile birlikte)
    var package = await _packageRepo.GetPackageWithDetailsAsync(_req.Id, _token);

    // 2. Paket bulunamadıysa
    if (package is null)
      return Result<ProtectionPackageDto>.Failure(404, "Paket bulunamadı.");

    #region  CreatedByName: "";
    // // sistemdeki tüm kullanıcı ID'lerini bir havuzda topluyoruz.
    var userIds = new List<Guid>();


    // Ana paketin oluşturucusu ve güncelleyicisi
    userIds.Add(package.CreatedBy);
      if (package.UpdatedBy.HasValue)
      {
      userIds.Add(package.UpdatedBy.Value);

        // Paket içerisindeki her bir avantajın (Benefit) sorumlusu
        foreach (var b in package.Benefits)
        {
        userIds.Add(b.CreatedBy);
          if (b.UpdatedBy.HasValue) userIds.Add(b.UpdatedBy.Value);
        }
        // Paket içerisindeki her bir fiyatlandırmanın (Pricing) sorumlusu
        foreach (var pr in package.Pricing)
        {
        userIds.Add(pr.CreatedBy);
          if (pr.UpdatedBy.HasValue) userIds.Add(pr.UpdatedBy.Value);
        }
      }    

    // gereksiz sorgu atmamak için Distinct() ile ID'leri tekilleştiriyoruz.
    var distinctUserIds = userIds.Distinct().ToList();
    // tüm kullanıcıların isimlerini (FullName) bir Dictionary olarak çekiyoruz.
    var userNames = await _userRepo.GetUserNamesByIdsAsync(distinctUserIds, _token);
    // Eğer ID'ye ait isim bulunamazsa "Bilinmiyor" döndürerek hata almamızı engelliyoruz.
    string GetUserName(Guid userId) => userNames.GetValueOrDefault(userId, "Bilinmiyor");

    #endregion 




    // 3. DTO'ya dönüştür
    var dto = new ProtectionPackageDto(
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
        Pricing: package.Pricing.Select(pr => new ProtectionPricingDto(
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
        IsActive: package.IsActive,
        CreatedAt: package.CreatedAt,
        CreatedBy: package.CreatedBy,
        CreatedByName: GetUserName(package.CreatedBy),
        UpdatedAt: package.UpdatedAt,
        UpdatedBy: package.UpdatedBy,
        UpdatedByName: package.UpdatedBy.HasValue ? GetUserName(package.UpdatedBy.Value) : null
    );

    return Result<ProtectionPackageDto>.Succeed(dto);
  }
}
