using Domain.Entities.Protection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Protection;

public sealed class BenefitCategoryConfiguration: BaseEntityConfiguration<BenefitCategory>
{
  public override void Configure(EntityTypeBuilder<BenefitCategory> builder)
  {
    base.Configure(builder);
    builder.ToTable("BenefitCategories", "protection");

    builder.Property(b => b.Name).IsRequired().HasMaxLength(100);
    builder.Property(b => b.Slug).IsRequired().HasMaxLength(100);
    builder.Property(b => b.Description).HasMaxLength(500);
    builder.Property(b => b.Icon).HasMaxLength(50);
    builder.Property(b => b.DisplayOrder).IsRequired().HasDefaultValue(0);

    builder.HasIndex(b => b.Slug).IsUnique().HasFilter("\"IsDeleted\" = false");
    builder.HasIndex(b => b.Name).HasFilter("\"IsDeleted\" = false");
    builder.HasIndex(b => b.DisplayOrder);

    builder.HasQueryFilter(b => !b.IsDeleted);

    // bu kod da migration'da BenefitCategoryId kolonunun tekrar çıkmasını engeller
    // BenefitsList isimli read-only bir property vardı ve builder.Ignore() ile gizlendi
    builder.Ignore(b => b.BenefitsList);


  }
    
}
