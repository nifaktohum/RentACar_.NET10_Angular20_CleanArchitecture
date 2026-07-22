using Application.Behaviors;
using Application.Features.ProtectionPackages.Dto;
using Domain.Repositories;
using Domain.Repositories.Protection;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TS.Result;

namespace Application.Features.ProtectionPackages._ProtectionBenefits.Queries;

[Permission("ProtectionBenefit.Read")]
public sealed record GetAllProtectionBenefitsQuery: IRequest<Result<List<ProtectionBenefitDto>>>;

public sealed class GetAllProtectionBenefitsQueryHandler(
                            IProtectionBenefitRepository _benefitRepo,
                            IUserRepository _userRepo

                    ) : IRequestHandler<GetAllProtectionBenefitsQuery, Result<List<ProtectionBenefitDto>>>
{
  public async Task<Result<List<ProtectionBenefitDto>>> Handle(GetAllProtectionBenefitsQuery _req, CancellationToken _token)
  {

    var benefits = await _benefitRepo
              .Where(b => b.IsActive && !b.IsDeleted)
              .Include(b => b.Category)
              .OrderBy(b => b.DisplayOrder)
              .ToListAsync(_token);

    #region  CreatedByName: "";
    // // sistemdeki tüm kullanıcı ID'lerini bir havuzda topluyoruz.
    var userIds = new List<Guid>();

    foreach (var b in benefits)
    {
      // Ana paketin oluşturucusu ve güncelleyicisi
      userIds.Add(b.CreatedBy);
      if (b.UpdatedBy.HasValue)
      {
        userIds.Add(b.UpdatedBy.Value);
   
      }
    }

    // gereksiz sorgu atmamak için Distinct() ile ID'leri tekilleştiriyoruz.
    var distinctUserIds = userIds.Distinct().ToList();
    // tüm kullanıcıların isimlerini (FullName) bir Dictionary olarak çekiyoruz.
    var userNames = await _userRepo.GetUserNamesByIdsAsync(distinctUserIds, _token);

    // Eğer ID'ye ait isim bulunamazsa "Bilinmiyor" döndürerek hata almamızı engelliyoruz.
    string GetUserName(Guid userId) => userNames.GetValueOrDefault(userId, "Bilinmiyor");

    #endregion 



    var dtos = benefits.Select(b => new ProtectionBenefitDto(
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
              )).ToList();


    return Result<List<ProtectionBenefitDto>>.Succeed(dtos);
  }
}