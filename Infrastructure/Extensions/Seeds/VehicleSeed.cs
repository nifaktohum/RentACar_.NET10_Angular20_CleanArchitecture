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

      var vehicleModels = new List<VehicleModel>
            {
                // Sedan
                new(
                    name: "C200",
                    brand: "Mercedes-Benz",
                    description: "Lüks sedan, konforlu sürüş",
                    stock: 5,
                    vehicleTypeId: sedan.Id,
                    createdBy: systemUserId
                ),
                new(
                    name: "A4",
                    brand: "Audi",
                    description: "Premium sedan, sportif",
                    stock: 8,
                    vehicleTypeId: sedan.Id,
                    createdBy: systemUserId
                ),
                new(
                    name: "3 Serisi",
                    brand: "BMW",
                    description: "Sportif sedan",
                    stock: 6,
                    vehicleTypeId: sedan.Id,
                    createdBy: systemUserId
                ),
                
                // SUV
                new(
                    name: "X5",
                    brand: "BMW",
                    description: "Lüks SUV",
                    stock: 10,
                    vehicleTypeId: suv.Id,
                    createdBy: systemUserId
                ),
                new(
                    name: "XC90",
                    brand: "Volvo",
                    description: "Premium SUV, güvenlik odaklı",
                    stock: 7,
                    vehicleTypeId: suv.Id,
                    createdBy: systemUserId
                ),
                new(
                    name: "Q5",
                    brand: "Audi",
                    description: "Kompakt lüks SUV",
                    stock: 9,
                    vehicleTypeId: suv.Id,
                    createdBy: systemUserId
                ),
                new(
                    name: "GLC",
                    brand: "Mercedes-Benz",
                    description: "Lüks kompakt SUV",
                    stock: 6,
                    vehicleTypeId: suv.Id,
                    createdBy: systemUserId
                ),
                
                // Hatchback
                new(
                    name: "Golf",
                    brand: "Volkswagen",
                    description: "Popüler hatchback",
                    stock: 12,
                    vehicleTypeId: hatchback.Id,
                    createdBy: systemUserId
                ),
                new(
                    name: "Clio",
                    brand: "Renault",
                    description: "Kompakt hatchback",
                    stock: 15,
                    vehicleTypeId: hatchback.Id,
                    createdBy: systemUserId
                ),
                new(
                    name: "A3 Sportback",
                    brand: "Audi",
                    description: "Premium hatchback",
                    stock: 5,
                    vehicleTypeId: hatchback.Id,
                    createdBy: systemUserId
                ),
                
                // Luxury
                new(
                    name: "S Serisi",
                    brand: "Mercedes-Benz",
                    description: "Üst segment lüks sedan",
                    stock: 3,
                    vehicleTypeId: luxury.Id,
                    createdBy: systemUserId
                ),
                new(
                    name: "7 Serisi",
                    brand: "BMW",
                    description: "Lüks premium sedan",
                    stock: 4,
                    vehicleTypeId: luxury.Id,
                    createdBy: systemUserId
                ),
                
                // Convertible
                new(
                    name: "Z4",
                    brand: "BMW",
                    description: "Spor roadster",
                    stock: 2,
                    vehicleTypeId: convertible.Id,
                    createdBy: systemUserId
                ),
                new(
                    name: "SLK",
                    brand: "Mercedes-Benz",
                    description: "Komakt lüks roadster",
                    stock: 3,
                    vehicleTypeId: convertible.Id,
                    createdBy: systemUserId
                )
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

      var vehicles = new List<Vehicle>
            {
                // BMW X5
                new(
                    brand: "BMW",
                    model: "X5",
                    year: "2023",
                    plate: "34ABC123",
                    color: "Siyah",
                    vehicleModelId: bmwX5.Id,
                    fuelType: "Dizel",
                    transmission: "Otomatik",
                    seatCount: 5,
                    doorCount: 4,
                    minAge: 25,
                    dailyPrice: 1500,
                    description: "Lüks SUV, full aksesuar",
                    createdBy: systemUserId,
                    isActive: true
                ),
                new(
                    brand: "BMW",
                    model: "X5",
                    year: "2024",
                    plate: "34ABC124",
                    color: "Beyaz",
                    vehicleModelId: bmwX5.Id,
                    fuelType: "Dizel",
                    transmission: "Otomatik",
                    seatCount: 5,
                    doorCount: 4,
                    minAge: 25,
                    dailyPrice: 1600,
                    description: "Lüks SUV, yeni model",
                    createdBy: systemUserId,
                    isActive: true
                ),
                new(
                    brand: "BMW",
                    model: "X5",
                    year: "2023",
                    plate: "34ABC125",
                    color: "Gri",
                    vehicleModelId: bmwX5.Id,
                    fuelType: "Benzin",
                    transmission: "Otomatik",
                    seatCount: 5,
                    doorCount: 4,
                    minAge: 25,
                    dailyPrice: 1400,
                    description: "Lüks SUV, spor paket",
                    createdBy: systemUserId,
                    isActive: true
                ),

                // Volvo XC90
                new(
                    brand: "Volvo",
                    model: "XC90",
                    year: "2024",
                    plate: "34MNO345",
                    color: "Siyah",
                    vehicleModelId: volvoXC90.Id,
                    fuelType: "Dizel",
                    transmission: "Otomatik",
                    seatCount: 7,
                    doorCount: 4,
                    minAge: 25,
                    dailyPrice: 1800,
                    description: "Premium SUV, 7 kişilik",
                    createdBy: systemUserId,
                    isActive: true
                ),

                // Mercedes C200
                new(
                    brand: "Mercedes-Benz",
                    model: "C200",
                    year: "2024",
                    plate: "34XYZ789",
                    color: "Beyaz",
                    vehicleModelId: mercedesC200.Id,
                    fuelType: "Benzin",
                    transmission: "Otomatik",
                    seatCount: 5,
                    doorCount: 4,
                    minAge: 25,
                    dailyPrice: 2250,
                    description: "Lüks sedan, AMG paket",
                    createdBy: systemUserId,
                    isActive: true
                ),

                // Audi A4
                new(
                    brand: "Audi",
                    model: "A4",
                    year: "2024",
                    plate: "35DEF456",
                    color: "Gri",
                    vehicleModelId: audiA4.Id,
                    fuelType: "Dizel",
                    transmission: "Otomatik",
                    seatCount: 5,
                    doorCount: 4,
                    minAge: 25,
                    dailyPrice: 2000,
                    description: "Premium sedan, S Line",
                    createdBy: systemUserId,
                    isActive: true
                ),

                // Volkswagen Golf
                new(
                    brand: "Volkswagen",
                    model: "Golf",
                    year: "2023",
                    plate: "07GH1789",
                    color: "Kırmızı",
                    vehicleModelId: vwGolf.Id,
                    fuelType: "Benzin",
                    transmission: "Manuel",
                    seatCount: 5,
                    doorCount: 4,
                    minAge: 21,
                    dailyPrice: 800,
                    description: "Kompakt hatchback, sportif",
                    createdBy: systemUserId,
                    isActive: true
                ),

                // Renault Clio
                new(
                    brand: "Renault",
                    model: "Clio",
                    year: "2023",
                    plate: "16JKL012",
                    color: "Mavi",
                    vehicleModelId: renaultClio.Id,
                    fuelType: "Hibrit",
                    transmission: "Otomatik",
                    seatCount: 5,
                    doorCount: 4,
                    minAge: 21,
                    dailyPrice: 850,
                    description: "Ekonomik hatchback",
                    createdBy: systemUserId,
                    isActive: true
                )
            };

      await _context.Vehicles.AddRangeAsync(vehicles);
      await _context.SaveChangesAsync();
    }
  }
}