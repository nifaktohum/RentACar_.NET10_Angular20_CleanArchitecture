namespace Application.Features.ProtectionPackages._BenefitCategories.Dto;

public sealed class BenefitCategoryResponseDto
{
  public Guid Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public string Slug { get; set; } = string.Empty;
  public string? Description { get; set; }
  public string? Icon { get; set; }
  public int DisplayOrder { get; set; }
  public bool IsActive { get; set; }
  public DateTimeOffset CreatedAt { get; set; }
  public Guid CreatedBy { get; set; }
  public string? CreatedByName { get; set; }
  public DateTimeOffset? UpdatedAt { get; set; }
  public Guid? UpdatedBy { get; set; }
  public string? UpdatedByName { get; set; }
  public int BenefitCount { get; set; }  // ✅ Bu kategoriye kaç benefit bağlı?
  public List<BenefitBriefDto>? Benefits { get; set; }  // ✅ Benefit'leri de getir
}

public sealed class BenefitBriefDto
{
  public Guid Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public string? Icon { get; set; }
  public int DisplayOrder { get; set; }
  public bool IsActive { get; set; }
}