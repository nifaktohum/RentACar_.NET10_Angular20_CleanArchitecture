using Application.Features.VehicleModels.Dto;
using Domain.Repositories;
using Domain.Repositories.Vehicles;
using FluentValidation;
using GenericRepository;
using MediatR;
using TS.Result;

namespace Application.Features.VehicleModels.Commands;

public sealed record UpdateVehicleModelCommand(
                          Guid Id,
                          string Name,
                          string Brand,
                          string? Description,
                          int Stock,
                          Guid VehicleTypeId
                      ) : IRequest<Result<UpdateVehicleModelDto>>;

public sealed class UpdateVehicleModelCommandValidator : AbstractValidator<UpdateVehicleModelCommand>
{
  public UpdateVehicleModelCommandValidator()
  {
    RuleFor(x => x.Id)
        .NotEmpty().WithMessage("Model ID boş olamaz!");

    RuleFor(x => x.Name)
        .NotEmpty().WithMessage("Model adı boş olamaz!")
        .MaximumLength(50).WithMessage("Model adı en fazla 50 karakter olabilir!");

    RuleFor(x => x.Brand)
        .NotEmpty().WithMessage("Marka adı boş olamaz!")
        .MaximumLength(50).WithMessage("Marka adı en fazla 50 karakter olabilir!");

    RuleFor(x => x.Description)
        .MaximumLength(500).WithMessage("Açıklama en fazla 500 karakter olabilir!");

    RuleFor(x => x.Stock)
        .GreaterThanOrEqualTo(0).WithMessage("Stok 0'dan küçük olamaz!");

    RuleFor(x => x.VehicleTypeId)
        .NotEmpty().WithMessage("Araç tipi seçilmelidir!");
  }
}

public sealed class UpdateVehicleModelCommandHandler(
                        IVehicleModelRepository _vehicleModelRepo,
                        IVehicleTypeRepository _vehicleTypeRepo,
                        IUserRepository _userRepo,
                        IUnitOfWork _unit
                    ) : IRequestHandler<UpdateVehicleModelCommand, Result<UpdateVehicleModelDto>>
{
  public async Task<Result<UpdateVehicleModelDto>> Handle(UpdateVehicleModelCommand _req, CancellationToken _token)
  {
    // 1. VehicleModel var mı kontrol et
    var vehicleModel = await _vehicleModelRepo.FirstOrDefaultAsync(vm => vm.Id == _req.Id, _token);
    if (vehicleModel is null) return Result<UpdateVehicleModelDto>.Failure("Güncellenecek model bulunamadı!");

    // 2. Soft delete kontrolü
    if (vehicleModel.IsDeleted) return Result<UpdateVehicleModelDto>.Failure("Bu model silinmiş, güncellenemez!");

    // 3. Aynı marka ve model başka bir kayıtta var mı kontrol et (kendi hariç)
    var exists = await _vehicleModelRepo.ExistsByBrandAndNameExcludeSelfAsync(
                                                _req.Brand,
                                                _req.Name,
                                                _req.Id,
                                                _token
                                            );

    if (exists) return Result<UpdateVehicleModelDto>.Failure($"'{_req.Brand} - {_req.Name}' zaten mevcut!");

    // 4. VehicleType var mı kontrol et
    var vehicleType = await _vehicleTypeRepo.FirstOrDefaultAsync(vt => vt.Id == _req.VehicleTypeId && !vt.IsDeleted, _token);
    if (vehicleType is null) return Result<UpdateVehicleModelDto>.Failure("Seçilen araç tipi bulunamadı!");

    // 5. Current user ID'yi al
    var userId = _userRepo.GetCurrentUserId();
    if (userId == Guid.Empty) return Result<UpdateVehicleModelDto>.Failure("Kullanıcı bilgisi alınamadı!");

    // 6. Domain metodları ile güncelle
    vehicleModel.SetName(_req.Name);
    vehicleModel.SetBrand(_req.Brand);
    vehicleModel.SetDescription(_req.Description);
    vehicleModel.SetStock(_req.Stock);
    vehicleModel.SetVehicleType(vehicleType);

    // 7. Update metadata
    vehicleModel.UpdateMetadata(userId);

    // 8. Save
    _vehicleModelRepo.Update(vehicleModel);
    await _unit.SaveChangesAsync(_token);

    // 9. User name'i al
    var userNames = await _userRepo.GetUserNamesByIdsAsync(
                                        new List<Guid> { userId },
                                        _token
                                    );

    string GetUserName(Guid id) => userNames.GetValueOrDefault(id, "Bilinmiyor");

    // 10. Response oluştur
    var response = new UpdateVehicleModelDto(
        vehicleModel.Id,
        vehicleModel.Brand,
        vehicleModel.Name,
        vehicleModel.Description,
        vehicleModel.Stock,
        vehicleModel.AvailableStock,
        vehicleModel.IsInStock,
        vehicleModel.VehicleTypeId,
        vehicleType.Name,
        vehicleModel.UpdatedAt,
        vehicleModel.UpdatedBy,
        vehicleModel.UpdatedBy.HasValue ? GetUserName(vehicleModel.UpdatedBy.Value) : null

    );

    return Result<UpdateVehicleModelDto>.Succeed(response);

  }
}