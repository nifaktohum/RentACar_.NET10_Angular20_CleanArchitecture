using Domain.Repositories;
using Domain.Repositories.Vehicles;
using FluentValidation;
using GenericRepository;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TS.Result;

namespace Application.Features.Vehicles.Commands;

public sealed record DeleteVehicleCommand(Guid Id) : IRequest<Result<Unit>>;

public sealed class DeleteVehicleCommandValidator : AbstractValidator<DeleteVehicleCommand>
{
  public DeleteVehicleCommandValidator()
  {
    RuleFor(x => x.Id)
        .NotEmpty().WithMessage("Araç ID boş olamaz!");
  }
}

public sealed class DeleteVehicleCommandHandler(
                        IVehicleRepository _vehicleRepo,
                        IUserRepository _userRepo,
                        IConfiguration _config,
                        IUnitOfWork _unit,
                        ILogger<DeleteVehicleCommandHandler> _logger
                    ) : IRequestHandler<DeleteVehicleCommand, Result<Unit>>
{
  public async Task<Result<Unit>> Handle(DeleteVehicleCommand _req, CancellationToken _token)
  {
    try
    {
      _logger.LogInformation($"🗑️ Araç silme başladı. VehicleId: {_req.Id}");

      // 1. Vehicle var mı kontrol et
      var vehicle = await _vehicleRepo.FirstOrDefaultAsync( v => v.Id == _req.Id, _token);
      if (vehicle is null)
      {
        _logger.LogWarning($"❌ Araç bulunamadı. VehicleId: {_req.Id}");
        return Result<Unit>.Failure(404, "Araç bulunamadı!");
      }

      // 3. Save
      _vehicleRepo.Delete(vehicle);
      await _unit.SaveChangesAsync(_token);

      // 2. Soft Delete
      var userId = _userRepo.GetCurrentUserId();
      if (userId == Guid.Empty) userId = Guid.Parse(_config["SeedData:AdminUserId"]!);
      vehicle.SoftDelete(userId);

      _logger.LogInformation($"✅ Araç başarıyla silindi. VehicleId: {_req.Id}");

      return Result<Unit>.Succeed(Unit.Value);
    }
    catch (Exception _ex)
    {
      _logger.LogError(_ex, "❌ Araç silme hatası. VehicleId: {VehicleId}", _req.Id);
      return Result<Unit>.Failure(500, $"Araç silinirken bir hata oluştu: {_ex.Message}");
    }
    throw new NotImplementedException();
  }
}

