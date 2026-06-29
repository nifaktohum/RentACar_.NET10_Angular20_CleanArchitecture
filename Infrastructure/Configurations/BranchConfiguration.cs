using Domain.Branchs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

internal sealed class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
  public void Configure(EntityTypeBuilder<Branch> builder)
  {
    // 1. TABLO ADLANDIRMA: Veritabanında bu şemanın haritasını çıkartacağımız tablo ismi.
    builder.ToTable("Branches");

    // 2. KİMLİK (PRIMARY KEY): BaseEntity'den miras alınan Id alanını anahtar yapıyoruz.
    builder.HasKey(b => b.Id);

    // 3. ŞUBE ADI ÖZELLİKLERİ: Boş geçilemez (NOT NULL) ve maksimum 150 karakter sınırlandırması.
    builder.Property(b => b.Name)
        .IsRequired()
        .HasMaxLength(150);

    // 4. VALUE OBJECT (RECORD) YAPILANDIRMASI: 
    // Address bir record (Value Object) olduğu için ayrı bir tablo oluşturmaz, 
    // tüm iletişim ve adres alanlarını doğrudan "Branches" tablosunun içine kolon olarak gömer.
    builder.OwnsOne(b => b.Address, addressBuilder =>
    {
      addressBuilder.Property(a => a.City)
              .HasColumnName("Address_City")
              .IsRequired()
              .HasMaxLength(50); // İl adı için 50 karakter yeterlidir.

      addressBuilder.Property(a => a.District)
              .HasColumnName("Address_District")
              .IsRequired()
              .HasMaxLength(50); // İlçe adı için 50 karakter.

      addressBuilder.Property(a => a.FullAddress)
              .HasColumnName("Address_FullAddress")
              .IsRequired()
              .HasMaxLength(500); // Açık adres detaylı olabileceği için geniş tuttuk.

      addressBuilder.Property(a => a.Phone1)
              .HasColumnName("Address_Phone1")
              .IsRequired()
              .HasMaxLength(20); // Ülke kodlu telefon formatları düşünülerek 20 karakter.

      addressBuilder.Property(a => a.Phone2)
              .HasColumnName("Address_Phone2")
              .IsRequired(false) // record içinde "string?" olduğu için NULL olabilir (Nullable).
              .HasMaxLength(20);

      addressBuilder.Property(a => a.Email)
              .HasColumnName("Address_Email")
              .IsRequired()
              .HasMaxLength(100); // E-posta adresi için maksimum 100 karakter.
    });

  }
}