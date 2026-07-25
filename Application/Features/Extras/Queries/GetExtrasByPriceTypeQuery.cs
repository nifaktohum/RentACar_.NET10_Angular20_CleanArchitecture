using Application.Features.Extras.Dto;
using Domain.Entities.Extras.Enum;
using Domain.Repositories;
using Domain.Repositories.Extras;
using FluentValidation;
using MediatR;
using TS.Result;

namespace Application.Features.Extras.Queries;

public sealed record GetExtrasByPriceTypeQuery(
                          int PriceType
                    ) : IRequest<Result<List<ExtraSummaryDto>>>;

public sealed class GetExtrasByPriceTypeQueryValidator : AbstractValidator<GetExtrasByPriceTypeQuery>
{
  public GetExtrasByPriceTypeQueryValidator()
  {
    RuleFor(x => x.PriceType)
         .Must(x => Enum.IsDefined(typeof(PriceType), x))
         .WithMessage("Geçersiz ücret hesaplama. (Daily = 1, Rental = 2)");
  }
}

public sealed class GetExtrasByPriceTypeQueryHandler(
                        IExtraRepository _extraRepo,
                        IUserRepository _userRepo
                    ) : IRequestHandler<GetExtrasByPriceTypeQuery, Result<List<ExtraSummaryDto>>>
{
  public async Task<Result<List<ExtraSummaryDto>>> Handle(GetExtrasByPriceTypeQuery _req, CancellationToken _token)
  {
    // ADIM 1: Fiyat tipine göre ürünleri getir
    var extras = await _extraRepo.GetByPriceTypeAsync(_req.PriceType, _token);

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