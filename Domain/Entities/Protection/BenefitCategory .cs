using Domain.Abstractions;

namespace Domain.Entities.Protection;

public sealed class BenefitCategory : BaseEntity
{
  // ==================== CONSTRUCTORS ====================

  // ✅ EF Core için private constructor
  private BenefitCategory() : base(Guid.Empty)
  {
    Name = string.Empty;
    Slug = string.Empty;
  }

  // ✅ Business constructor
  public BenefitCategory(
      string name,
      string slug,
      string? description,
      string? icon,
      int displayOrder,
      Guid createdBy) : base(createdBy)
  {
    Name = name;
    Slug = slug.ToLowerInvariant();
    Description = description;
    Icon = icon;
    DisplayOrder = displayOrder;
  }

  // ==================== PROPERTIES ====================

  public string Name { get; private set; }
  public string Slug { get; private set; }
  public string? Description { get; private set; }
  public string? Icon { get; private set; }
  public int DisplayOrder { get; private set; }

  // ==================== NAVIGATION PROPERTIES ====================

  public ICollection<ProtectionBenefit> Benefits { get; private set; } = new List<ProtectionBenefit>();

  // ==================== DOMAIN METHODS ====================

  public void AddBenefit(ProtectionBenefit benefit)
  {
    if (!Benefits.Contains(benefit))
      Benefits.Add(benefit);
  }

  public void RemoveBenefit(ProtectionBenefit benefit)
  {
    if (Benefits.Contains(benefit))
      Benefits.Remove(benefit);
  }

  public void ClearBenefits()
  {
    Benefits.Clear();
  }

  // ✅ ReadOnly erişim (opsiyonel)
  public IReadOnlyCollection<ProtectionBenefit> BenefitsList => Benefits.ToList().AsReadOnly();

  public void UpdateDetails(
      string name,
      string slug,
      string? description,
      string? icon,
      int displayOrder)
  {
    Name = name;
    Slug = slug.ToLowerInvariant();
    Description = description;
    Icon = icon;
    DisplayOrder = displayOrder;
  }
}