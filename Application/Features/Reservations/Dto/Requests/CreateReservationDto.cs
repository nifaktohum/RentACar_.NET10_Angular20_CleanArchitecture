namespace Application.Features.Reservations.Dto.Requests;

public sealed record CreateReservationDto(
                          Guid CustomerId,                    // Rezervasyonu yapan müşterinin sistemdeki benzersiz ID'si
                          Guid VehicleId,                     // Kiralanmak istenen aracın sistemdeki benzersiz ID'si
                          Guid PickupBranchId,                // Aracın teslim alınacağı (başlangıç) şubenin ID'si
                          Guid? ReturnBranchId,               // Aracın bırakılacağı şubenin ID'si (Eğer aynı şube olacaksa null geçilebilir)
                          DateTimeOffset PlannedStartDate,    // Kiralamanın planlanan başlangıç tarihi ve saat bilgisi (Timezone destekli)
                          DateTimeOffset PlannedEndDate,      // Kiralamanın planlanan bitiş tarihi ve saat bilgisi (Timezone destekli)
                          string? Notes,                      // Müşterinin eklemek istediği özel notlar veya talepler (Opsiyonel)
                          List<CreateReservationExtraDto> Extras  // Kiralama ile birlikte seçilen ekstra hizmetlerin (bebek koltuğu, sigorta vb.) listesi
                      );
