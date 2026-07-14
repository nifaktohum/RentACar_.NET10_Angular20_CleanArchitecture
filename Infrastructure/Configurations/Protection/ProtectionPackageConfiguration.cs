using Domain.Entities.Protection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Protection;

/// <summary>
/// ProtectionPackage varlığı için veritabanı eşleme (mapping) ayarlarını yönetir.
/// Fluent API kullanılarak veritabanı şeması ve ilişkiler tanımlanmıştır.
/// </summary>
public class ProtectionPackageConfiguration : BaseEntityConfiguration<ProtectionPackage>
{
  public override void Configure(EntityTypeBuilder<ProtectionPackage> builder)
  {
    base.Configure(builder);

    // Tablo ismini ve şemasını belirler (protection şeması altında tutuyoruz)
    builder.ToTable("ProtectionPackages", "protection");

    // ==================== Property Konfigürasyonları ====================
    // Name, boş geçilemez ve 100 karakter sınırına sahip
    builder.Property(p => p.Name).IsRequired().HasMaxLength(100);
    // Description ve Icon opsiyonel olduğu için sadece boyut kısıtlaması ekledik
    builder.Property(p => p.Description).HasMaxLength(500);
    builder.Property(p => p.Icon).HasMaxLength(50);

    // Default değerler: Sistemde paket eklenirken varsayılan değerleri belirler
    builder.Property(p => p.DisplayOrder).IsRequired().HasDefaultValue(0);
    builder.Property(p => p.IsRecommended).IsRequired().HasDefaultValue(false);
    builder.Property(p => p.StarRating).IsRequired().HasDefaultValue(3);

    // Enum tiplerinin veritabanında integer olarak saklanmasını sağlar (0, 1, 2...)
    builder.Property(p => p.ProtectionLevel).IsRequired().HasConversion<int>();
    builder.Property(p => p.DeductibleType).IsRequired().HasConversion<int>();

    // ==================== Index Tanımlamaları ====================
    // Performans için kritik alanlar (Sorgularda hızlı erişim sağlar)
    // Sadece aktif (IsDeleted == false) kayıtlar için benzersiz (unique) index
    builder.HasIndex(p => p.Name).IsUnique().HasFilter("\"IsDeleted\" = false");

    builder.HasIndex(p => p.DisplayOrder);
    builder.HasIndex(p => p.IsRecommended);
    builder.HasIndex(p => p.ProtectionLevel);
    builder.HasIndex(p => p.DeductibleType);

    // ==================== Relations (İlişkiler) ====================

    // ProtectionPackage <-> ProtectionBenefit (Çoka Çok İlişki)
    // "ProtectionPackageBenefits" isimli bir ara tablo oluşturur
    builder.HasMany(p => p.Benefits)
        .WithMany(b => b.ProtectionPackages)
        .UsingEntity<Dictionary<string, object>>(
            "ProtectionPackageBenefits",
            j => j.HasOne<ProtectionBenefit>()
                .WithMany()
                .HasForeignKey("BenefitId")
                .OnDelete(DeleteBehavior.Restrict), // Benefit silinirse paketten referansı koru
            j => j.HasOne<ProtectionPackage>()
                .WithMany()
                .HasForeignKey("PackageId")
                .OnDelete(DeleteBehavior.Cascade) // Paket silinirse ara tablodaki bağları da sil
        );

    // ProtectionPackage <-> ProtectionPricing (Bire Çok İlişki)
    // Bir paketin birden fazla fiyatlandırması (zaman aralıklı) olabilir
    builder.HasMany(p => p.Pricing)
        .WithOne(p => p.ProtectionPackage)
        .HasForeignKey(p => p.ProtectionPackageId)
        .OnDelete(DeleteBehavior.Cascade); // Paket silinirse fiyatları da sil

    // ==================== Global Sorgu Filtresi ====================
    // Soft Delete: "IsDeleted" alanı true olanları hiçbir sorguda getirme
    builder.HasQueryFilter(p => !p.IsDeleted);
  }
}