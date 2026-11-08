using Domain.Entities.Vehicles;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Extensions.Seeds;

public static class VehicleSeed
{
  public static async Task VehicleSeedAsync(this AppDbContext _context, IConfiguration _config)
  {
    // Admin User ID'yi config'den al
    var systemUserId = Guid.Parse(_config["SeedData:AdminUserId"]!);

    // 🆕 Şube ID'si (Örnek bir Guid, Branch tablon varsa oradan çekmelisin)
    var defaultBranchId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    // ==================== 1. VEHICLE TYPES ====================
    if (!await _context.VehicleTypes.AnyAsync())
    {
      var vehicleTypes = new List<VehicleType>
            {
                new(name: "Sedan", description: "Binek araç, 4 kapılı, konforlu", icon: "ri-car-line", displayOrder: 1, createdBy: systemUserId),
                new(name: "Hatchback", description: "Küçük sınıf araç, 5 kapılı", icon: "ri-car-line", displayOrder: 2, createdBy: systemUserId),
                new(name: "SUV", description: "Sport Utility Vehicle, geniş ve yüksek", icon: "ri-suv-line", displayOrder: 3, createdBy: systemUserId),
                new(name: "Van", description: "Minivan / Panelvan, geniş iç hacim", icon: "ri-van-line", displayOrder: 4, createdBy: systemUserId),
                new(name: "Pickup", description: "Kamyonet, yük taşıma amaçlı", icon: "ri-truck-line", displayOrder: 5, createdBy: systemUserId),
                new(name: "Convertible", description: "Açılır tavan, spor araç", icon: "ri-convertible-line", displayOrder: 6, createdBy: systemUserId),
                new(name: "Luxury", description: "Lüks araç, üst segment", icon: "ri-car-fill", displayOrder: 7, createdBy: systemUserId),
                new(name: "Minivan", description: "Aile arabası, 7-8 kişilik", icon: "ri-van-line", displayOrder: 8, createdBy: systemUserId),
                new(name: "StationWagon", description: "Station Wagon, bagaj hacmi geniş", icon: "ri-car-wagon-line", displayOrder: 9, createdBy: systemUserId),
                new(name: "Coupe", description: "Coupe, 2 kapılı spor araç", icon: "ri-coupe-line", displayOrder: 10, createdBy: systemUserId)
            };

      await _context.VehicleTypes.AddRangeAsync(vehicleTypes);
      await _context.SaveChangesAsync();
    }

    // ==================== 2. VEHICLE MODELS ====================
    if (!await _context.VehicleModels.AnyAsync())
    {
      // VehicleType'ları getir
      var sedan = await _context.VehicleTypes.FirstAsync(vt => vt.Name == "Sedan");
      var suv = await _context.VehicleTypes.FirstAsync(vt => vt.Name == "SUV");
      var hatchback = await _context.VehicleTypes.FirstAsync(vt => vt.Name == "Hatchback");
      var luxury = await _context.VehicleTypes.FirstAsync(vt => vt.Name == "Luxury");
      var convertible = await _context.VehicleTypes.FirstAsync(vt => vt.Name == "Convertible");

      // ❌ stock parametresi ÇIKARILDI (Computed property oldu)
      var vehicleModels = new List<VehicleModel>
            {
                // Sedan
                new(name: "C200", brand: "Mercedes-Benz", description: "Lüks sedan, konforlu sürüş", vehicleTypeId: sedan.Id, createdBy: systemUserId),
                new(name: "A4", brand: "Audi", description: "Premium sedan, sportif", vehicleTypeId: sedan.Id, createdBy: systemUserId),
                new(name: "3 Serisi", brand: "BMW", description: "Sportif sedan", vehicleTypeId: sedan.Id, createdBy: systemUserId),
                
                // SUV
                new(name: "X5", brand: "BMW", description: "Lüks SUV", vehicleTypeId: suv.Id, createdBy: systemUserId),
                new(name: "XC90", brand: "Volvo", description: "Premium SUV, güvenlik odaklı", vehicleTypeId: suv.Id, createdBy: systemUserId),
                new(name: "Q5", brand: "Audi", description: "Kompakt lüks SUV", vehicleTypeId: suv.Id, createdBy: systemUserId),
                new(name: "GLC", brand: "Mercedes-Benz", description: "Lüks kompakt SUV", vehicleTypeId: suv.Id, createdBy: systemUserId),
                
                // Hatchback
                new(name: "Golf", brand: "Volkswagen", description: "Popüler hatchback", vehicleTypeId: hatchback.Id, createdBy: systemUserId),
                new(name: "Clio", brand: "Renault", description: "Kompakt hatchback", vehicleTypeId: hatchback.Id, createdBy: systemUserId),
                new(name: "A3 Sportback", brand: "Audi", description: "Premium hatchback", vehicleTypeId: hatchback.Id, createdBy: systemUserId),
                
                // Luxury
                new(name: "S Serisi", brand: "Mercedes-Benz", description: "Üst segment lüks sedan", vehicleTypeId: luxury.Id, createdBy: systemUserId),
                new(name: "7 Serisi", brand: "BMW", description: "Lüks premium sedan", vehicleTypeId: luxury.Id, createdBy: systemUserId),
                
                // Convertible
                new(name: "Z4", brand: "BMW", description: "Spor roadster", vehicleTypeId: convertible.Id, createdBy: systemUserId),
                new(name: "SLK", brand: "Mercedes-Benz", description: "Kompakt lüks roadster", vehicleTypeId: convertible.Id, createdBy: systemUserId)
            };

      await _context.VehicleModels.AddRangeAsync(vehicleModels);
      await _context.SaveChangesAsync();
    }

    // ==================== 3. VEHICLES (Sample Data) ====================
    if (!await _context.Vehicles.AnyAsync())
    {
      // VehicleModel'ları getir
      var bmwX5 = await _context.VehicleModels.FirstAsync(vm => vm.Brand == "BMW" && vm.Name == "X5");
      var volvoXC90 = await _context.VehicleModels.FirstAsync(vm => vm.Brand == "Volvo" && vm.Name == "XC90");
      var mercedesC200 = await _context.VehicleModels.FirstAsync(vm => vm.Brand == "Mercedes-Benz" && vm.Name == "C200");
      var audiA4 = await _context.VehicleModels.FirstAsync(vm => vm.Brand == "Audi" && vm.Name == "A4");
      var vwGolf = await _context.VehicleModels.FirstAsync(vm => vm.Brand == "Volkswagen" && vm.Name == "Golf");
      var renaultClio = await _context.VehicleModels.FirstAsync(vm => vm.Brand == "Renault" && vm.Name == "Clio");

      var currentDate = DateTimeOffset.UtcNow;
      var nextYear = currentDate.AddYears(1);

      // 🆕 TÜM YENİ ALANLAR EKLENDİ
      var vehicles = new List<Vehicle>
            {
                // BMW X5 - 1
                new(
                    brand: "BMW", model: "X5", year: "2023", plate: "34ABC123", color: "Siyah",
                    vehicleModelId: bmwX5.Id, branchId: defaultBranchId,
                    vinNumber: "WBA1234567890X5A1", engineNumber: "ENG-X5-001",
                    fuelType: "Dizel", transmission: "Otomatik", tractionType: "AWD",
                    engineVolume: 3.0m, enginePower: 286, fuelConsumption: 7.5m,
                    seatCount: 5, doorCount: 4, minAge: 25,
                    dailyPrice: 1500, depositAmount: 5000, weeklyDiscountRate: 10, monthlyDiscountRate: 20,
                    currentKilometer: 45000, lastMaintenanceKm: 40000, nextMaintenanceKm: 50000,
                    lastMaintenanceDate: currentDate.AddMonths(-2),
                    inspectionDate: nextYear, insuranceEndDate: nextYear, cascoEndDate: nextYear,
                    tireStatus: "İyi", generalStatus: "Mükemmel",
                    description: "Lüks SUV, full aksesuar", createdBy: systemUserId, isActive: true
                ),
                // BMW X5 - 2
                new(
                    brand: "BMW", model: "X5", year: "2024", plate: "34ABC124", color: "Beyaz",
                    vehicleModelId: bmwX5.Id, branchId: defaultBranchId,
                    vinNumber: "WBA1234567890X5A2", engineNumber: "ENG-X5-002",
                    fuelType: "Dizel", transmission: "Otomatik", tractionType: "AWD",
                    engineVolume: 3.0m, enginePower: 286, fuelConsumption: 7.5m,
                    seatCount: 5, doorCount: 4, minAge: 25,
                    dailyPrice: 1600, depositAmount: 5000, weeklyDiscountRate: 10, monthlyDiscountRate: 20,
                    currentKilometer: 15000, lastMaintenanceKm: 10000, nextMaintenanceKm: 20000,
                    lastMaintenanceDate: currentDate.AddMonths(-1),
                    inspectionDate: nextYear, insuranceEndDate: nextYear, cascoEndDate: nextYear,
                    tireStatus: "Mükemmel", generalStatus: "Mükemmel",
                    description: "Lüks SUV, yeni model", createdBy: systemUserId, isActive: true
                ),
                // BMW X5 - 3
                new(
                    brand: "BMW", model: "X5", year: "2023", plate: "34ABC125", color: "Gri",
                    vehicleModelId: bmwX5.Id, branchId: defaultBranchId,
                    vinNumber: "WBA1234567890X5A3", engineNumber: "ENG-X5-003",
                    fuelType: "Benzin", transmission: "Otomatik", tractionType: "AWD",
                    engineVolume: 3.0m, enginePower: 340, fuelConsumption: 9.0m,
                    seatCount: 5, doorCount: 4, minAge: 25,
                    dailyPrice: 1400, depositAmount: 5000, weeklyDiscountRate: 10, monthlyDiscountRate: 20,
                    currentKilometer: 60000, lastMaintenanceKm: 55000, nextMaintenanceKm: 65000,
                    lastMaintenanceDate: currentDate.AddMonths(-3),
                    inspectionDate: nextYear, insuranceEndDate: nextYear, cascoEndDate: nextYear,
                    tireStatus: "Orta", generalStatus: "İyi",
                    description: "Lüks SUV, spor paket", createdBy: systemUserId, isActive: true
                ),

                // Volvo XC90
                new(
                    brand: "Volvo", model: "XC90", year: "2024", plate: "34MNO345", color: "Siyah",
                    vehicleModelId: volvoXC90.Id, branchId: defaultBranchId,
                    vinNumber: "YV1234567890XC90A", engineNumber: "ENG-XC90-001",
                    fuelType: "Dizel", transmission: "Otomatik", tractionType: "AWD",
                    engineVolume: 2.0m, enginePower: 235, fuelConsumption: 6.8m,
                    seatCount: 7, doorCount: 4, minAge: 25,
                    dailyPrice: 1800, depositAmount: 6000, weeklyDiscountRate: 10, monthlyDiscountRate: 20,
                    currentKilometer: 25000, lastMaintenanceKm: 20000, nextMaintenanceKm: 30000,
                    lastMaintenanceDate: currentDate.AddMonths(-1),
                    inspectionDate: nextYear, insuranceEndDate: nextYear, cascoEndDate: nextYear,
                    tireStatus: "Mükemmel", generalStatus: "Mükemmel",
                    description: "Premium SUV, 7 kişilik", createdBy: systemUserId, isActive: true
                ),

                // Mercedes C200
                new(
                    brand: "Mercedes-Benz", model: "C200", year: "2024", plate: "34XYZ789", color: "Beyaz",
                    vehicleModelId: mercedesC200.Id, branchId: defaultBranchId,
                    vinNumber: "WDD1234567890C200", engineNumber: "ENG-C200-001",
                    fuelType: "Benzin", transmission: "Otomatik", tractionType: "RWD",
                    engineVolume: 1.5m, enginePower: 204, fuelConsumption: 6.5m,
                    seatCount: 5, doorCount: 4, minAge: 25,
                    dailyPrice: 2250, depositAmount: 7000, weeklyDiscountRate: 10, monthlyDiscountRate: 20,
                    currentKilometer: 10000, lastMaintenanceKm: 5000, nextMaintenanceKm: 15000,
                    lastMaintenanceDate: currentDate.AddMonths(-1),
                    inspectionDate: nextYear, insuranceEndDate: nextYear, cascoEndDate: nextYear,
                    tireStatus: "Mükemmel", generalStatus: "Mükemmel",
                    description: "Lüks sedan, AMG paket", createdBy: systemUserId, isActive: true
                ),

                // Audi A4
                new(
                    brand: "Audi", model: "A4", year: "2024", plate: "35DEF456", color: "Gri",
                    vehicleModelId: audiA4.Id, branchId: defaultBranchId,
                    vinNumber: "WAU1234567890A4AA", engineNumber: "ENG-A4-001",
                    fuelType: "Dizel", transmission: "Otomatik", tractionType: "FWD",
                    engineVolume: 2.0m, enginePower: 163, fuelConsumption: 5.5m,
                    seatCount: 5, doorCount: 4, minAge: 25,
                    dailyPrice: 2000, depositAmount: 6000, weeklyDiscountRate: 10, monthlyDiscountRate: 20,
                    currentKilometer: 30000, lastMaintenanceKm: 25000, nextMaintenanceKm: 35000,
                    lastMaintenanceDate: currentDate.AddMonths(-2),
                    inspectionDate: nextYear, insuranceEndDate: nextYear, cascoEndDate: nextYear,
                    tireStatus: "İyi", generalStatus: "İyi",
                    description: "Premium sedan, S Line", createdBy: systemUserId, isActive: true
                ),

                // Volkswagen Golf
                new(
                    brand: "Volkswagen", model: "Golf", year: "2023", plate: "07GH1789", color: "Kırmızı",
                    vehicleModelId: vwGolf.Id, branchId: defaultBranchId,
                    vinNumber: "WVW1234567890GOLF", engineNumber: "ENG-GOLF-001",
                    fuelType: "Benzin", transmission: "Manuel", tractionType: "FWD",
                    engineVolume: 1.4m, enginePower: 150, fuelConsumption: 5.8m,
                    seatCount: 5, doorCount: 4, minAge: 21,
                    dailyPrice: 800, depositAmount: 3000, weeklyDiscountRate: 5, monthlyDiscountRate: 15,
                    currentKilometer: 80000, lastMaintenanceKm: 75000, nextMaintenanceKm: 85000,
                    lastMaintenanceDate: currentDate.AddMonths(-3),
                    inspectionDate: nextYear, insuranceEndDate: nextYear, cascoEndDate: nextYear,
                    tireStatus: "Orta", generalStatus: "İyi",
                    description: "Kompakt hatchback, sportif", createdBy: systemUserId, isActive: true
                ),

                // Renault Clio
                new(
                    brand: "Renault", model: "Clio", year: "2023", plate: "16JKL012", color: "Mavi",
                    vehicleModelId: renaultClio.Id, branchId: defaultBranchId,
                    vinNumber: "VF11234567890CLIO", engineNumber: "ENG-CLIO-001",
                    fuelType: "Hibrit", transmission: "Otomatik", tractionType: "FWD",
                    engineVolume: 1.6m, enginePower: 140, fuelConsumption: 4.2m,
                    seatCount: 5, doorCount: 4, minAge: 21,
                    dailyPrice: 850, depositAmount: 3000, weeklyDiscountRate: 5, monthlyDiscountRate: 15,
                    currentKilometer: 40000, lastMaintenanceKm: 35000, nextMaintenanceKm: 45000,
                    lastMaintenanceDate: currentDate.AddMonths(-2),
                    inspectionDate: nextYear, insuranceEndDate: nextYear, cascoEndDate: nextYear,
                    tireStatus: "İyi", generalStatus: "Mükemmel",
                    description: "Ekonomik hatchback", createdBy: systemUserId, isActive: true
                )
            };

      await _context.Vehicles.AddRangeAsync(vehicles);
      await _context.SaveChangesAsync();
    }
  }
}