namespace Application.Features.Vehicles.Dto;

public sealed record VehicleStatisticsDto(
    int TotalVehicleCount,              // Toplam Araç
    int AvailableVehicleCount,          // Müsait (Kiralanabilir)
    int RentedVehicleCount,             // Kirada
    int MaintenanceVehicleCount,        // 🆕 Bakımda
    int OutOfServiceVehicleCount,       // 🆕 Serviste / Arızalı
    int PassiveVehicleCount,            // Pasif (Silinmemiş ama kullanım dışı)
    decimal AverageDailyPrice           // Ortalama Günlük Fiyat
);