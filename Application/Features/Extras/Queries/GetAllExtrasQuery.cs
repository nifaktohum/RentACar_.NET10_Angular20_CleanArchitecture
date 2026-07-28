using Application.Features.Extras.Dto;
using Domain.Repositories;
using Domain.Repositories.Extras;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TS.Result;

namespace Application.Features.Extras.Queries;

public sealed record GetAllExtrasQuery : IRequest<Result<List<ExtraSummaryDto>>>;

public sealed class GetAllExtrasQueryHandler(
                        IExtraRepository _extraRepo,
                        IUserRepository _userRepo
                    ) : IRequestHandler<GetAllExtrasQuery, Result<List<ExtraSummaryDto>>>
{
  public async Task<Result<List<ExtraSummaryDto>>> Handle(GetAllExtrasQuery _req, CancellationToken _token)
  {
    // ============================================================
    // ADIM 1: Tüm aktif ürünleri getir
    // ============================================================
    var extras = await _extraRepo.Where(b => !b.IsDeleted).ToListAsync(_token);

    var userIds = new List<Guid>();

    foreach (var b in extras)
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


    var dtoList = extras.Select(e => new ExtraSummaryDto(
        Id: e.Id,
        Name: e.Name,
        Price: e.Price,
        Icon: e.Icon,
        PriceType: e.PriceType.ToString(),
        Category: e.Category.ToString(),
        DisplayOrder: e.DisplayOrder,
        StockLimit: e.StockLimit,
        IsRecommended: e.IsRecommended,
        IsActive: e.IsActive,
        CreatedAt: e.CreatedAt,
        CreatedByName: GetUserName(e.CreatedBy) // TODO: User service'ten çekilecek
    )).ToList();

    // ============================================================
    // ADIM 3: Sonucu döndür
    // ============================================================
    return Result<List<ExtraSummaryDto>>.Succeed(dtoList);
  }
}