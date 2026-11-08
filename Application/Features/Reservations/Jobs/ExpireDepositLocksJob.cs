using Application.Features.Reservations.Specifications;
using Domain.Entities.Vehicles;
using Domain.Repositories;
using Domain.Repositories.Vehicles;
using GenericRepository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Application.Features.Reservations.Jobs;

public sealed class ExpireDepositLocksJob(
    IReservationRepository _reservationRepo,
    IVehicleRepository _vehicleRepo,
    IUserRepository _userRepo,
    IUnitOfWork _unit,
    ILogger<ExpireDepositLocksJob> _logger,
    IConfiguration _config)
{
  public async Task ExecuteAsync(CancellationToken _token)
  {
    var userId = _userRepo.GetCurrentUserId();
    if (userId == Guid.Empty) userId = Guid.Parse(_config["SeedData:AdminUserId"]!);

    var spec = new ExpiredDepositLocksSpecification();
    var expiredReservations = await _reservationRepo.GetAsync(spec, _token);

    if (!expiredReservations.Any()) return;

    _logger.LogInformation("⌛ {Count} adet 24 saatlik süresi dolan rezervasyon bulundu. İptal ediliyor...", expiredReservations.Count);

    foreach (var reservation in expiredReservations)
    {
      reservation.MarkAsCancelledAfterDeposit(userId);
      _reservationRepo.Update(reservation);

      var vehicle = await _vehicleRepo.GetByIdAsync(reservation.VehicleId, _token);
      if (vehicle != null && vehicle.Status == VehicleStatus.Rented)
      {
        vehicle.Return(vehicle.CurrentKilometer);
        _vehicleRepo.Update(vehicle);
      }
    }

    await _unit.SaveChangesAsync(_token);
    _logger.LogInformation("✅ 24 saati dolan rezervasyonlar iptal edildi ve araçlar stoğa döndü.");
  }
}