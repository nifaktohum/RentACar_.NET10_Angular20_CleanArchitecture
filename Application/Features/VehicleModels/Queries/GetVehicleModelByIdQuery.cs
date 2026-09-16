using Application.Features.VehicleModels.Dto;
using Application.Features.Vehicles.Specifications;
using Domain.Repositories;
using Domain.Repositories.Vehicles;
using MediatR;
using TS.Result;

namespace Application.Features.VehicleModels.Queries;

public sealed record GetVehicleModelByIdQuery(Guid Id) : IRequest<Result<VehicleModelDetailDto>>;

public sealed class GetVehicleModelByIdQueryHandler(
                        IVehicleModelRepository _vehicleModelRepo,
                        IUserRepository _userRepo
                    ) : IRequestHandler<GetVehicleModelByIdQuery, Result<VehicleModelDetailDto>>
{
  public async Task<Result<VehicleModelDetailDto>> Handle(GetVehicleModelByIdQuery _req, CancellationToken _token)
  {
    // 1. Specification oluştur
    var spec = new GetVehicleModelByIdSpecification(_req.Id);

    var vehicleModel = await _vehicleModelRepo.GetByExpressionAsync(c => c.Id == _req.Id && !c.IsDeleted, _token);

    if (vehicleModel is null) return Result<VehicleModelDetailDto>.Failure("Araç modeli bulunamadı!");

    var userIds = new List<Guid>();

    if (vehicleModel.CreatedBy != Guid.Empty)
      userIds.Add(vehicleModel.CreatedBy);

    if (vehicleModel.UpdatedBy.HasValue && vehicleModel.UpdatedBy.Value != Guid.Empty)
      userIds.Add(vehicleModel.UpdatedBy.Value);

    var userNames = await _userRepo.GetUserNamesByIdsAsync(userIds, _token);

    string GetUserName(Guid id) => userNames.GetValueOrDefault(id, "Bilinmiyor");
    // 3. DTO'ya map et
    var response = new VehicleModelDetailDto(
        vehicleModel.Id,
        vehicleModel.Brand,
        vehicleModel.Name,
        vehicleModel.Description,
        vehicleModel.Stock,
        vehicleModel.AvailableStock,
        vehicleModel.IsInStock,
        vehicleModel.VehicleTypeId,
        vehicleModel.VehicleType?.Name ?? "Belirtilmemiş",
        vehicleModel.VehicleType?.Description,
        vehicleModel.IsActive,
        vehicleModel.CreatedAt,
        vehicleModel.CreatedBy,
        GetUserName(vehicleModel.CreatedBy),
        vehicleModel.UpdatedAt,
        vehicleModel.UpdatedBy,
        vehicleModel.UpdatedBy.HasValue ? GetUserName(vehicleModel.UpdatedBy.Value) : null
    );

    return Result<VehicleModelDetailDto>.Succeed(response);
  }
}
