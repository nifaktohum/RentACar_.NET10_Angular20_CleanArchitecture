using Domain.Entities.Protection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Protection;


/// ProtectionBenefit varlığı için veritabanı eşleme (mapping) ayarlarını yönetir.
/// Paket içerisindeki her bir güvence maddesinin (Lastik, Cam vb.) veritabanı kurallarını tanımlar.
// ProtectionBenefitConfiguration.cs - Netleştirilmiş hali
public sealed class ProtectionBenefitConfiguration : BaseEntityConfiguration<ProtectionBenefit>
{
  public override void Configure(EntityTypeBuilder<ProtectionBenefit> builder)
  {
    base.Configure(builder);

    builder.ToTable("ProtectionBenefits", "protection");

    // Property konfigürasyonları
    builder.Property(b => b.Name).IsRequired().HasMaxLength(100);
    builder.Property(b => b.Description).HasMaxLength(500);
    builder.Property(b => b.Icon).HasMaxLength(50);
    builder.Property(b => b.DisplayOrder).IsRequired().HasDefaultValue(0);

    builder.Property(b => b.CategoryId).IsRequired();

    builder.HasOne(x => x.Category)
       .WithMany(x => x.Benefits)
       .HasForeignKey(x => x.CategoryId)
       .OnDelete(DeleteBehavior.Restrict);

    // Index'ler
    builder.HasIndex(x => new { x.CategoryId, x.Name })
        .IsUnique()
        .HasFilter("\"IsDeleted\" = false");

    builder.HasIndex(b => b.DisplayOrder);
    builder.HasIndex(b => b.CategoryId);

    // Global Filter
    builder.HasQueryFilter(b => !b.IsDeleted);
    builder.Navigation(x => x.Category).AutoInclude(false);
  }
}
