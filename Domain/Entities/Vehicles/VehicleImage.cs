using Domain.Abstractions;
using Domain.Exceptions;

namespace Domain.Entities.Vehicles;

public class VehicleImage : BaseEntity
{
  // ==================== PRIVATE CONSTRUCTOR (EF Core için) ====================
  private VehicleImage() : base(Guid.Empty)
  {
    ImageUrl = string.Empty;
    Description = null;
    Vehicle = null!;
  }

  // ==================== PUBLIC CONSTRUCTOR ====================
  public VehicleImage(
      Guid vehicleId,
      string imageUrl,
      int displayOrder,
      bool isMain,
      string? description,
      Guid createdBy
  ) : base(createdBy)
  {
    SetVehicleId(vehicleId);
    SetImageUrl(imageUrl);
    SetDisplayOrder(displayOrder);
    IsMain = isMain;
    SetDescription(description);

    Vehicle = null!;
  }

  // ==================== PROPERTIES ====================
  public Guid VehicleId { get; private set; }
  public string ImageUrl { get; private set; } = string.Empty;
  public int DisplayOrder { get; private set; }
  public bool IsMain { get; private set; }
  public string? Description { get; private set; }

  // ==================== NAVIGATION PROPERTIES ====================
  public Vehicle Vehicle { get; private set; } = null!;

  // ==================== DOMAIN METHODS ====================

  public void SetAsMain() => IsMain = true;

  public void SetAsNonMain() => IsMain = false;

  public void UpdateDisplayOrder(int newOrder)
  {
    if (newOrder < 0)
      throw new DomainValidationException("Sıralama değeri 0'dan küçük olamaz!", nameof(newOrder));

    DisplayOrder = newOrder;
  }

  public void UpdateDescription(string? description)
  {
    SetDescription(description);
  }

  public void UpdateImageUrl(string imageUrl)
  {
    SetImageUrl(imageUrl);
  }

  // ==================== SETTER'LAR (VALIDASYONLU) ====================

  public void SetVehicleId(Guid vehicleId)
  {
    if (vehicleId == Guid.Empty)
      throw new DomainValidationException("VehicleId boş olamaz!", nameof(vehicleId));
    VehicleId = vehicleId;
  }

  public void SetImageUrl(string imageUrl)
  {
    if (string.IsNullOrWhiteSpace(imageUrl))
      throw new DomainValidationException("Resim URL'si boş olamaz!", nameof(imageUrl));
    if (imageUrl.Length > 1000)
      throw new DomainValidationException("Resim URL'si 1000 karakterden uzun olamaz!", nameof(imageUrl));
    ImageUrl = imageUrl.Trim();
  }

  public void SetDisplayOrder(int displayOrder)
  {
    if (displayOrder < 0)
      throw new DomainValidationException("Sıralama değeri 0'dan küçük olamaz!", nameof(displayOrder));
    DisplayOrder = displayOrder;
  }

  public void SetDescription(string? description)
  {
    if (description?.Length > 200)
      throw new DomainValidationException("Açıklama 200 karakterden uzun olamaz!", nameof(description));
    Description = description?.Trim();
  }
}