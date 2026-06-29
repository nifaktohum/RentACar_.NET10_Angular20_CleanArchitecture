using Domain.Categories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class CategoryConfiguration : BaseEntityConfiguration<Category>
{
  public override void Configure(EntityTypeBuilder<Category> _builder)
  {
    // Base konfigürasyonları uygula
    base.Configure(_builder);

    // Tablo adı
    _builder.ToTable("Categories");

    // Property konfigürasyonları
    _builder.Property(c => c.Name)
        .IsRequired()
        .HasMaxLength(100);

    _builder.Property(c => c.Slug)
        .IsRequired()
        .HasMaxLength(100);

    _builder.Property(c => c.Description)
        .HasMaxLength(500);

    // Index'ler
    _builder.HasIndex(c => c.Slug)
        .IsUnique()
        .HasFilter("\"IsDeleted\" = false");

    _builder.HasIndex(c => c.ParentCategoryId);

    // Self-Referencing Relationship
    _builder.HasOne(c => c.ParentCategory)
        .WithMany(c => c.SubCategories)
        .HasForeignKey(c => c.ParentCategoryId)
        .OnDelete(DeleteBehavior.Restrict); // Cascade delete'i engelle
  }
}
