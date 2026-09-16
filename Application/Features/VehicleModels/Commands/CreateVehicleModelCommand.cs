using System.Formats.Tar;
using Application.Features.VehicleModels.Dto;
using Domain.Entities.Vehicles;
using Domain.Repositories;
using Domain.Repositories.Vehicles;
using FluentValidation;
using GenericRepository;
using MediatR;
using TS.Result;

namespace Application.Features.VehicleModels.Commands;

public sealed record CreateVehicleModelCommand(
                        string Name,
                        string Brand,
                        string? Description,
                        int Stock,
                        Guid VehicleTypeId
                    ) : IRequest<Result<CreateVehicleModelDto>>;

public sealed class CreateVehicleModelCommandValidator : AbstractValidator<CreateVehicleModelCommand>
{
  public CreateVehicleModelCommandValidator()
  {
    // Name
    RuleFor(x => x.Name)
        .NotEmpty().WithMessage("Model adı boş olamaz!")
        .MaximumLength(50).WithMessage("Model adı en fazla 50 karakter olabilir!");

    // Brand
    RuleFor(x => x.Brand)
        .NotEmpty().WithMessage("Marka adı boş olamaz!")
        .MaximumLength(50).WithMessage("Marka adı en fazla 50 karakter olabilir!");

    // Description
    RuleFor(x => x.Description)
        .MaximumLength(500).WithMessage("Açıklama en fazla 500 karakter olabilir!");

    // Stock
    RuleFor(x => x.Stock)
        .GreaterThanOrEqualTo(0).WithMessage("Stok 0'dan küçük olamaz!");

    // VehicleTypeId
    RuleFor(x => x.VehicleTypeId)
        .NotEmpty().WithMessage("Araç tipi seçilmelidir!");
  }
}

public sealed class CreateVehicleModelCommandHandler(
                        IVehicleModelRepository _vehicleModelRepo,
                        IVehicleTypeRepository _vehicleTypeRepo,
                        IUserRepository _userRepo,
                        IUnitOfWork _unit
                    ) : IRequestHandler<CreateVehicleModelCommand, Result<CreateVehicleModelDto>>
{
  public async Task<Result<CreateVehicleModelDto>> Handle(CreateVehicleModelCommand _req, CancellationToken _token)
  {
      // 1. Aynı marka ve model zaten var mı kontrol et
      var exists = await _vehicleModelRepo.ExistsByBrandAndNameAsync(_req.Brand, _req.Name);
      if (exists) return Result<CreateVehicleModelDto>.Failure($"'{_req.Brand} - {_req.Name}' zaten mevcut!");

      // 2. VehicleType var mı kontrol et
      var vehicleType = await _vehicleTypeRepo.FirstOrDefaultAsync( vt => vt.Id == _req.VehicleTypeId, _token);
      if (vehicleType is null)
        return Result<CreateVehicleModelDto>.Failure("Seçilen araç tipi bulunamadı!");

      // 3. Current user ID'yi al
      var userId = _userRepo.GetCurrentUserId();
      if (userId == Guid.Empty) return Result<CreateVehicleModelDto>.Failure("Kullanıcı bilgisi alınamadı!");

      // 4. Domain entity oluştur (Domain'deki constructor'ı kullan)
      var vehicleModel = new VehicleModel(
          name: _req.Name,
          brand: _req.Brand,
          description: _req.Description,
          stock: _req.Stock,
          vehicleTypeId: _req.VehicleTypeId,
          createdBy: userId
      );

      // 5. VehicleType'ı set et (navigation property için)
      vehicleModel.SetVehicleType(vehicleType);

      // 6. Save
      await _vehicleModelRepo.AddAsync(vehicleModel, _token);
      await _unit.SaveChangesAsync(_token);
      // 7. User name'i al
      var userName = await _userRepo.GetUserNamesByIdsAsync(
          new List<Guid> { userId }, _token );

      string GetUserName(Guid id) => userName.GetValueOrDefault(id, "Bilinmiyor");

      // 8. Response oluştur
      var response = new CreateVehicleModelDto(
          vehicleModel.Id,
          vehicleModel.Brand,
          vehicleModel.Name,
          vehicleModel.Description,
          vehicleModel.Stock,
          vehicleModel.AvailableStock,
          vehicleModel.IsInStock,
          vehicleModel.VehicleTypeId,
          vehicleType?.Name ?? "Belirtilmemiş",
          vehicleModel.CreatedAt,
          vehicleModel.CreatedBy,
          GetUserName(vehicleModel.CreatedBy)
      );

      return Result<CreateVehicleModelDto>.Succeed(response);

  }
}
