namespace Application.Features.Vehicles.Dto;

public sealed record UpdateVehicleDto(
    // --- Kimlik ---
    Guid Id,

    // --- Temel Bilgiler ---
    string Brand,
    string Model,
    string Year,
    string Plate,
    string Color,
    Guid VehicleModelId,
    string? VehicleModelName,              // 🆕 Detaylı bilgi dönüşü için faydalı
    string? VehicleTypeName,               // 🆕
    Guid BranchId,                         // 🆕 Şube ID'si

    // --- Teknik & Tanımlayıcı Bilgiler ---
    string VinNumber,                      // 🆕 Şasi Numarası
    string EngineNumber,                   // 🆕 Motor Numarası
    string FuelType,
    string Transmission,
    string TractionType,                   // 🆕 Çekiş Tipi
    decimal EngineVolume,                  // 🆕 Motor Hacmi
    int EnginePower,                       // 🆕 Motor Gücü
    decimal FuelConsumption,               // 🆕 Yakıt Tüketimi
    int SeatCount,
    int DoorCount,

    // --- Ticari & Yasal Bilgiler ---
    int? MinAge,
    decimal DailyPrice,
    decimal DepositAmount,                 // 🆕 Depozito
    decimal WeeklyDiscountRate,            // 🆕 Haftalık İndirim
    decimal MonthlyDiscountRate,           // 🆕 Aylık İndirim

    // --- Bakım & Durum Bilgileri ---
    int CurrentKilometer,                  // 🆕 Güncel KM
    int LastMaintenanceKm,                 // 🆕 Son Bakım KM
    int NextMaintenanceKm,                 // 🆕 Sonraki Bakım KM
    DateTimeOffset LastMaintenanceDate,    // 🆕 Son Bakım Tarihi
    DateTimeOffset InspectionDate,         // 🆕 Muayene Tarihi
    DateTimeOffset InsuranceEndDate,       // 🆕 Sigorta Bitiş
    DateTimeOffset CascoEndDate,           // 🆕 Kasko Bitiş
    string TireStatus,                     // 🆕 Lastik Durumu
    string GeneralStatus,                  // 🆕 Genel Durum

    // --- Sistem & Durum ---
    bool IsAvailable,
    string Status,                         // 🆕 Anlık operasyonel durum (Available, Rented vb.)
    bool IsActive,                         // Silinmiş mi / Aktif mi?
    string? Description,

    // --- Görseller ---
    string? MainImageUrl,                  // 🆕 ImageUrl yerine MainImageUrl
    List<VehicleImageDto> Images,          // 🆕 Tüm görselleri gösterelim

    // --- Audit Bilgileri ---
    DateTimeOffset? CreatedAt,
    Guid? CreatedBy,
    string? CreatedByName,
    DateTimeOffset? UpdatedAt,
    Guid? UpdatedBy,
    string? UpdatedByName
);