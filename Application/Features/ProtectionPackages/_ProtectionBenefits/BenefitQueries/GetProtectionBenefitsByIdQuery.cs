using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Behaviors;
using Application.Features.ProtectionPackages.Dto;
using Domain.Repositories;
using Domain.Repositories.Protection;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TS.Result;

namespace Application.Features.ProtectionPackages._ProtectionBenefits.BenefitQueries;

[Permission("ProtectionBenefit.Read")]
public sealed record GetProtectionBenefitsByIdQuery(Guid Id) : IRequest<Result<ProtectionBenefitDto>>;

public sealed class GetProtectionBenefitsByIdValidator : AbstractValidator<GetProtectionBenefitsByIdQuery>
{
  public GetProtectionBenefitsByIdValidator()
  {
    RuleFor(x => x.Id)
    .NotEmpty().WithMessage("Benefit ID boş olamaz.")
    .NotNull().WithMessage("Benefit ID null olamaz.");
  }
}

public sealed class GetProtectionBenefitsByIdQueryHandler(
                            IProtectionBenefitRepository _benefitRepo,
                            IUserRepository _userRepo
                    ) : IRequestHandler<GetProtectionBenefitsByIdQuery, Result<ProtectionBenefitDto>>
{
  public async Task<Result<ProtectionBenefitDto>> Handle(GetProtectionBenefitsByIdQuery _req, CancellationToken _token)
  {
    // 1. Benefit'i getir
    var benefit = await _benefitRepo.GetByIdBenefitAsync(_req.Id);

    // 2. Benefit bulunamadıysa
    if (benefit is null)
                return Result<ProtectionBenefitDto>.Failure(404, "İstenilen koruma özelliği bulunamadı.");

    #region  CreatedByName: "";
        // sistemdeki tüm kullanıcı ID'lerini bir havuzda topluyoruz.
        var userId = new List<Guid>();


        // Ana paketin oluşturucusu ve güncelleyicisi
        userId.Add(benefit.CreatedBy);
        if (benefit.UpdatedBy.HasValue)
        {
          userId.Add(benefit.UpdatedBy.Value);

        }

        // gereksiz sorgu atmamak için Distinct() ile ID'leri tekilleştiriyoruz.
        var distinctUserIds = userId.Distinct().ToList();
        // tüm kullanıcıların isimlerini (FullName) bir Dictionary olarak çekiyoruz.
        var userNames = await _userRepo.GetUserNamesByIdsAsync(distinctUserIds, _token);

        // Eğer ID'ye ait isim bulunamazsa "Bilinmiyor" döndürerek hata almamızı engelliyoruz.
        string GetUserName(Guid userId) => userNames.GetValueOrDefault(userId, "Bilinmiyor");

    #endregion 


    // 3. Response oluştur
    var dto = new ProtectionBenefitDto(
        Id: benefit.Id,
        Name: benefit.Name,
        Description: benefit.Description,
        Icon: benefit.Icon,
        DisplayOrder: benefit.DisplayOrder,
        Category: benefit.Category?.Name ?? "Bilinmiyor",
        CategoryId: benefit.CategoryId,
        IsActive: benefit.IsActive,
        CreatedAt: benefit.CreatedAt,
        CreatedBy: benefit.CreatedBy,
        CreatedByName: GetUserName(benefit.CreatedBy), // Kullanıcı adı servisinden doldurulacak
        UpdatedAt: benefit.UpdatedAt,
        UpdatedBy: benefit.UpdatedBy,
        UpdatedByName: benefit.UpdatedBy.HasValue ? GetUserName(benefit.UpdatedBy.Value) : null
    );

    return Result<ProtectionBenefitDto>.Succeed(dto);

  }
}
