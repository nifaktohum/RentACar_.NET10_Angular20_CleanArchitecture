using Domain.Repositories.Vehicles;
using FluentValidation;
using GenericRepository;
using MediatR;
using Microsoft.Extensions.Logging;
using TS.Result;

namespace Application.Features.Vehicles.Commands;

public sealed record UpdateVehicleStockCommand(
                          Guid Id,
                          int Stock
                      ) : IRequest<Result<Unit>>;

public sealed class UpdateVehicleStockCommandValidator : AbstractValidator<UpdateVehicleStockCommand>
{
  public UpdateVehicleStockCommandValidator()
  {
    RuleFor(x => x.Id)
        .NotEmpty().WithMessage("Araç ID boş olamaz!");

    RuleFor(x => x.Stock)
        .GreaterThanOrEqualTo(0).WithMessage("Stok 0'dan küçük olamaz!")
        .LessThanOrEqualTo(9999).WithMessage("Stok en fazla 9999 olabilir!");
  }
}

public sealed class UpdateVehicleStockCommandHandler(
                        IVehicleRepository _vehicleRepo,
                        IUnitOfWork _unit,
                        ILogger<UpdateVehicleStockCommandHandler> _logger
                    ) : IRequestHandler<UpdateVehicleStockCommand, Result<Unit>>
{
  public async Task<Result<Unit>> Handle(UpdateVehicleStockCommand _req, CancellationToken _token)
  {
    try
    {
      _logger.LogInformation($"📦 Stok güncelleme başladı. VehicleId: {_req.Id}, Yeni Stok: {_req.Stock}");

      // 1. Vehicle var mı kontrol et
      var vehicle = await _vehicleRepo.FirstOrDefaultAsync(v => v.Id == _req.Id, _token);
      if (vehicle is null)
      {
        _logger.LogWarning($"❌ Araç bulunamadı. VehicleId: {_req.Id}");
        return Result<Unit>.Failure(404, "Araç bulunamadı!");
      }

      // 2. Stok güncelle
      vehicle.VehicleModel.SetStock(_req.Stock);

      // 3. Save
      _vehicleRepo.Update(vehicle);
      await _unit.SaveChangesAsync(_token);

      _logger.LogInformation($"✅ Stok başarıyla güncellendi. VehicleId: {_req.Id}, Yeni Stok: {_req.Stock}");

      return Result<Unit>.Succeed(Unit.Value);
    }
    catch (Exception _ex)
    {
      _logger.LogError(_ex, $"❌ Stok güncelleme hatası. VehicleId: {_req.Id}");
      return Result<Unit>.Failure(500, $"Stok güncellenirken bir hata oluştu: {_ex.Message}");
    }
  }
}