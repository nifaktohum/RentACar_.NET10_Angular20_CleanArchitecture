using Domain.Abstractions;
using Domain.Exceptions;

namespace Domain.Entities.Vehicles;

public sealed class VehicleType : BaseEntity
{
  private VehicleType() : base(Guid.Empty)
  {
    Name = string.Empty;
    Description = null;
    Icon = null;
    VehicleModels = new List<VehicleModel>();
  }

  public VehicleType(
      string name,
      string? description,
      string? icon,
      int displayOrder,
      Guid createdBy
  ) : base(createdBy)
  {
    SetName(name);
    SetDescription(description);
    SetIcon(icon);
    SetDisplayOrder(displayOrder);
    VehicleModels = new List<VehicleModel>();
  }

  public string Name { get; private set; } = string.Empty;
  public string? Description { get; private set; }
  public string? Icon { get; private set; }
  public int DisplayOrder { get; private set; }
  public ICollection<VehicleModel> VehicleModels { get; private set; } = new List<VehicleModel>();

  public void UpdateDetails(
      string name,
      string? description,
      string? icon,
      int displayOrder
  )
  {
    SetName(name);
    SetDescription(description);
    SetIcon(icon);
    SetDisplayOrder(displayOrder);
  }

  // ==================== SETTER'LAR (VALIDASYONLU) ====================

  public void SetName(string name)
  {
    if (string.IsNullOrWhiteSpace(name))
      throw new DomainValidationException("Tür adı boş olamaz!", nameof(name));
    if (name.Length > 50)
      throw new DomainValidationException("Tür adı 50 karakterden uzun olamaz!", nameof(name));
    Name = name.Trim();
  }

  public void SetDescription(string? description)
  {
    if (description?.Length > 200)
      throw new DomainValidationException("Açıklama 200 karakterden uzun olamaz!", nameof(description));
    Description = description?.Trim();
  }

  public void SetIcon(string? icon)
  {
    if (icon?.Length > 50)
      throw new DomainValidationException("İkon 50 karakterden uzun olamaz!", nameof(icon));
    Icon = icon?.Trim();
  }

  public void SetDisplayOrder(int displayOrder)
  {
    if (displayOrder < 0)
      throw new DomainValidationException("Sıralama değeri 0'dan küçük olamaz!", nameof(displayOrder));
    DisplayOrder = displayOrder;
  }
}