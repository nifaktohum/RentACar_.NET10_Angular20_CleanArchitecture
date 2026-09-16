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
    VehicleId = vehicleId;
    ImageUrl = imageUrl;
    DisplayOrder = displayOrder;
    IsMain = isMain;
    Description = description;
    Vehicle = null!;
  }

  // ==================== PROPERTIES ====================
  public Guid VehicleId { get; private set; }
  public string ImageUrl { get; private set; }
  public int DisplayOrder { get; private set; }
  public bool IsMain { get; private set; }
  public string? Description { get; private set; }

  // ==================== NAVIGATION PROPERTIES ====================
  public Vehicle Vehicle { get; private set; } = null!;


  // ==================== DOMAIN METHODS ====================

  public void SetAsMain()
  {
    IsMain = true;
  }

  public void SetAsNonMain()
  {
    IsMain = false;
  }

  public void UpdateDisplayOrder(int newOrder)
  {
    if (newOrder < 0)
      throw new DomainValidationException("Sıralama değeri 0'dan küçük olamaz!");

    DisplayOrder = newOrder;
  }

  public void UpdateDescription(string? description)
  {
    Description = description;
  }

  public void UpdateImageUrl(string imageUrl)
  {
    if (string.IsNullOrWhiteSpace(imageUrl))
      throw new DomainValidationException("Resim URL'si boş olamaz!");

    ImageUrl = imageUrl;
  }
}
