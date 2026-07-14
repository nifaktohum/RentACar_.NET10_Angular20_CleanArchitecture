using Domain.Entities.Protection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Protection;


/// ProtectionBenefit varlığı için veritabanı eşleme (mapping) ayarlarını yönetir.
/// Paket içerisindeki her bir güvence maddesinin (Lastik, Cam vb.) veritabanı kurallarını tanımlar.
public sealed class ProtectionBenefitConfiguration : BaseEntityConfiguration<ProtectionBenefit>
{
  public override void Configure(EntityTypeBuilder<ProtectionBenefit> builder)
  {
    // Temel entity (BaseEntity) konfigürasyonlarını (Id, CreatedBy, IsDeleted vb.) uygula
    base.Configure(builder);

    // Tabloyu 'protection' şeması altında tutuyoruz
    builder.ToTable("ProtectionBenefits", "protection");

    // ==================== Property Konfigürasyonları ====================

    // Name alanı zorunludur ve 100 karakterle sınırlandırılmıştır
    builder.Property(b => b.Name).IsRequired().HasMaxLength(100);

    // İsteğe bağlı açıklamalar ve ikon bilgisi
    builder.Property(b => b.Description).HasMaxLength(500);
    builder.Property(b => b.Icon).HasMaxLength(50);

    // Sıralama varsayılan olarak 0'dan başlar
    builder.Property(b => b.DisplayOrder).IsRequired().HasDefaultValue(0);

    // BenefitCategory enum'ını veritabanında integer olarak saklar
    builder.Property(b => b.Category).IsRequired().HasConversion<int>();

    // ==================== Index Tanımlamaları ====================

    // İsme göre benzersiz (unique) index, sadece silinmemiş kayıtlar dikkate alınır
    builder.HasIndex(b => b.Name).IsUnique().HasFilter("\"IsDeleted\" = false");

    // Listeleme ve filtreleme performansını artırmak için indexler
    builder.HasIndex(b => b.DisplayOrder);builder.HasIndex(b => b.Category);

    // ==================== Global Sorgu Filtresi ====================

    // Soft Delete: "IsDeleted" alanı true olanları uygulama tarafında otomatik filtrele
    builder.HasQueryFilter(b => !b.IsDeleted);
  }
}
