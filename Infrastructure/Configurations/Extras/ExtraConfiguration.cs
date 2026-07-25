using Domain.Entities.Extras;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Extras;

public sealed class ExtraConfiguration : IEntityTypeConfiguration<Extra>
{
  public void Configure(EntityTypeBuilder<Extra> _builder)
  {
    // Tablo adını açıkça "Extras" olarak belirliyoruz
    _builder.ToTable("Extras");

    // Primary Key (Birincil Anahtar) olarak Id alanını seçiyoruz
    _builder.HasKey(e => e.Id);

    // Name alanı zorunlu ve maksimum 100 karakter uzunluğunda olacak
    _builder.Property(e => e.Name)
        .IsRequired()
        .HasMaxLength(100);

    // Description alanı opsiyonel ve maksimum 500 karakter olacak
    _builder.Property(e => e.Description)
        .HasMaxLength(500);

    // Icon (Remix Icon kodu vb.) alanı maksimum 50 karakter olacak
    _builder.Property(e => e.Icon)
      .HasMaxLength(50);

    // Price alanı zorunlu ve parasal işlemler için 18,2 hassasiyetinde olacak
    _builder.Property(e => e.Price)
        .IsRequired()
        .HasPrecision(18, 2);

    // PriceType enum değerini veritabanında int (sayısal) olarak saklıyoruz
    _builder.Property(e => e.PriceType)
        .IsRequired()
        .HasConversion<int>();

    // Category enum değerini veritabanında int (sayısal) olarak saklıyoruz
    _builder.Property(e => e.Category)
        .IsRequired()
        .HasConversion<int>();

    // DisplayOrder alanı zorunlu olacak ve varsayılan olarak 0 atanacak
    _builder.Property(e => e.DisplayOrder)
        .IsRequired()
        .HasDefaultValue(0);

    // IsRecommended alanı zorunlu olacak ve varsayılan olarak false (önerilmiyor) atanacak
    _builder.Property(e => e.IsRecommended)
        .IsRequired()
        .HasDefaultValue(false);

    // MinAge alanı opsiyonel olacak (varsayılan değer null)
    _builder.Property(e => e.MinAge)
        .HasDefaultValue(null);

    // AgeRange açıklaması için maksimum 50 karakter sınır veriyoruz
    _builder.Property(e => e.AgeRange)
        .HasMaxLength(50);

    // StockLimit alanı opsiyonel olacak (varsayılan değer null)
    _builder.Property(e => e.StockLimit)
        .HasDefaultValue(null);

    // Name alanı için unique (benzersiz) index oluşturuyoruz (Yalnızca silinmemiş olanlar dahil edilir)
    _builder.HasIndex(e => e.Name)
        .IsUnique()
        .HasFilter("\"IsDeleted\" = false");

    // Sık sorgulanacak alanlar için veritabanı indeksleri (Index) tanımlıyoruz
    _builder.HasIndex(e => e.Category);
    _builder.HasIndex(e => e.PriceType);
    _builder.HasIndex(e => e.DisplayOrder);
    _builder.HasIndex(e => e.IsRecommended);
    _builder.HasIndex(e => e.IsActive);
  }
}