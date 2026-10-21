namespace Application.Features.Reservations.Dto.Responses;

public sealed record ReservationDetailDto(
    Guid Id,                        // Rezervasyonun sistemdeki benzersiz ID'si
    string ReservationNumber,       // Rezervasyon numarası (Örn: RES-2026-00042)

    // Müşteri
    Guid CustomerId,                // Rezervasyonu yapan müşterinin ID'si
    string CustomerFullName,        // Müşterinin ad soyad bilgisi
    string CustomerPhone,           // Müşterinin telefon numarası
    string CustomerEmail,           // Müşterinin e-posta adresi

    // Araç
    Guid VehicleId,                 // Kiralanan aracın ID'si
    string VehicleBrand,            // Aracın markası (Örn: Renault, BMW)
    string VehicleModel,            // Aracın modeli (Örn: Clio, 3.20)
    string VehiclePlate,            // Aracın plakası

    // Şubeler
    string PickupBranchName,        // Aracın teslim alınacağı şubenin adı
    string? ReturnBranchName,       // Aracın bırakılacağı şubenin adı (Eğer aynıysa null gelebilir)

    // Tarihler
    DateTimeOffset PlannedStartDate,  // Planlanan başlangıç tarihi
    DateTimeOffset PlannedEndDate,    // Planlanan bitiş tarihi
    int PlannedDays,                  // Toplam planlanan kiralama gün sayısı
    DateTimeOffset? ActualPickupAt,   // Aracın fiilen teslim alındığı tarih (Süreç başladığında dolar)
    DateTimeOffset? ActualReturnAt,   // Aracın fiilen teslim edildiği/iade edildiği tarih (Süreç kapandığında dolar)

    // Fiyatlandırma
    decimal DailyPrice,               // Aracın günlük kiralama fiyatı
    decimal TotalPrice,               // Kiralamanın toplam tutarı (Araç + Ekstralar vb.)
    decimal? BillablePrice,           // Faturalandırılacak net tutar
    decimal DepositAmount,            // Alınması gereken kapora / güvence bedeli tutarı
    decimal RemainingAmount,          // Ödenmesi gereken kalan tutar
    bool DepositPaid,                 // Kapora ödenip ödenmediğinin boolean bayrağı
    DateTimeOffset? DepositPaidAt,    // Kaporanın ödendiği zaman damgası
    string? PaymentId,                // Sanal POS ödeme işlem referans numarası

    // Durum ve Süreler
    string Status,                    // Rezervasyonun güncel statüsü (Pending, Confirmed, Active, Completed, Cancelled vb.)
    DateTimeOffset PaymentDeadline,   // Ödemenin yapılması gereken son süre sınırı
    DateTimeOffset? PickupDeadline,   // Müşterinin aracı teslim almak için şubeye gelmesi gereken son süre sınırı
    DateTimeOffset? ConfirmedAt,      // Rezervasyonun onaylandığı zaman
    DateTimeOffset? CompletedAt,      // Rezervasyonun başarıyla tamamlandığı zaman
    DateTimeOffset? CancelledAt,      // Rezervasyonun iptal edildiği zaman
    string? CancellationReason,       // İptal edildiyse girilen sebep açıklaması

    // Extralar
    List<RentalExtraDto> Extras,      // Bu rezervasyona eklenen tüm ekstra kalemlerin detay listesi (Bebek koltuğu, GPS vb.)
    decimal ExtrasTotalPrice,         // Seçilen tüm ekstraların toplam tutarı

    // Notlar
    string? Notes,                    // Müşterinin oluştururken bıraktığı özel notlar
    string? AdminNotes,               // Sadece personelin/adminin görebildiği dahili notlar

    DateTimeOffset CreatedAt          // Kaydın ilk oluşturulduğu zaman damgası
);