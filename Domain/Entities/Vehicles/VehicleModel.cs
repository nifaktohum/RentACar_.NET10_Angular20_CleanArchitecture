using Domain.Abstractions;
using Domain.Exceptions;

namespace Domain.Entities.Vehicles;

public class VehicleModel: BaseEntity
{
  // ==================== PRIVATE CONSTRUCTOR (EF Core için) ====================
  // ==================== PRIVATE CONSTRUCTOR (EF Core için) ====================
  private VehicleModel() : base(Guid.Empty)
  {
    Name = string.Empty;
    Brand = string.Empty;
    Description = null;
    Vehicles = new List<Vehicle>();
  }

  // ==================== PUBLIC CONSTRUCTOR ====================
  public VehicleModel(
      string name,
      string brand,
      string? description,
      int stock,
      Guid vehicleTypeId,
      Guid createdBy
  ) : base(createdBy)
  {
    SetName(name);
    SetBrand(brand);
    SetDescription(description);
    SetStock(stock);

    VehicleTypeId = vehicleTypeId;
    VehicleType = null!;
    AvailableStock = stock;  // Başlangıçta tüm stok müsait
    Vehicles = new List<Vehicle>();
  }


  // ==================== PROPERTIES ====================
  public string Name { get; private set; } = string.Empty;
  public string Brand { get; private set; } = string.Empty;
  public string? Description { get; private set; }
  public int Stock { get; private set; }
  public int AvailableStock { get; private set; }
  public bool IsInStock => AvailableStock > 0; // "Stokta var/yok"

  // ==================== NAVIGATION PROPERTIES ====================
  public Guid VehicleTypeId { get; private set; }
  public VehicleType VehicleType { get; private set; } = null!;
  public ICollection<Vehicle> Vehicles { get; private set; }


  // ==================== DOMAIN METHODS ====================


  // Brand validasyonu ile set et
  public void SetBrand(string brand)
  {
    if (string.IsNullOrWhiteSpace(brand))
      throw new DomainValidationException("Marka adı boş olamaz!", nameof(brand));

    if (brand.Length > 50)
      throw new DomainValidationException("Marka adı 50 karakterden uzun olamaz!", nameof(brand));

    Brand = brand.Trim();
  }

  // Name validasyonu ile set et
  public void SetName(string name)
  {
    if (string.IsNullOrWhiteSpace(name))
      throw new DomainValidationException("Model adı boş olamaz!", nameof(name));

    if (name.Length > 50)
      throw new DomainValidationException("Model adı 50 karakterden uzun olamaz!", nameof(name));

    Name = name.Trim();
  }

  // Description set et
  public void SetDescription(string? description)
  {
    if (description?.Length > 500)
      throw new DomainValidationException("Açıklama 500 karakterden uzun olamaz!", nameof(description));

    Description = description?.Trim();
  }

  // Stok ekle (araç eklendiğinde)
  public void AddStock(int quantity = 1)
  {
    if (quantity <= 0)
      throw new DomainValidationException("Eklenecek stok miktarı 0'dan büyük olmalı!", nameof(quantity));

    Stock += quantity;
    AvailableStock += quantity;
  }

  // Stok güncelleme (toplam stok)
  public void SetStock(int newStock)
  {
    if (newStock < 0)
      throw new DomainValidationException("Stok değeri 0'dan küçük olamaz!", nameof(newStock));

    if (newStock < AvailableStock)
      throw new DomainValidationException(
          $"Yeni stok ({newStock}) mevcut müsait stoktan ({AvailableStock}) az olamaz!",
          nameof(newStock)
      );

    Stock = newStock;
    // AvailableStock zaten doğru, değiştirmeye gerek yok
  }

  // Stok azalt (araç kiralandığında veya satıldığında)
  public void RemoveStock(int quantity = 1)
  {
    if (quantity <= 0)
      throw new DomainValidationException("Azaltılacak stok miktarı 0'dan büyük olmalı!", nameof(quantity));

    if (AvailableStock < quantity)
      throw new DomainValidationException(
          $"Yetersiz stok! Mevcut: {AvailableStock}, İstenen: {quantity}",
          nameof(quantity)
      );

    AvailableStock -= quantity;
    Stock -= quantity;
  }

  // Araç kiralandığında (available stock azalır)
  public void RentVehicle(int quantity = 1)
  {
    if (quantity <= 0)
      throw new DomainValidationException("Kiralanacak araç sayısı 0'dan büyük olmalı!", nameof(quantity));

    if (AvailableStock < quantity)
      throw new InsufficientStockException(
          $"Yetersiz stok! Müsait: {AvailableStock}, İstenen: {quantity}"
      );

    AvailableStock -= quantity;
  }

  // Araç iade edildiğinde (available stock artar)
  public void ReturnVehicle(int quantity = 1)
  {
    if (quantity <= 0)
      throw new DomainValidationException("İade edilecek araç sayısı 0'dan büyük olmalı!", nameof(quantity));

    if (AvailableStock + quantity > Stock)
      throw new DomainValidationException(
          $"İade sonrası stok ({AvailableStock + quantity}) toplam stoğu ({Stock}) aşıyor!",
          nameof(quantity)
      );

    AvailableStock += quantity;
  }

