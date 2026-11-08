using Application.Features.Reservations.Specifications;
using Domain.Repositories;
using GenericRepository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Application.Features.Reservations.Jobs;

public sealed class ExpirePendingPaymentsJob(
    IReservationRepository _reservationRepo,
    IUserRepository _userRepo,
    IUnitOfWork _unit,
    ILogger<ExpirePendingPaymentsJob> _logger,
    IConfiguration _config)
{
  public async Task ExecuteAsync(CancellationToken _token)
  {
    var userId = _userRepo.GetCurrentUserId();
    if (userId == Guid.Empty) userId = Guid.Parse(_config["SeedData:AdminUserId"]!);

    var spec = new ExpiredPendingPaymentsSpecification();
    var expiredReservations = await _reservationRepo.GetAsync(spec, _token);

    if (!expiredReservations.Any()) return;

    _logger.LogInformation("⏰ {Count} adet süresi dolan rezervasyon bulundu. Expired yapılıyor...", expiredReservations.Count);

    foreach (var reservation in expiredReservations)
    {
      reservation.MarkAsExpired(userId);
      _reservationRepo.Update(reservation);
    }

    await _unit.SaveChangesAsync(_token);
    _logger.LogInformation("✅ Süresi dolan ödemeler başarıyla Expired yapıldı.");
  }
}