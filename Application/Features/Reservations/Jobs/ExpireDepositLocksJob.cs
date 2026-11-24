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
  // 1️⃣ Arka plan işini başlatan ve dışarıdan (BackgroundJobScheduler tarafından) tetiklenen ana metodumuz.
  public async Task ExecuteAsync(CancellationToken _token)
  {
    // 2️⃣ Arka planda HTTP isteği (tarayıcıdan gelen bir istek) olmadığı için HttpContext üzerinden kullanıcı ID'si okunamaz.
    // Bu yüzden önce aktif kullanıcıyı (varsa) almaya çalışıyoruz.
    var userId = _userRepo.GetCurrentUserId();

    // 3️⃣ Eğer arkada çalışan bir HTTP isteği yoksa (userId boş dönüyorsa), 
    // sistemin bu işlemi kendi başına (sistem kullanıcısı / admin adıyla) yapabilmesi için 
    // appsettings.json içerisindeki sabit Admin ID'yi fallback olarak alıyoruz.
    if (userId == Guid.Empty) userId = Guid.Parse(_config["SeedData:AdminUserId"]!);

    // 4️⃣ 24 saatlik depozito süresi dolan rezervasyonları bulmak için yazdığımız Specification kuralını çağırıyoruz.
    var spec = new ExpiredDepositLocksSpecification();

    // 5️⃣ Specification kuralına uyan rezervasyonları veritabanından async olarak çekiyoruz.
    var expiredReservations = await _reservationRepo.GetAsync(spec, _token);

    // 6️⃣ Eğer süresi dolan hiç rezervasyon yoksa, gereksiz yere döngüye girmeden direkt metoddan çıkıyoruz (Early Return).
    if (!expiredReservations.Any()) return;

    // 7️⃣ Süresi dolan kayıt bulundu, süreci logluyoruz.
    _logger.LogInformation("⌛ {Count} adet 24 saatlik süresi dolan rezervasyon bulundu. İptal ediliyor...", expiredReservations.Count);

    // 8️⃣ Bulunan her bir süresi geçmiş rezervasyon üzerinde tek tek dönüyoruz.
    foreach (var reservation in expiredReservations)
    {
      // 9️⃣ Rezervasyonun durumunu Domain katmanındaki iş kuralına göre 'İptal Edildi' olarak değiştiriyoruz (Audit için userId veriyoruz).
      reservation.MarkAsCancelledAfterDeposit(userId);

      // 10 Rezervasyondaki bu değişikliği repository'nin ChangeTracker'ına işaretliyoruz.
      _reservationRepo.Update(reservation);

      // 11 Bu rezervasyona bağlı olan aracı veritabanından buluyoruz.
      var vehicle = await _vehicleRepo.GetByIdAsync(reservation.VehicleId, _token);

      // 12 Eğer araç bulunduysa ve hâlâ 'Kiralandı (Rented)' durumundaysa...
      if (vehicle != null && vehicle.Status == VehicleStatus.Rented)
      {
        // 13 Aracı tekrar boşa çıkarıp (Return) müsait/stok durumuna döndürüyoruz.
        vehicle.Return(vehicle.CurrentKilometer);

        // 14 Araç üzerindeki bu değişikliği de repository'ye bildiriyoruz.
        _vehicleRepo.Update(vehicle);
      }
    }

    // 15 Döngü bitti; yapılan tüm değişiklikleri (rezervasyon iptalleri ve araç güncellemeleri) 
    // Unit of Work kullanarak tek bir veritabanı turunda (Transaction bütünlüğüyle) kalıcı hale getiriyoruz.
    await _unit.SaveChangesAsync(_token);

    // 16 İşlemin başarıyla tamamlandığını logluyoruz.
    _logger.LogInformation("✅ 24 saati dolan rezervasyonlar iptal edildi ve araçlar stoğa döndü.");
  }
}