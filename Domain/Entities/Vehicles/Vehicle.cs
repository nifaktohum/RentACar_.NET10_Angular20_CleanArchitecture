using Domain.Abstractions;
using Domain.Exceptions;

namespace Domain.Entities.Vehicles;

public sealed class Vehicle : BaseEntity
{
  // ==================== PRIVATE CONSTRUCTOR (EF Core için) ====================
  private Vehicle() : base(Guid.Empty)
  {
    Brand = string.Empty;
    Model = string.Empty;
    Year = string.Empty;
    Plate = string.Empty;
    Color = string.Empty;
    FuelType = string.Empty;
    Transmission = string.Empty;
    Description = null;
    VehicleModel = null!;
    Images = new List<VehicleImage>();
  }

  // ==================== PUBLIC CONSTRUCTOR ====================
  public Vehicle(
      string brand,
      string model,
      string year,
      string plate,
      string color,
      Guid vehicleModelId,
      string fuelType,
      string transmission,
      int seatCount,
      int doorCount,
      int? minAge,
      decimal dailyPrice,
      string? description,
      Guid createdBy,
      bool isActive = true
  ) : base(createdBy)
  {
    Brand = brand;
    Model = model;
    Year = year;
    Plate = plate;
    Color = color;
    VehicleModelId = vehicleModelId;
    FuelType = fuelType;
    Transmission = transmission;
    SeatCount = seatCount;
    DoorCount = doorCount;
    MinAge = minAge;
    DailyPrice = dailyPrice;
    Description = description;
    IsAvailable = true;

    VehicleModel = null!;
    Images = new List<VehicleImage>();

    if (!isActive) Deactivate();
  }

  // ==================== PROPERTIES ====================
  public string Brand { get; private set; }
  public string Model { get; private set; }
  public string Year { get; private set; }
  public string Plate { get; private set; }
  public string Color { get; private set; }
  public string FuelType { get; private set; }
  public string Transmission { get; private set; }
  public int SeatCount { get; private set; }
  public int DoorCount { get; private set; }
  public int? MinAge { get; private set; }
  public decimal DailyPrice { get; private set; }
  public bool IsAvailable { get; private set; }
  public string? Description { get; private set; }


  // ==================== RELATIONSHIP PROPERTIES ====================
  public Guid VehicleModelId { get; private set; }  
  public VehicleModel VehicleModel { get; private set; } = null!;  
  public ICollection<VehicleImage> Images { get; private set; }
  // ==================== DOMAIN METHODS ====================

  public void UpdateDetails(
      string brand,
      string model,
      string year,
      string plate,
      string color,
      Guid vehicleModelId,
      string fuelType,
      string transmission,
      int seatCount,
      int doorCount,
      int? minAge,
      decimal dailyPrice,
      string? description
  )
  {
    Brand = brand;
    Model = model;
    Year = year;
    Plate = plate;
    Color = color;
    VehicleModelId = vehicleModelId;
    FuelType = fuelType;
    Transmission = transmission;
    SeatCount = seatCount;
    DoorCount = doorCount;
    MinAge = minAge;
    DailyPrice = dailyPrice;
    Description = description;
  }

  // ✅ Yeni: Araç kiralandığında
  public void Rent()
  {
    if (!IsActive || IsDeleted)
      throw new DomainValidationException("Araç aktif değil veya silinmiş!");

    if (!IsAvailable)
      throw new DomainValidationException("Araç şu anda müsait değil!");

    IsAvailable = false;
  }

  // ✅ Yeni: Araç iade edildiğinde
  public void Return()
  {
    if (IsAvailable)
      throw new DomainValidationException("Araç zaten müsait!");

    IsAvailable = true;
  }


  // ✅ Yeni: VehicleModel ilişkisini kur
  public void SetVehicleModel(VehicleModel vehicleModel)
  {
    if (vehicleModel == null)
      throw new ArgumentNullException(nameof(vehicleModel));

    VehicleModel = vehicleModel;
    VehicleModelId = vehicleModel.Id;
  }




  // ==================== IMAGE METHODS ====================

  public string? GetMainImageUrl()
  {
    return Images.FirstOrDefault(i => i.IsMain)?.ImageUrl
           ?? Images.OrderBy(i => i.DisplayOrder).FirstOrDefault()?.ImageUrl;
  }

  public List<string> GetAllImageUrls()
  {
    return Images
        .Where(i => i.IsActive && !i.IsDeleted)
        .OrderBy(i => i.DisplayOrder)
        .Select(i => i.ImageUrl)
        .ToList();
  }

  public VehicleImage AddImage(string imageUrl, bool isMain = false, string? description = null)
  {
    if (isMain)
    {
      foreach (var img in Images.Where(i => i.IsMain && i.IsActive && !i.IsDeleted))
      {
        img.SetAsNonMain(); // Domain metodunuzu çağırın
      }
    }

    var image = new VehicleImage(
        vehicleId: Id,
        imageUrl: imageUrl,
        displayOrder: Images.Count(i => i.IsActive && !i.IsDeleted) + 1,
        isMain: isMain,
        description: description,
        createdBy: CreatedBy
    );

    Images.Add(image);
    return image;
  }

  public void RemoveImage(Guid imageId)
  {
    var image = Images.FirstOrDefault(i => i.Id == imageId);
    if (image != null && image.IsActive && !image.IsDeleted)
    {
      image.SoftDelete(CreatedBy);
    }
  }

  public void SetMainImage(Guid imageId)
  {
    var newMain = Images.FirstOrDefault(i => i.Id == imageId);
    if (newMain == null || !newMain.IsActive || newMain.IsDeleted)
      throw new DomainValidationException("Geçersiz resim!");

    foreach (var img in Images.Where(i => i.IsActive && !i.IsDeleted))
    {
      img.SetAsNonMain();
    }

    newMain.SetAsMain();
  }

  public void ClearImages()
  {
    foreach (var img in Images.Where(i => i.IsActive && !i.IsDeleted))
    {
      img.SoftDelete(CreatedBy);
    }
  }
}