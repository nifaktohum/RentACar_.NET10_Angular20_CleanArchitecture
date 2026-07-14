namespace Application.Features.ProtectionPackages.Dto;

/// Bir koruma paketine ait fiyatlandırma ve muafiyet detaylarını 
/// tarih aralığı bazında yönetmek için kullanılan veri transfer nesnesi.
public sealed record ProtectionPricingDto(
    Guid Id,                         // Fiyat kaydının benzersiz kimliği
    Guid ProtectionPackageId,        // İlgili olduğu paket ID'si
    decimal? DailyPrice,             // Günlük fiyat (Örn: 166.00)
    decimal? DeductibleAmount,       // Muafiyet bedeli (Örn: 11500.00)
    bool IsDefault,                  // Varsayılan fiyatlandırma mı?
    DateTimeOffset ValidityStart,    // Geçerlilik başlangıç tarihi
    DateTimeOffset? ValidityEnd,     // Geçerlilik bitiş tarihi (Opsiyonel)
    bool IsCurrentlyValid,           // Audit: Şu an aktif/geçerli mi? (Hesaplanmış alan)
    bool IsActive,                   // Kaydın sistemde silinmemiş (soft-delete) durumu
    DateTimeOffset? CreatedAt,        // Oluşturulma tarihi
    Guid? CreatedBy,                  // Oluşturanın ID'si
    string? CreatedByName,            // Audit: Oluşturanın görünen adı
    DateTimeOffset? UpdatedAt,       // Son güncelleme tarihi
    Guid? UpdatedBy,                 // Güncelleyenin ID'si
    string? UpdatedByName            // Audit: Güncelleyenin görünen adı
);

