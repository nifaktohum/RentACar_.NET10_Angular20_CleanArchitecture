using Domain.Protection;

namespace Application.Features.ProtectionPackages.Dto;

public sealed record ProtectionPackageDto(
    Guid Id,                                         // Paketin benzersiz kimlik bilgisi
    string Name,                                     // Paket adı (Örn: "Gold Güvence")
    string? Description,                             // Paket açıklaması
    string? Icon,                                    // Arayüz ikon kodu
    int DisplayOrder,                                // Sıralama indeksi
    bool IsRecommended,                              // "Önerilen" etiketi durumu
    int StarRating,                                  // Paket puanı (1-5)
    ProtectionLevel ProtectionLevel,                 // Koruma seviyesi (Enum)
    DeductibleType DeductibleType,                   // Muafiyet türü (Enum)
    IReadOnlyCollection<ProtectionBenefitDto> Benefits, // Pakete bağlı güvence kapsamları
    IReadOnlyCollection<ProtectionPricingDto> Pricing,  // Paketin fiyatlandırma geçmişi/dönemleri
    bool IsActive,                                   // Kaydın aktiflik durumu
    DateTimeOffset? CreatedAt,                        // Oluşturulma tarihi
    Guid? CreatedBy,                                  // Oluşturan kullanıcının ID'si
    string? CreatedByName,                            // Oluşturan kullanıcının görünen adı
    DateTimeOffset? UpdatedAt,                       // Son güncellenme tarihi
    Guid? UpdatedBy,                                 // Güncelleyen kullanıcının ID'si
    string? UpdatedByName                            // Güncelleyen kullanıcının görünen adı
);