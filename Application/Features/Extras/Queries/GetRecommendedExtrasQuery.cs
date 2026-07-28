using Application.Features.Extras.Dto;
using Domain.Repositories;
using Domain.Repositories.Extras;
using MediatR;
using TS.Result;

namespace Application.Features.Extras.Queries;

public sealed record GetRecommendedExtrasQuery : IRequest<Result<List<ExtraSummaryDto>>>;

public sealed class GetRecommendedExtrasQueryHandler(
                          IExtraRepository _extraRepo,
                          IUserRepository _userRepo
                    ) : IRequestHandler<GetRecommendedExtrasQuery, Result<List<ExtraSummaryDto>>>
{
  public async Task<Result<List<ExtraSummaryDto>>> Handle(GetRecommendedExtrasQuery _req, CancellationToken _token)
  {
    var extras = await _extraRepo.GetRecommendedAsync(_token);
    // 1. Listede geçen tüm benzersiz CreatedBy ID'lerini topla
    var userIds = extras.Select(e => e.CreatedBy).Distinct().ToList();

    // 2. Hepsini veritabanından tek sorguda dictionary olarak çek
    var userNames = await _userRepo.GetUserNamesByIdsAsync(userIds, _token);

    // 3. Güvenli getirme fonksiyonu
    string GetUserName(Guid id) => userNames.TryGetValue(id, out var name) ? name : "Bilinmiyor";

    var recommended = extras.Select(e => new ExtraSummaryDto(
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
        CreatedByName: GetUserName(e.CreatedBy)



    )).ToList();

    return Result<List<ExtraSummaryDto>>.Succeed(recommended);
  }
}

  // DateTimeOffset CreatedAt,
  //   string? CreatedByName
