using Application.Features.Vehicles.Dto;
using Domain.Repositories;
using Domain.Repositories.Vehicles;
using FluentValidation;
using GenericRepository;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TS.Result;

namespace Application.Features.Vehicles.Commands;

public sealed record UpdateVehicleCommand(
    Guid Id,
    string Brand,
    string Model,
    string Year,
    string Plate,
    string Color,
    Guid VehicleModelId,      
    string FuelType,
    string Transmission,
    int SeatCount,
    int DoorCount,
    int? MinAge,
    decimal DailyPrice,
    string? Description,
    bool IsActive
) : IRequest<Result<UpdateVehicleDto>>;

public sealed class UpdateVehicleCommandValidator : AbstractValidator<UpdateVehicleCommand>
{
  public UpdateVehicleCommandValidator()
  {
    RuleFor(x => x.Id)
        .NotEmpty().WithMessage("Araç ID boş olamaz!");

    RuleFor(x => x.Brand)
        .NotEmpty().WithMessage("Marka boş olamaz!")
        .MaximumLength(50).WithMessage("Marka en fazla 50 karakter olabilir!");

    RuleFor(x => x.Model)
        .NotEmpty().WithMessage("Model boş olamaz!")
        .MaximumLength(50).WithMessage("Model en fazla 50 karakter olabilir!");

    RuleFor(x => x.Year)
        .NotEmpty().WithMessage("Yıl boş olamaz!")
        .Length(4).WithMessage("Yıl 4 haneli olmalı!")
        .Must(y => int.TryParse(y, out var year) && year >= 1900 && year <= 2100)
        .WithMessage("Yıl 1900 ile 2100 arasında olmalı!");

    RuleFor(x => x.Plate)
        .NotEmpty().WithMessage("Plaka boş olamaz!")
        .MaximumLength(20).WithMessage("Plaka en fazla 20 karakter olabilir!")
        .Matches(@"^[A-Z0-9\s-]+$").WithMessage("Plaka geçersiz format!");

    RuleFor(x => x.VehicleModelId)
        .NotEmpty().WithMessage("Araç tipi boş olamaz!");

    RuleFor(x => x.DailyPrice)
        .GreaterThan(0).WithMessage("Günlük fiyat 0'dan büyük olmalı!");
  }
}


public sealed class UpdateVehicleCommandHandler(
                          IVehicleRepository _vehicleRepo,
                          IVehicleModelRepository _vehicleModelRepo,
                          IUserRepository _userRepo,
                          IUnitOfWork _unit,
                          IConfiguration _config,
                          ILogger<UpdateVehicleCommandHandler> _logger
                    ) : IRequestHandler<UpdateVehicleCommand, Result<UpdateVehicleDto>>
{
  public async Task<Result<UpdateVehicleDto>> Handle(UpdateVehicleCommand _req, CancellationToken _token)
  {
    try
    {
      _logger.LogInformation(" Araç güncelleme başladı. VehicleId: {VehicleId}", _req.Id);

      // 1. Vehicle var mı kontrol et
      var vehicle = await _vehicleRepo.GetVehicleWithDetailsAsync(_req.Id, _token);

      if (vehicle is null)
      {
        _logger.LogWarning($"❌ Araç bulunamadı. VehicleId: {_req.Id}");
        return Result<UpdateVehicleDto>.Failure(404, "Araç bulunamadı!");
      }

      // 2. Plate unique mi kontrol et (kendi plakası hariç)
      var isPlateUnique = await _vehicleRepo.IsPlateUniqueAsync(_req.Plate, _req.Id, _token);
      if (!isPlateUnique)
      {
        _logger.LogWarning($"❌ Plaka zaten mevcut. Plate: {_req.Plate}");
        return Result<UpdateVehicleDto>.Failure(409, $"'{_req.Plate}' plakalı araç zaten mevcut!");
      }

      // 3. VehicleType (araç Model) var mı kontrol et
      var vehicleModel = await _vehicleModelRepo.FirstOrDefaultAsync(x => x.Id == _req.VehicleModelId && !x.IsDeleted, _token);
      if (vehicleModel is null)
      {
        _logger.LogWarning($"❌ Araç Model bulunamadı. VehicleTypeId: {_req.VehicleModelId}");
        return Result<UpdateVehicleDto>.Failure(404, "Araç Model bulunamadı!");
      }

      // 4. Vehicle'ı güncelle
      _logger.LogInformation("📝 Araç güncelleniyor...");

      vehicle.UpdateDetails(
               _req.Brand,
               _req.Model,
               _req.Year,
               _req.Plate,
               _req.Color,
               _req.VehicleModelId,
               _req.FuelType,
               _req.Transmission,
               _req.SeatCount,
               _req.DoorCount,
               _req.MinAge,
               _req.DailyPrice,
               _req.Description
           );

      // 5. IsActive durumunu güncelle
      if (_req.IsActive)
      { vehicle.Activate(); }
      else
      { vehicle.Deactivate(); }

      // 6. Güncelleme metadata'sını set et
      var userId = _userRepo.GetCurrentUserId();
      if (userId == Guid.Empty) userId = Guid.Parse(_config["SeedData:AdminUserId"]!);
      vehicle.UpdateMetadata(userId);

      // 7. Save
      // _vehicleRepo.Update(vehicle);
      await _unit.SaveChangesAsync(_token);

      _logger.LogInformation($"✅ Araç başarıyla güncellendi. VehicleId: {_req.Id}");

      var userName = await _userRepo.GetUserNamesByIdsAsync(new List<Guid> { userId }, _token);
      string GetUserName(Guid id) => userName.GetValueOrDefault(id, "Bilinmiyor");

      // 8. ⭐ Ana resmi bul
      var mainImage = vehicle.Images.FirstOrDefault(i => i.IsMain && i.IsActive && !i.IsDeleted);
      var imageUrl = mainImage?.ImageUrl;

      // 8. Response oluştur
      var response = new UpdateVehicleDto(
          vehicle.Id,
          vehicle.Brand,
          vehicle.Model,
          vehicle.Year,
          vehicle.Plate,
          vehicle.Color,
          vehicle.VehicleModelId,
          vehicle.FuelType,
          vehicle.Transmission,
          vehicle.SeatCount,
          vehicle.DoorCount,
          vehicle.MinAge,
          vehicle.DailyPrice,
          vehicle.Description,
          imageUrl,           // ⬅️ Ana resim URL'i
          vehicle.IsActive,
          vehicle.CreatedAt,
          vehicle.CreatedBy,
          GetUserName(vehicle.CreatedBy),
          vehicle.UpdatedAt,
          vehicle.UpdatedBy,
          vehicle.UpdatedBy.HasValue ? GetUserName(vehicle.UpdatedBy.Value) : null
      );

      return Result<UpdateVehicleDto>.Succeed(response);

    }
    catch (Exception _ex)
    {
      _logger.LogError(_ex, $"❌ Araç güncelleme hatası. VehicleId: {_req.Id}");
      return Result<UpdateVehicleDto>.Failure(500, $"Araç güncellenirken bir hata oluştu: {_ex.Message}");
    }
  }
}