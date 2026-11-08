using Application.Features.VehicleModels.Dto;
using Application.Features.Vehicles.Specifications;
using Domain.Repositories;
using Domain.Repositories.Vehicles;
using MediatR;
using Microsoft.EntityFrameworkCore; // 🆕 Include ve FirstOrDefaultAsync için eklendi
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
    // 1. VehicleModel'i ilişkili tablolarıyla birlikte çekiyoruz
    // 🚨 KRİTİK: Stock ve AvailableStock hesaplanabilmesi için Vehicles'ı, 
    // VehicleType adını gösterebilmek için VehicleType'ı Include etmeliyiz.
    var vehicleModel = await _vehicleModelRepo
        .Where(vm => vm.Id == _req.Id && !vm.IsDeleted)
        .Include(vm => vm.Vehicles)
        .Include(vm => vm.VehicleType)
        .FirstOrDefaultAsync(_token);

    if (vehicleModel is null)
      return Result<VehicleModelDetailDto>.Failure("Araç modeli bulunamadı!");

    // 2. Kullanıcı isimlerini hazırla
    var userIds = new List<Guid>();
    if (vehicleModel.CreatedBy != Guid.Empty) userIds.Add(vehicleModel.CreatedBy);
    if (vehicleModel.UpdatedBy.HasValue && vehicleModel.UpdatedBy.Value != Guid.Empty) userIds.Add(vehicleModel.UpdatedBy.Value);

    var userNames = await _userRepo.GetUserNamesByIdsAsync(userIds, _token);
    string GetUserName(Guid id) => userNames.GetValueOrDefault(id, "Bilinmiyor");

    // 3. DTO'ya map et
    var response = new VehicleModelDetailDto(
        vehicleModel.Id,
        vehicleModel.Brand,
        vehicleModel.Name,
        vehicleModel.Description,
        vehicleModel.Stock,             // ✅ Include sayesinde doğru hesaplanacak
        vehicleModel.AvailableStock,    // ✅ Include sayesinde doğru hesaplanacak
        vehicleModel.IsInStock,         // ✅
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