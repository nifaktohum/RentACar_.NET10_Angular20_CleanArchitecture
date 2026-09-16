using Domain.Abstractions;

namespace Domain.Entities.Vehicles;

public sealed class VehicleType : BaseEntity
{
  private VehicleType() : base(Guid.Empty)
  {
    Name = string.Empty;
    Description = null;
    Icon = null;
    VehicleModels = new List<VehicleModel>();  // ✅ VehicleModels
  }
  public VehicleType(
    string name,
    string? description,
    string? icon,
    int displayOrder,
    Guid createdBy
) : base(createdBy)
  {
    Name = name;
    Description = description;
    Icon = icon;
    DisplayOrder = displayOrder;
    VehicleModels = new List<VehicleModel>();
  }

  public string Name { get; private set; }           // "Sedan", "SUV", "Van"
  public string? Description { get; private set; }   // "Binek araç"
  public string? Icon { get; private set; }          // "ri-car-line"
  public int DisplayOrder { get; private set; }      // Sıralama
  public ICollection<VehicleModel> VehicleModels { get; private set; } = new List<VehicleModel>();

  public void UpdateDetails(
        string name,
        string? description,
        string? icon,
        int displayOrder
  )
  {
    Name = name;
    Description = description;
    Icon = icon;
    DisplayOrder = displayOrder;
  }
}
