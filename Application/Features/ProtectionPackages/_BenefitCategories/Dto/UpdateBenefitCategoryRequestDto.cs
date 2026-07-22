namespace Application.Features.ProtectionPackages._BenefitCategories.Dto;

public sealed record UpdateBenefitCategoryRequestDto
{
  public Guid Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public string Slug { get; set; } = string.Empty;
  public string? Description { get; set; }
  public string? Icon { get; set; }
  public int DisplayOrder { get; set; }
}
