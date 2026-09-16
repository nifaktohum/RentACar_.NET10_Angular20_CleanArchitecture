using Domain.Repositories.Vehicles;
using FluentValidation;
using GenericRepository;
using MediatR;
using Microsoft.Extensions.Logging;
using TS.Result;

namespace Application.Features.Vehicles.Commands;

public sealed record ToggleVehicleStatusCommand(Guid Id) : IRequest<Result<Unit>>;

public sealed class ToggleVehicleStatusCommandValidator : AbstractValidator<ToggleVehicleStatusCommand>
{
  public ToggleVehicleStatusCommandValidator()
  {
    RuleFor(x => x.Id)
        .NotEmpty().WithMessage("Araç ID boş olamaz!");
  }
}

public sealed class ToggleVehicleStatusCommandHandler(
                        IVehicleRepository _vehicleRepo,
                        IUnitOfWork _unit,
                        ILogger<ToggleVehicleStatusCommandHandler> _logger
                    ) : IRequestHandler<ToggleVehicleStatusCommand, Result<Unit>>
{
  public async Task<Result<Unit>> Handle(ToggleVehicleStatusCommand _req, CancellationToken _token)
  {
    try
    {
      _logger.LogInformation($"🔄 Araç durum değiştirme başladı. VehicleId: {_req.Id}");

      // 1. Vehicle var mı kontrol et
      var vehicle = await _vehicleRepo.FirstOrDefaultAsync(v => v.Id == _req.Id, _token);
      if (vehicle is null)
      {
        _logger.LogWarning($"❌ Araç bulunamadı. VehicleId: {_req.Id}");
        return Result<Unit>.Failure(404, "Araç bulunamadı!");
      }

      // 2. Durumu değiştir (Active ↔ Passive)
      if (vehicle.IsActive)
      { vehicle.Deactivate(); }
      else
      { vehicle.Activate(); }

      // 3. Save
      _vehicleRepo.Update(vehicle);
      await _unit.SaveChangesAsync(_token);

      _logger.LogInformation($"✅ Araç durumu değiştirildi. VehicleId: {_req.Id}, Yeni Durum: {vehicle.IsActive}");

      return Result<Unit>.Succeed(Unit.Value);
    }
    catch (Exception _ex)
    {
      _logger.LogError(_ex, $"❌ Araç durum değiştirme hatası. VehicleId: {_req.Id}");
      return Result<Unit>.Failure(500, $"Araç durumu değiştirilirken bir hata oluştu: {_ex.Message}");
    }
  }
}