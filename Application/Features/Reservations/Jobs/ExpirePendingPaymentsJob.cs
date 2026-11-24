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
  // 1️⃣ Arka plan işini başlatan ve BackgroundJobScheduler tarafından tetiklenen ana metodumuz.
  public async Task ExecuteAsync(CancellationToken _token)
  {
    // 2️⃣ Arka planda HTTP isteği (tarayıcı oturumu) olmadığı için aktif kullanıcı ID'sini almaya çalışıyoruz.
    var userId = _userRepo.GetCurrentUserId();

    // 3️⃣ Eğer arkada çalışan bir istek yoksa (userId boş dönüyorsa), 
    // appsettings.json içerisindeki sabit Admin / Sistem ID'sini fallback olarak atıyoruz.
    if (userId == Guid.Empty) userId = Guid.Parse(_config["SeedData:AdminUserId"]!);

    // 4️⃣ Ödeme süresi dolmuş (pending durumda ama süresi aşılmış) rezervasyonları yakalamak için Specification'ı çağırıyoruz.
    var spec = new ExpiredPendingPaymentsSpecification();

    // 5️⃣ Karta/ödeme sağlayıcıya gönderilip belirli sürede (örn: 15 dk) ödenmeyen rezervasyonları veritabanından çekiyoruz.
    var expiredReservations = await _reservationRepo.GetAsync(spec, _token);

    // 6️⃣ Eğer süresi dolan kayıt yoksa, gereksiz işlem yapmamak için direkt metoddan çıkıyoruz (Early Return).
    if (!expiredReservations.Any()) return;

    // 7️⃣ Süresi dolan kayıtların sayısını logluyoruz.
    _logger.LogInformation("⏰ {Count} adet süresi dolan rezervasyon bulundu. Expired yapılıyor...", expiredReservations.Count);

    // 8️⃣ Bulunan her bir süresi geçmiş rezervasyon üzerinde dönüyoruz.
    foreach (var reservation in expiredReservations)
    {
      // 9️⃣ Domain katmanındaki iş kuralını çalıştırarak rezervasyonu 'Expired' durumuna çekiyoruz (Audit için userId veriyoruz).
      reservation.MarkAsExpired(userId);

      // 🔟 Rezervasyondaki bu durumu repository'nin ChangeTracker mekanizmasına bildiriyoruz.
      _reservationRepo.Update(reservation);
    }

    // 11 Döngü bitti; yapılan tüm değişiklikleri Unit of Work ile tek seferde (Atomic Transaction) veritabanına kaydediyoruz.
    await _unit.SaveChangesAsync(_token);

    // 12 Sürecin başarıyla tamamlandığını logluyoruz.
    _logger.LogInformation("✅ Süresi dolan ödemeler başarıyla Expired yapıldı.");
  }
}