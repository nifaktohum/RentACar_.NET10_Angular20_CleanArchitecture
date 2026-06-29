using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
  public void Configure(EntityTypeBuilder<User> _builder)
  {
    // 1. Tablo Adı Tanımlaması (İsteğe bağlı, PostgreSQL için toplu çoğul isim iyidir)
    _builder.ToTable("Users");

    // 2. BaseEntity'den gelen Id zaten otomatik maplenir ama açıkça belirtmek iyi bir pratiktir.
    _builder.HasKey(u => u.Id);

    // 3. FirstName & LastName Alanları
    _builder.Property(u => u.FirstName)
        .HasMaxLength(50)
        .IsRequired();

    _builder.Property(u => u.LastName)
        .HasMaxLength(50)
        .IsRequired();

    // 4. FullName Alanı (Artık DB hesaplamıyor, C# tarafında set edilen değer yazılıyor)
    _builder.Property(u => u.FullName)
        .HasMaxLength(101) // 50 (First) + 1 (Boşluk) + 50 (Last)
        .IsRequired();

    // 5. Email Alanı (Zorunlu, Maksimum Uzunluk ve Benzersiz/Unique Index)
    _builder.Property(u => u.Email)
        .HasMaxLength(150)
        .IsRequired();

    _builder.HasIndex(u => u.Email)
        .IsUnique(); // Aynı mail adresiyle 2. üyelik açılamaz.

    // 6. PhoneNumber Alanı
    _builder.Property(u => u.PhoneNumber)
        .HasMaxLength(20)
        .IsRequired();

    // 7. PasswordHash Alanı (Şifrelenmiş metinler uzun olabileceği için sınırı yüksek tutuyoruz)
    _builder.Property(u => u.PasswordHash)
        .HasMaxLength(500)
        .IsRequired();

    _builder.Property(u => u.BranchId)
            .IsRequired();

    _builder.Property(u => u.SecurityStamp)
               .HasColumnName("SecurityStamp") // Veritabanındaki tam kolon adı
               .IsRequired();

    // 8. BaseEntity'den gelen diğer alanların (SoftDelete vb.) sorgularda otomatik filtrelenmesi
    // Veritabanından veri çekerken IsDeleted = true olanlar otomatik olarak GELMEZ (Global Query Filter)
    _builder.HasQueryFilter(u => !u.IsDeleted);
  }
}
