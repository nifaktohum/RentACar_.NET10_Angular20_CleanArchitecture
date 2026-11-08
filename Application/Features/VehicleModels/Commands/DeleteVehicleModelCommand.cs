using Application.Behaviors;
using Domain.Repositories;
using Domain.Repositories.Vehicles;
using FluentValidation;
using GenericRepository;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TS.Result;

namespace Application.Features.VehicleModels.Commands;

[Permission("VehicleModels.Delete")]
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
    IVehicleRepository _vehicleRepo,
    IUserRepository _userRepo,
    IConfiguration _config,
    IUnitOfWork _unit,
    ILogger<DeleteVehicleModelCommandHandler> _logger
) : IRequestHandler<DeleteVehicleModelCommand, Result<Unit>>
{
  public async Task<Result<Unit>> Handle(DeleteVehicleModelCommand _req, CancellationToken _token)
  {
    try
    {
      _logger.LogInformation("🗑️ VehicleModel silme başladı. ModelId: {ModelId}", _req.Id);

      // 1. VehicleModel var mı kontrol et
      var vehicleModel = await _vehicleModelRepo.FirstOrDefaultAsync(vm => vm.Id == _req.Id, _token);
      if (vehicleModel is null)
      {
        _logger.LogWarning("❌ Silinecek model bulunamadı. ModelId: {ModelId}", _req.Id);
        return Result<Unit>.Failure(404, "Silinecek model bulunamadı!");
      }

      if (vehicleModel.IsDeleted)
        return Result<Unit>.Failure(400, "Bu model zaten silinmiş!");

      // 2. Aktif araç kontrolü (Hem silinmemiş hem de sistemde kayıtlı olan araçlar)
      var activeVehiclesCount = await _vehicleRepo.CountAsync(v => v.VehicleModelId == _req.Id && !v.IsDeleted, _token);

      if (activeVehiclesCount > 0)
      {
        _logger.LogWarning("⚠️ Model silinemez, bağlı {Count} adet aktif araç var. ModelId: {ModelId}", activeVehiclesCount, _req.Id);
        return Result<Unit>.Failure(
            400,
            $"'{vehicleModel.Brand} - {vehicleModel.Name}' modeline bağlı {activeVehiclesCount} adet aktif araç bulunmaktadır. Önce bu araçları silmelisiniz."
        );
      }

      // 3. Yetki ve Audit bilgisi
      var userId = _userRepo.GetCurrentUserId();
      if (userId == Guid.Empty)
        userId = Guid.Parse(_config["SeedData:AdminUserId"]!);

      // 4. Soft Delete İşlemi
      vehicleModel.SoftDelete(userId);

      // 5. Save
      _vehicleModelRepo.Update(vehicleModel);
      await _unit.SaveChangesAsync(_token);

      _logger.LogInformation("✅ VehicleModel başarıyla silindi (Soft Delete). Id: {Id}, Brand: {Brand}, Name: {Name}",
          vehicleModel.Id, vehicleModel.Brand, vehicleModel.Name);

      return Result<Unit>.Succeed(Unit.Value);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "❌ VehicleModel silme hatası. ModelId: {ModelId}", _req.Id);
      return Result<Unit>.Failure(500, $"Model silinirken bir hata oluştu: {ex.Message}");
    }
  }
}