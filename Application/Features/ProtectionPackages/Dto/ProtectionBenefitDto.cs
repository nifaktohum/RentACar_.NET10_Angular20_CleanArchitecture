namespace Application.Features.ProtectionPackages.Dto;


/// Bir koruma kapsamının (örneğin: Lastik, Cam veya Mini Hasar) 
/// API üzerinden taşınması için kullanılan veri transfer nesnesi.
public sealed record ProtectionBenefitDto(
    Guid Id,                         // Kapsamın benzersiz ID'si
    string Name,                     // Kapsamın adı (örn: "Lastik Cam Far Güvencesi")
    string? Description,             // Kapsamın detaylı açıklaması
    string? Icon,                    // UI'da görüntülenecek ikonun referansı
    int DisplayOrder,                // Listeleme sırası
    string Category,                  // Kategori adı (örnek: "Lastik")
    Guid CategoryId,                  // Kategori ID'si
    bool IsActive,                   // Kaydın sistemdeki aktiflik durumu
    DateTimeOffset? CreatedAt,        // Kaydın oluşturulduğu zaman
    Guid? CreatedBy,                  // Kaydı oluşturan kullanıcının ID'si
    string? CreatedByName,            // Audit: Kaydı oluşturan kullanıcının görünen adı
    DateTimeOffset? UpdatedAt,       // Kaydın son güncellendiği zaman
    Guid? UpdatedBy,                 // Kaydı güncelleyen kullanıcının ID'si
    string? UpdatedByName            // Audit: Kaydı güncelleyen kullanıcının görünen adı
);
