using Application.Features.Extras.Dto;
using Domain.Entities.Extras.Enum;
using Domain.Repositories;
using Domain.Repositories.Extras;
using FluentValidation;
using MediatR;
using TS.Result;

namespace Application.Features.Extras.Queries;

public sealed record GetExtrasByCategoryQuery(
                          int Category
                      ) : IRequest<Result<List<ExtraSummaryDto>>>;

public sealed class GetExtrasByCategoryQueryValidator : AbstractValidator<GetExtrasByCategoryQuery>
{
  public GetExtrasByCategoryQueryValidator()
  {
    // Category - Geçerli bir enum değeri olmalı
    RuleFor(x => x.Category)
        .IsInEnum().WithMessage("Geçersiz kategori. (Guarantee=1, Driver=2, Seat=3, Other=4)");
  }
}

public sealed class GetExtrasByCategoryQueryHandler(
                        IExtraRepository _extraRepo,
                        IUserRepository _userRepo
                    ) : IRequestHandler<GetExtrasByCategoryQuery, Result<List<ExtraSummaryDto>>>
{
  public async Task<Result<List<ExtraSummaryDto>>> Handle(GetExtrasByCategoryQuery _req, CancellationToken _token)
  {
    // ADIM 1: Kategoriye göre ürünleri getir
    var extras = await _extraRepo.GetByCategoryAsync(_req.Category, _token);

    #region CreatedByName = ""

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

    #endregion

    var dtoList = extras.Select(e => new ExtraSummaryDto(
        Id: e.Id,
        Name: e.Name,
        Price: e.Price,
        PriceType: e.PriceType.ToString(),
        Category: e.Category.ToString(),
        DisplayOrder: e.DisplayOrder,
        IsRecommended: e.IsRecommended,
        IsActive: e.IsActive,
        CreatedAt: e.CreatedAt,
        CreatedByName: GetUserName(e.CreatedBy) // TODO: User service'ten çekilecek
    )).ToList();

    return Result<List<ExtraSummaryDto>>.Succeed(dtoList);
  }
}