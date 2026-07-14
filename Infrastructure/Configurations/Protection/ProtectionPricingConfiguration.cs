using Domain.Entities.Protection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Protection;

/// <summary>
/// ProtectionPricing varlığı için veritabanı eşleme ayarlarını yönetir.
/// Fiyatlandırma kurallarını, tarih aralıklarını ve paket ile olan ilişkisini tanımlar.
/// </summary>
public sealed class ProtectionPricingConfiguration : BaseEntityConfiguration<ProtectionPricing>
{
  public override void Configure(EntityTypeBuilder<ProtectionPricing> builder)
  {
    base.Configure(builder);

    // Tablo ismini 'protection' şeması altında tanımlıyoruz
    builder.ToTable("ProtectionPricings", "protection");

    // ==================== Property Konfigürasyonları ====================

    // PackageId zorunlu alan
    builder.Property(p => p.ProtectionPackageId).IsRequired();

    // Finansal alanlar: 18 basamak toplam, 2 basamak kuruş hassasiyeti
    builder.Property(p => p.DailyPrice).IsRequired(false).HasPrecision(18, 2);
    builder.Property(p => p.DeductibleAmount).IsRequired(false).HasPrecision(18, 2);

    builder.Property(p => p.IsDefault).IsRequired().HasDefaultValue(false);

    // ValidityStart için veritabanı seviyesinde "NOW()" kullanarak zamanı sabitliyoruz
    builder.Property(p => p.ValidityStart).IsRequired().HasDefaultValueSql("TIMEZONE('UTC', NOW())");

    builder.Property(p => p.ValidityEnd).IsRequired(false);

    // ==================== Index Tanımlamaları ====================

    // Hızlı erişim için indexler
    builder.HasIndex(p => p.ProtectionPackageId);

    // Paket bazlı aktif fiyatı bulmak için bileşik (composite) index
    builder.HasIndex(p => new { p.ProtectionPackageId, p.IsDefault });

    builder.HasIndex(p => p.ValidityStart);
    builder.HasIndex(p => p.ValidityEnd);

    // ==================== Relations ====================

    // ProtectionPricing -> ProtectionPackage (Bire Çok İlişki)
    // Paket silindiğinde ona bağlı tüm fiyat kayıtlarını da temizler (Cascade)
    builder.HasOne(p => p.ProtectionPackage)
        .WithMany(p => p.Pricing)
        .HasForeignKey(p => p.ProtectionPackageId)
        .OnDelete(DeleteBehavior.Cascade);

    // Soft Delete: Sadece aktif olan fiyatlandırmaları sorgulara dahil eder
    builder.HasQueryFilter(p => !p.IsDeleted);
  }
}
