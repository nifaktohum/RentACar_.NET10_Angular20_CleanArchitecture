namespace Application.Features.Reservations.Dto.Responses;

// listelenen rezervasyonların hafif, performanslı ve öz versiyonudur.
public sealed record ReservationDto(
    Guid Id,                                  // Rezervasyonun sistemdeki benzersiz ID'si
    string ReservationNumber,                 // Kullanıcıya ve operasyona gösterilen okunabilir rezervasyon numarası (Örn: RES-2026-00042)
    string CustomerFullName,                  // Rezervasyonu yapan müşterinin ad ve soyad bilgisi
    string VehicleInfo,                       // Aracın marka, model ve plaka bilgisini tek satırda birleştiren özet alan (Örn: "BMW X5 - 34 ABC 123")
    DateTimeOffset PlannedStartDate,          // Kiralamanın planlanan başlangıç tarihi ve saati
    DateTimeOffset PlannedEndDate,            // Kiralamanın planlanan bitiş tarihi ve saati
    int PlannedDays,                          // Toplam planlanan kiralama gün sayısı
    string Status,                            // Rezervasyonun anlık durumu (Pending, Confirmed, Active, Completed, Cancelled vb.)
    decimal TotalPrice,                       // Kiralamanın toplam tutarı
    decimal DepositAmount,                    // Alınması gereken kapora / güvence bedeli tutarı
    bool DepositPaid,                         // Kaporanın ödenip ödenmediğini belirten boolean bayrak (True/False)
    DateTimeOffset CreatedAt                  // Rezervasyon kaydının ilk oluşturulduğu zaman damgası
);
