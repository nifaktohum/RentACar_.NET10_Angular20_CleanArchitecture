using Domain.Entities.Categories;
using GenericRepository;

namespace Domain.Repositories;

public interface ICategoryRepository : IRepository<Category>
{
  // Kategoriye ait benzersiz URL (slug) bilgisi ile veriyi getirir. 
  // SEO dostu rotalama işlemleri için kullanılır.
  Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

  // Sadece ParentCategoryId alanı NULL olan "Ana Kategorileri" listeler.
  // Menü yapıları veya ana sayfa listelemeleri için kullanılır.
  Task<List<Category>> GetMainCategoriesAsync(CancellationToken cancellationToken = default);

  // Belirli bir üst kategoriye (parentId) bağlı olan tüm alt kategorileri getirir.
  // "Alt kırılımları göster" senaryoları için kullanılır.
  Task<List<Category>> GetSubCategoriesAsync(Guid parentId, CancellationToken cancellationToken = default);

  // Bir kategorinin silinmesi veya güncellenmesi öncesinde, 
  // altında başka kategori var mı diye kontrol eder. Güvenlik ve veri bütünlüğü için kritik.
  Task<bool> HasSubCategoriesAsync(Guid categoryId, CancellationToken cancellationToken = default);

  // Tüm kategorileri (ana ve alt kırılımlarıyla birlikte) ağaç yapısında getirir.
  // Kategori yönetim panellerinde tüm ağacı görselleştirmek için kullanılır.
  Task<List<Category>> GetCategoryHierarchyAsync(CancellationToken cancellationToken = default);

  Task<Category?> GetByIdWithSubCategoriesAsync(Guid id, CancellationToken cancellationToken = default);
}
