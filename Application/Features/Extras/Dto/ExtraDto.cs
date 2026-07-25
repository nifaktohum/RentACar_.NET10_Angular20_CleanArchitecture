namespace Application.Features.Extras.Dto;

// Tüm bilgileri içerir(Response)
public sealed record ExtraDto(
    Guid Id,
    string Name,
    string? Description,
    string? Icon,
    decimal Price,
    string PriceType,        // "Daily" veya "Rental"
    string Category,         // "Guarantee", "Driver", "Seat", "Other"
    int DisplayOrder,
    bool IsRecommended,
    int? MinAge,
    string? AgeRange,
    int? StockLimit,
    bool IsActive,
    DateTimeOffset? CreatedAt,        // Oluşturulma tarihi
    Guid? CreatedBy,                  // Oluşturanın ID'si
    string? CreatedByName,            // Audit: Oluşturanın görünen adı
    DateTimeOffset? UpdatedAt,       // Son güncelleme tarihi
    Guid? UpdatedBy,                 // Güncelleyenin ID'si
    string? UpdatedByName
);

/* 🎯 HANGİ DTO NEREDE KULLANILIR?


İşlem	                    Command/Query	                Kullanılan DTO

Tümünü Listele	          GetAllExtrasQuery	            ExtraProductSummaryDto[]
ID'ye Göre Getir	        GetExtraByIdQuery	            ExtraProductDto
Kategoriye Göre Listele	  GetExtrasByCategoryQuery	    ExtraProductSummaryDto[]
Yeni Oluştur	            CreateExtraCommand	          Request: CreateExtraProductDto → Response: ExtraProductDto
Güncelle	                UpdateExtraCommand	          Request: UpdateExtraProductDto → Response: ExtraProductDto
Sil	                      DeleteExtraCommand	          bool
Kiralama'ya Extra Ekle	  AddExtraToRentalCommand	      Request: CreateRentalExtraDto → Response: RentalExtraDto
Fiyat Hesapla	            CalculateExtraPriceQuery	    Response: ExtraPriceCalculationDto
*/