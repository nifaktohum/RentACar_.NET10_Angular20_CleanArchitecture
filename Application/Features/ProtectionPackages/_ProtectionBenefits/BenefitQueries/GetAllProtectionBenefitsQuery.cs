using Application.Features.ProtectionPackages.Dto;
using Domain.Repositories.Protection;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TS.Result;

namespace Application.Features.ProtectionPackages._ProtectionBenefits.Queries;

public sealed record GetAllProtectionBenefitsQuery: IRequest<Result<List<ProtectionBenefitDto>>>;

public sealed class GetAllProtectionBenefitsQueryHandler(
                            IProtectionBenefitRepository _benefitRepo

                    ) : IRequestHandler<GetAllProtectionBenefitsQuery, Result<List<ProtectionBenefitDto>>>
{
  public async Task<Result<List<ProtectionBenefitDto>>> Handle(GetAllProtectionBenefitsQuery _req, CancellationToken _token)
  {

    var benefits = await _benefitRepo
     .Where(b => b.IsActive && !b.IsDeleted)
     .OrderBy(b => b.DisplayOrder)
     .ToListAsync(_token);

    var dtos = benefits.Select(b => new ProtectionBenefitDto(
                  Id: b.Id,
                  Name: b.Name,
                  Description: b.Description,
                  Icon: b.Icon,
                  DisplayOrder: b.DisplayOrder,
                  Category: b.Category.ToString(),
                  IsActive: b.IsActive,
                  CreatedAt: b.CreatedAt,
                  CreatedBy: b.CreatedBy,
                  CreatedByName: "",
                  UpdatedAt: b.UpdatedAt,
                  UpdatedBy: b.UpdatedBy,
                  UpdatedByName: null
              )).ToList();


    return Result<List<ProtectionBenefitDto>>.Succeed(dtos);
  }
}