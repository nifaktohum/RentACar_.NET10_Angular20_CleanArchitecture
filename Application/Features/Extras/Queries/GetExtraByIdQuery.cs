using Application.Features.Extras.Dto;
using Domain.Repositories;
using Domain.Repositories.Extras;
using MediatR;
using Microsoft.Extensions.Configuration;
using TS.Result;

namespace Application.Features.Extras.Queries;

public sealed record GetExtraByIdQuery(Guid Id) : IRequest<Result<ExtraDto>>;

public sealed class GetExtraByIdQueryHandler(
                        IExtraRepository _extraRepo,
                        IUserRepository _userRepo
                    ) : IRequestHandler<GetExtraByIdQuery, Result<ExtraDto>>
{
  public async Task<Result<ExtraDto>> Handle(GetExtraByIdQuery _req, CancellationToken _token)
  {
    // ADIM 1: Ürün var mı kontrol et
    var extra = await _extraRepo.FirstOrDefaultAsync(e => e.Id == _req.Id, _token, isTrackingActive: false);
    if (extra is null)
      return Result<ExtraDto>.Failure($"'{_req.Id}' ID'li ekstra hizmet bulunamadı.");

    var userId = new Guid(extra.CreatedBy.ToString());

    var userName = await _userRepo.GetUserNamesByIdsAsync([userId], _token);

    string GetUserName(Guid userId) => userName.GetValueOrDefault(userId, "Bilinmiyor");


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
        CreatedByName: GetUserName(extra.CreatedBy), // TODO: User service'ten çekilecek
        UpdatedAt: extra.UpdatedAt,
        UpdatedBy: extra.UpdatedBy,
        UpdatedByName: extra.UpdatedBy.HasValue ? GetUserName(extra.UpdatedBy.Value) : null
    );

    // ============================================================
    // ADIM 3: Sonucu döndür
    // ============================================================
    return Result<ExtraDto>.Succeed(dto);
  }
}
