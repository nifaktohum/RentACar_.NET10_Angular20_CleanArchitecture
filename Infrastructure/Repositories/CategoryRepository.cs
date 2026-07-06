using System.Runtime.CompilerServices;
using Domain.Categories;
using Domain.Repositories;
using GenericRepository;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class CategoryRepository : Repository<Category, AppDbContext>, ICategoryRepository
{
  public CategoryRepository(AppDbContext _context) : base(_context)
  {
  }
  // Kategoriye ait benzersiz URL (slug) bilgisi ile veriyi getirir.
  public async Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
  {
    var res = await FirstOrDefaultAsync(
       c => c.Slug == slug && !c.IsDeleted,
       cancellationToken,
       false);

    if (res != null)
    {
      Console.WriteLine($"FOUND => Id:{res.Id} Slug:{res.Slug} IsDeleted:{res.IsDeleted}");
    }
    else
    {
      Console.WriteLine("FOUND => NULL");
    }

    return res;
  }
  // Tüm kategorileri (ana ve alt kırılımlarıyla birlikte) ağaç yapısında getirir.
  // Kategori yönetim panellerinde tüm ağacı görselleştirmek için kullanılır.
  public async Task<List<Category>> GetCategoryHierarchyAsync(CancellationToken cancellationToken = default)
  {
    return await GetAll()
           .Include(c => c.SubCategories) // <--- Sadece 1 seviye!
           .Where(c => c.ParentCategoryId == null && !c.IsDeleted) // <--- Alt kategorilerde IsDeleted kontrolü yok!
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
    return await AnyAsync(
        c => c.ParentCategoryId == categoryId && !c.IsDeleted,
        cancellationToken);
  }
  // belirli bir kategoriyi alt kategorileriyle birlikte tek seferde getirebilirsin.
  public async Task<Category?> GetByIdWithSubCategoriesAsync(Guid id, CancellationToken cancellationToken = default)
  {
    return await GetAll() // GetAll() metodu muhtemelen IQueryable döndürüyordur
         .Include(c => c.SubCategories.Where(sc => !sc.IsDeleted)) // Sadece silinmemiş alt kategorileri dahil et
         .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);
  }
}
