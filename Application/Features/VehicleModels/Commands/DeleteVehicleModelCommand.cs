using Domain.Repositories.Vehicles;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TS.Result;

namespace Application.Features.VehicleModels.Commands;

public sealed record DeleteVehicleModelCommand(Guid Id) : IRequest<Result<Unit>>;

public sealed class DeleteVehicleModelCommandValidator : AbstractValidator<DeleteVehicleModelCommand>
{
  public DeleteVehicleModelCommandValidator()
  {
    RuleFor(x => x.Id)
        .NotEmpty().WithMessage("Model ID boş olamaz!");
  }
}

public sealed class DeleteVehicleModelCommandHandler(
    IVehicleModelRepository _vehicleModelRepo,
    IVehicleRepository _vehicleRepo,  // Araçlar kontrol edilecek
    ILogger<DeleteVehicleModelCommandHandler> _logger
) : IRequestHandler<DeleteVehicleModelCommand, Result<Unit>>
{
  public async Task<Result<Unit>> Handle(DeleteVehicleModelCommand _req, CancellationToken _token)
  {
    // 1. VehicleModel var mı kontrol et
    // 1. Silinecek VehicleModel var mı kontrol et
    var vehicleModel = await _vehicleModelRepo.FirstOrDefaultAsync(vm => vm.Id == _req.Id, _token);
    if (vehicleModel is null) return Result<Unit>.Failure("Silinecek model bulunamadı!");

    // 2. Sadece AKTİF (silinmemiş: !IsDeleted) araç var mı kontrol et
    // Soft-delete yapılmış araçlar (IsDeleted = true) tamamen görmezden gelinir
    var hasActiveVehicles = await _vehicleRepo.AnyAsync(v => v.VehicleModelId == _req.Id && !v.IsDeleted, _token);
    if (hasActiveVehicles)
    {
      return Result<Unit>.Failure(
          400,
          $"'{vehicleModel.Brand} - {vehicleModel.Name}' modeline bağlı aktif araçlar bulunmaktadır. Önce bu araçları silmelisiniz."
      );
    }

    // 3. PostgreSQL FK kısıtlamasına takılmamak için bu modele ait soft-deleted araçları veritabanından temizle
    await _vehicleRepo.GetAll()
        .IgnoreQueryFilters()
        .Where(v => v.VehicleModelId == _req.Id && v.IsDeleted)
        .ExecuteDeleteAsync(_token);

    // 4. VehicleModel'ı veritabanından tamamen sil (HARD DELETE)
    await _vehicleModelRepo.HardDeleteAsync(_req.Id, _token);

    _logger.LogInformation(
        "🔥 VehicleModel veritabanından tamamen silindi (Hard Delete). Id: {Id}, Brand: {Brand}, Name: {Name}",
        vehicleModel.Id,
        vehicleModel.Brand,
        vehicleModel.Name
    );

    return Result<Unit>.Succeed(Unit.Value);

  }
}