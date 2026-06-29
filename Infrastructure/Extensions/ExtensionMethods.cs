using System.Linq.Expressions;
using Domain.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public static class ExtensionMethods
{
  public static void ApplyGlobalFilters(this ModelBuilder modelBuilder)
  {
    // 1. EF Core'un haritasındaki tüm veritabanı nesnelerini (Entity'leri) dönüyoruz
    foreach (var entityType in modelBuilder.Model.GetEntityTypes())
    {
      // 2. Eğer bu nesne bizim yazdığımız BaseEntity sınıfından miras alıyorsa
      if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
      {
        // 3. Arka planda dinamik olarak "e => e.IsDeleted == false" sorgusunu hazırlıyoruz
        var parameter = Expression.Parameter(entityType.ClrType, "e");
        var property = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
        var falseConstant = Expression.Constant(false);
        var comparison = Expression.Equal(property, falseConstant);
        var lambda = Expression.Lambda(comparison, parameter);

        // 4. Hazırladığımız bu filtreyi o tabloya (Entity'ye) global kural olarak gömüyoruz
        modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
      }
    }
  }
    
}
