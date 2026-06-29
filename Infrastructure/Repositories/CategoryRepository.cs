using System.Runtime.CompilerServices;
using Domain.Categories;
using Domain.Repositories;
using GenericRepository;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class CategoryRepository : Repository<Category, AppDbContext>, ICategoryRepository
{
  public CategoryRepository(AppDbContext context) : base(context)
  {
  }
  // Kategoriye ait benzersiz URL (slug) bilgisi ile veriyi getirir.
  public async Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
  {
    return await FirstOrDefaultAsync(c => c.Slug == slug, cancellationToken, false);
  }
  // Tüm kategorileri (ana ve alt kırılımlarıyla birlikte) ağaç yapısında getirir.
  // Kategori yönetim panellerinde tüm ağacı görselleştirmek için kullanılır.
  public async Task<List<Category>> GetCategoryHierarchyAsync(CancellationToken cancellationToken = default)
  {
    return await GetAll()
        .Include(c => c.SubCategories)  // ✅ Sadece 1 seviye alt kategori
        .Where(c => c.ParentCategoryId == null && !c.IsDeleted)
        .OrderBy(c => c.DisplayOrder)
        .ToListAsync(cancellationToken);
  }
  // Sadece ParentCategoryId alanı NULL olan "Ana Kategorileri" listeler.
  // Menü yapıları veya ana sayfa listelemeleri için kullanılır.
  public async Task<List<Category>> GetMainCategoriesAsync(CancellationToken cancellationToken = default)
  {
    return await Where(c => c.ParentCategoryId == null)
       .OrderBy(c => c.DisplayOrder)
       .ToListAsync(cancellationToken);
  }
  // Belirli bir üst kategoriye (parentId) bağlı olan tüm alt kategorileri getirir.
  // "Alt kırılımları göster" senaryoları için kullanılır.
  public async Task<List<Category>> GetSubCategoriesAsync(Guid parentId, CancellationToken cancellationToken = default)
  {
    return await Where(c => c.ParentCategoryId == parentId)
      .OrderBy(c => c.DisplayOrder)
      .ToListAsync(cancellationToken);

  }
  // Bir kategorinin silinmesi veya güncellenmesi öncesinde, 
  // altında başka kategori var mı diye kontrol eder. Güvenlik ve veri bütünlüğü için kritik.
  public async Task<bool> HasSubCategoriesAsync(Guid categoryId, CancellationToken cancellationToken = default)
  {
    return await AnyAsync(c => c.ParentCategoryId == categoryId, cancellationToken);
  }
}
