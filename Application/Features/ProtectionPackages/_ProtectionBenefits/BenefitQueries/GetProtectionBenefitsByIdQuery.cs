using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Features.ProtectionPackages.Dto;
using Domain.Repositories.Protection;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TS.Result;

namespace Application.Features.ProtectionPackages._ProtectionBenefits.BenefitQueries;

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
                            IProtectionBenefitRepository _benefitRepo
                    ) : IRequestHandler<GetProtectionBenefitsByIdQuery, Result<ProtectionBenefitDto>>
{
  public async Task<Result<ProtectionBenefitDto>> Handle(GetProtectionBenefitsByIdQuery _req, CancellationToken _token)
  {
    // 1. Benefit'i getir
    var benefit = await _benefitRepo.GetByIdBenefitsAsync(_req.Id);

    // 2. Benefit bulunamadıysa
    if (benefit is null)
                return Result<ProtectionBenefitDto>.Failure(404, "İstenilen koruma özelliği bulunamadı.");
    

    // 3. Response oluştur
    var dto = new ProtectionBenefitDto(
        Id: benefit.Id,
        Name: benefit.Name,
        Description: benefit.Description,
        Icon: benefit.Icon,
        DisplayOrder: benefit.DisplayOrder,
        Category: benefit.Category.ToString(),
        IsActive: benefit.IsActive,
        CreatedAt: benefit.CreatedAt,
        CreatedBy: benefit.CreatedBy,
        CreatedByName: "", // Kullanıcı adı servisinden doldurulacak
        UpdatedAt: benefit.UpdatedAt,
        UpdatedBy: benefit.UpdatedBy,
        UpdatedByName: null
    );

    return Result<ProtectionBenefitDto>.Succeed(dto);

  }
}
