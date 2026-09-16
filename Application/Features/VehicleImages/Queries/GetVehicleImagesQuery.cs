using Application.Features.Vehicles.Dto;
using Domain.Repositories;
using Domain.Repositories.Vehicles;
using MediatR;
using TS.Result;

namespace Application.Features.VehicleImages.Queries;

public sealed record GetVehicleImagesQuery(Guid VehicleId) : IRequest<Result<List<VehicleImageDto>>>;

public sealed class GetVehicleImagesQueryHandler(
                          IVehicleImageRepository _vehicleImageRepo,
                          IUserRepository _userRepo
                    ) : IRequestHandler<GetVehicleImagesQuery, Result<List<VehicleImageDto>>>
{
  public async Task<Result<List<VehicleImageDto>>> Handle(GetVehicleImagesQuery _req, CancellationToken _token)
  {

    var images = await _vehicleImageRepo.GetActiveImagesByVehicleAsync(_req.VehicleId, _token);

    var userIds = new List<Guid>();

    foreach (var b in images)
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
    var userNames = await _userRepo.GetUserNamesByIdsAsync(distinctUserIds, _token);

    string GetUserName(Guid userId) => userNames.GetValueOrDefault(userId, "Bilinmiyor");

    var dtos = images.Select(x => new VehicleImageDto(
      Id: x.Id,
      ImageUrl: x.ImageUrl,
      DisplayOrder: x.DisplayOrder,
      IsMain: x.IsMain,
      Description: x.Description,
      IsActive: x.IsActive,
      CreatedAt: x.CreatedAt,
      CreatedByName: GetUserName(x.CreatedBy),
      CreatedBy: x.CreatedBy,
      UpdatedAt: x.UpdatedAt,
      UpdatedBy: x.UpdatedBy,
      UpdatedByName: x.UpdatedBy.HasValue ? GetUserName(x.UpdatedBy.Value) : null

    )).ToList();

    return Result<List<VehicleImageDto>>.Succeed(dtos);
  }
}