  public void SetVehicleType(VehicleType vehicleType)  
  {
    if (vehicleType == null)
      throw new ArgumentNullException(nameof(vehicleType));

    VehicleType = vehicleType;
    VehicleTypeId = vehicleType.Id;
  }


  // Araç bakıma alındığında
  public void SendToMaintenance(int quantity = 1)
  {
    if (quantity <= 0)
      throw new DomainValidationException("Bakıma alınacak araç sayısı 0'dan büyük olmalı!", nameof(quantity));

    if (AvailableStock < quantity)
      throw new InsufficientStockException(
          $"Yetersiz stok! Müsait: {AvailableStock}, Bakıma alınacak: {quantity}"
      );

    AvailableStock -= quantity;
    // Stock değişmez, çünkü araç hala var ama müsait değil
  }

  // Araç bakımdan döndüğünde
  public void ReturnFromMaintenance(int quantity = 1)
  {
    if (quantity <= 0)
      throw new DomainValidationException("Bakımdan dönen araç sayısı 0'dan büyük olmalı!", nameof(quantity));

    if (AvailableStock + quantity > Stock)
      throw new DomainValidationException(
          $"Bakımdan dönüş sonrası stok ({AvailableStock + quantity}) toplam stoğu ({Stock}) aşıyor!",
          nameof(quantity)
      );

    AvailableStock += quantity;
  }

  // Toplam stok ve müsait stok oranını kontrol et
  public bool HasSufficientStock(int requestedQuantity)
  {
    return requestedQuantity > 0 && AvailableStock >= requestedQuantity;
  }

  // Stok durumunu raporla
  public string GetStockStatus()
  {
    if (Stock == 0)
      return "Stokta yok";

    if (AvailableStock == 0)
      return "Tüm araçlar kiralandı/bakımda";

    if (AvailableStock <= Stock * 0.2) // %20'den az kaldıysa
      return "Stok bitiyor!";

    return "Stokta mevcut";
  }

  // Stok yüzdesini hesapla
  public decimal GetAvailabilityPercentage()
  {
    if (Stock == 0)
      return 0;

    return Math.Round((decimal)AvailableStock / Stock * 100, 2);
  }


  // Vehicle ekle (relationship yönetimi)
  public void AddVehicle(Vehicle vehicle)
  {
    if (vehicle == null)
      throw new ArgumentNullException(nameof(vehicle));

    if (vehicle.VehicleModelId != Id)
      throw new DomainValidationException("Araç bu modele ait değil!", nameof(vehicle));

    if (Vehicles.Any(v => v.Id == vehicle.Id))
      throw new DomainValidationException("Araç zaten bu modele eklenmiş!", nameof(vehicle));

    Vehicles.Add(vehicle);
    AddStock(1); // Yeni araç eklendiğinde stok artar
  }


  // Vehicle kaldır (relationship yönetimi)
  public void RemoveVehicle(Vehicle vehicle)
  {
    if (vehicle == null)
      throw new ArgumentNullException(nameof(vehicle));

    if (!Vehicles.Any(v => v.Id == vehicle.Id))
      throw new DomainValidationException("Araç bu modelde bulunamadı!", nameof(vehicle));

    if (vehicle.IsActive && !vehicle.IsDeleted && vehicle.IsAvailable)
      throw new DomainValidationException("Müsait bir araç modelden kaldırılamaz!", nameof(vehicle));

    Vehicles.Remove(vehicle);
    RemoveStock(1); // Araç kaldırıldığında stok azalır
  }


  // Domain validasyon
  public void Validate()
  {
    if (string.IsNullOrWhiteSpace(Brand))
      throw new DomainValidationException("Marka adı boş olamaz!");

    if (string.IsNullOrWhiteSpace(Name))
      throw new DomainValidationException("Model adı boş olamaz!");

    if (Stock < 0)
      throw new DomainValidationException("Stok 0'dan küçük olamaz!");

    if (AvailableStock < 0)
      throw new DomainValidationException("Müsait stok 0'dan küçük olamaz!");

    if (AvailableStock > Stock)
      throw new DomainValidationException("Müsait stok, toplam stoktan büyük olamaz!");
  }
}




/*
    // 1. Yeni model oluştur
    var bmwX5 = new VehicleModel(
        brand: "BMW",
        name: "X5",
        description: "Lüks SUV",
        stock: 10,
        createdBy: adminUserId
    );

    // 2. Stok ekle
    bmwX5.AddStock(5); // Stock: 10 → 15, AvailableStock: 10 → 15

    // 3. Araç kirala
    bmwX5.RentVehicle(3); // AvailableStock: 15 → 12

    // 4. Araç iade et
    bmwX5.ReturnVehicle(2); // AvailableStock: 12 → 14

    // 5. Bakıma al
    bmwX5.SendToMaintenance(1); // AvailableStock: 14 → 13

    // 6. Stok durumunu kontrol et
    var status = bmwX5.GetStockStatus(); // "Stokta mevcut"
    var percentage = bmwX5.GetAvailabilityPercentage(); // 86.67%

    // 7. Validasyon
    bmwX5.Validate(); // Geçerliyse exception fırlatmaz

    // 8. Exception fırlatan senaryolar
    bmwX5.RentVehicle(20); // ❌ InsufficientStockException
    bmwX5.SetStock(5); // ❌ DomainValidationException (mevcut müsait stoktan az)


*/
