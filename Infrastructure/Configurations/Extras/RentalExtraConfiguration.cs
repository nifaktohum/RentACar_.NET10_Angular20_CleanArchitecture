using Domain.Entities.Extras;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Extras;

public sealed class RentalExtraConfiguration : IEntityTypeConfiguration<RentalExtra>
{
  public void Configure(EntityTypeBuilder<RentalExtra> _builder)
  {
    // Tablo adını açıkça "RentalExtras" olarak belirliyoruz
    _builder.ToTable("RentalExtras");

    // Primary Key (Birincil Anahtar) olarak Id alanını seçiyoruz
    _builder.HasKey(re => re.Id);

    // UnitPrice (Birim fiyat) alanı zorunlu ve parasal işlemler için 18,2 hassasiyetinde olacak
    _builder.Property(re => re.UnitPrice)
        .IsRequired()
        .HasPrecision(18, 2);

    // Quantity (Adet/Miktar) alanı zorunlu ve varsayılan olarak 1 atanacak
    _builder.Property(re => re.Quantity)
        .IsRequired()
        .HasDefaultValue(1);

    // TotalPrice (Toplam fiyat) alanı zorunlu ve parasal işlemler için 18,2 hassasiyetinde olacak
    _builder.Property(re => re.TotalPrice)
        .IsRequired()
        .HasPrecision(18, 2);

    // Performans için RentalId ve ExtraId alanlarına tekli indeksler ekliyoruz
    _builder.HasIndex(re => re.RentalId);
    _builder.HasIndex(re => re.ExtraId);

    // Aynı ürünün aynı kiralamaya birden fazla eklenmesini önlemek için birleşik (composite) unique index
    _builder.HasIndex(re => new { re.RentalId, re.ExtraId })
        .IsUnique()
        .HasFilter("\"IsDeleted\" = false");  // Yalnızca silinmemiş olan kayıtlar için geçerli

    // Extra tablosu ile ilişki (Foreign Key) tanımlıyoruz
    _builder.HasOne(re => re.Extra)
        .WithMany(e => e.RentalExtras)
        .HasForeignKey(re => re.ExtraId)
        .OnDelete(DeleteBehavior.Restrict);  // Bir ekstra silindiğinde geçmiş kiralama kayıtlarının silinmesini engeller
  }
}

