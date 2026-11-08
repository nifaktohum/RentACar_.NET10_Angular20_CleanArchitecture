using Domain.Abstractions;
using Domain.Exceptions;

namespace Domain.Entities.Vehicles;

public class VehicleModel : BaseEntity
{
  // ==================== PRIVATE CONSTRUCTOR (EF Core için) ====================
  private VehicleModel() : base(Guid.Empty)
  {
    Name = string.Empty;
    Brand = string.Empty;
    Description = null;
    VehicleType = null!;
    Vehicles = new List<Vehicle>();
  }

  // ==================== PUBLIC CONSTRUCTOR ====================
  public VehicleModel(
      string name,
      string brand,
      string? description,
      Guid vehicleTypeId,
      Guid createdBy
  ) : base(createdBy)
  {
    SetName(name);
    SetBrand(brand);
    SetDescription(description);
    SetVehicleTypeId(vehicleTypeId);

    VehicleType = null!;
    Vehicles = new List<Vehicle>();
  }

  // ==================== PROPERTIES ====================
  public string Name { get; private set; } = string.Empty;
  public string Brand { get; private set; } = string.Empty;
  public string? Description { get; private set; }

  // ==================== COMPUTED PROPERTIES ====================
  /// Bu modele kayıtlı toplam fiziksel araç sayısı.
  public int Stock => Vehicles.Count(v => v.IsActive && !v.IsDeleted);

  /// Şu an kiraya verilebilir araç sayısı.
  public int AvailableStock => Vehicles.Count(v => v.IsAvailable);

  /// Stokta en az bir müsait araç var mı?
  public bool IsInStock => AvailableStock > 0;

  // ==================== NAVIGATION PROPERTIES ====================
  public Guid VehicleTypeId { get; private set; }
  public VehicleType VehicleType { get; private set; } = null!;
  public ICollection<Vehicle> Vehicles { get; private set; }

  // ==================== DOMAIN METHODS ====================

  public void SetBrand(string brand)
  {
    if (string.IsNullOrWhiteSpace(brand))
      throw new DomainValidationException("Marka adı boş olamaz!", nameof(brand));
    if (brand.Length > 50)
      throw new DomainValidationException("Marka adı 50 karakterden uzun olamaz!", nameof(brand));
    Brand = brand.Trim();
  }

  public void SetName(string name)
  {
    if (string.IsNullOrWhiteSpace(name))
      throw new DomainValidationException("Model adı boş olamaz!", nameof(name));
    if (name.Length > 100)   // ⚠️ 50 → 100 (config ile uyumlu)
      throw new DomainValidationException("Model adı 100 karakterden uzun olamaz!", nameof(name));
    Name = name.Trim();
  }

  public void SetDescription(string? description)
  {
    if (description?.Length > 500)
      throw new DomainValidationException("Açıklama 500 karakterden uzun olamaz!", nameof(description));
    Description = description?.Trim();
  }

  public void SetVehicleTypeId(Guid vehicleTypeId)
  {
    if (vehicleTypeId == Guid.Empty)
      throw new DomainValidationException("VehicleTypeId boş olamaz!", nameof(vehicleTypeId));
    VehicleTypeId = vehicleTypeId;
  }

  public void SetVehicleType(VehicleType vehicleType)
  {
    if (vehicleType == null)
      throw new ArgumentNullException(nameof(vehicleType));

    VehicleType = vehicleType;
    SetVehicleTypeId(vehicleType.Id);
  }

  // ==================== VEHICLE İLİŞKİ YÖNETİMİ ====================

  public void AddVehicle(Vehicle vehicle)
  {
    if (vehicle == null)
      throw new ArgumentNullException(nameof(vehicle));

    if (vehicle.VehicleModelId != Id)
      throw new DomainValidationException("Araç bu modele ait değil!", nameof(vehicle));

    if (Vehicles.Any(v => v.Id == vehicle.Id))
      throw new DomainValidationException("Araç zaten bu modele eklenmiş!", nameof(vehicle));

    Vehicles.Add(vehicle);
    // Stok computed — AddStock YOK
  }

  public void RemoveVehicle(Vehicle vehicle)
  {
    if (vehicle == null)
      throw new ArgumentNullException(nameof(vehicle));

    if (!Vehicles.Any(v => v.Id == vehicle.Id))
      throw new DomainValidationException("Araç bu modelde bulunamadı!", nameof(vehicle));

    if (vehicle.IsActive && !vehicle.IsDeleted && vehicle.IsAvailable)
      throw new DomainValidationException("Müsait bir araç modelden kaldırılamaz!", nameof(vehicle));

    Vehicles.Remove(vehicle);
    // Stok computed — RemoveStock YOK
  }

  // ==================== RAPORLAMA ====================

  public string GetStockStatus()
  {
    if (Stock == 0)
      return "Stokta yok";

    if (AvailableStock == 0)
      return "Tüm araçlar kiralandı/bakımda";

    if (AvailableStock <= Stock * 0.2m)
      return "Stok bitiyor!";

    return "Stokta mevcut";
  }

  public decimal GetAvailabilityPercentage()
  {
    if (Stock == 0)
      return 0;

    return Math.Round((decimal)AvailableStock / Stock * 100, 2);
  }

  // ==================== DOMAIN VALİDASYON ====================

  public void Validate()
  {
    if (string.IsNullOrWhiteSpace(Brand))
      throw new DomainValidationException("Marka adı boş olamaz!");

    if (string.IsNullOrWhiteSpace(Name))
      throw new DomainValidationException("Model adı boş olamaz!");

    if (VehicleTypeId == Guid.Empty)
      throw new DomainValidationException("VehicleTypeId boş olamaz!");

    // Stok kontrolleri YOK — computed (negatif olamaz, mantıksal olarak)
  }
}