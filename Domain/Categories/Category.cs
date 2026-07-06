using Domain.Abstractions;

namespace Domain.Categories;

public sealed class Category : BaseEntity
{
  // Constructor
  private Category() : base(Guid.Empty)
  {
    Name = string.Empty;
    Slug = string.Empty;
    SubCategories = new List<Category>();
  } // EF Core için

  public Category(
         string name,
         string slug,
         Guid createdBy,
         string? description = null,
         int? displayOrder = null,
         Guid? parentCategoryId = null)
         : base(createdBy)
  {
    Name = name;
    Slug = slug.ToLowerInvariant();
    Description = description;
    DisplayOrder = displayOrder;
    ParentCategoryId = parentCategoryId;
    SubCategories = new List<Category>();
  }

  // Temel Bilgiler
  public string Name { get; private set; }
  public string Slug { get; private set; }
  public string? Description { get; private set; }
  public int? DisplayOrder { get; private set; }

  // Hiyerarşik İlişki
  public Guid? ParentCategoryId { get; private set; }
  public Category? ParentCategory { get; private set; }
  public ICollection<Category> SubCategories { get; private set; } = new List<Category>();


  // Domain Metodları
  public void UpdateDetails(
      string name,
      string slug,
      string? description = null,
      int? displayOrder = null,
      Guid? parentCategoryId = null)
  {
    Name = name;
    Slug = slug.ToLowerInvariant();
    Description = description;
    DisplayOrder = displayOrder;
    ParentCategoryId = parentCategoryId;
  }

  public void SetParentCategory(Guid? parentCategoryId)
  {
    if (parentCategoryId.HasValue && parentCategoryId.Value == Id)
      throw new InvalidOperationException("Bir kategori kendi kendisinin alt kategorisi olamaz.");

    ParentCategoryId = parentCategoryId;
  }

  // ✅ OVERRIDE - Category'ye özel davranış
  public override void SetActiveStatus(bool isActive)
  {
    IsActive = isActive;

    if (SubCategories.Any())
    {
      foreach (var sub in SubCategories)
      {
        sub.SetActiveStatus(isActive);
      }
    }
  }

  // ✅ PRIVATE: Alt kategorileri pasif yap (Recursive)
  private void SetSubCategoriesInactive()
  {
    foreach (var sub in SubCategories)
    {
      sub.SetActiveStatus(false); // Recursive!
    }
  }

  // İş Kuralları
  // Bir kategorinin alt kategorileri olup olmadığını kontrol eder
  // UI'da kategori silme, güncelleme gibi işlemlerde kullanılır
  public bool HasSubCategories() => SubCategories?.Any() == true;
  // Kategorinin ana kategori(üst kategori) olup olmadığını kontrol eder
  // Yani ParentCategoryId'si NULL ise ana kategoridir
  public bool IsParentCategory() => ParentCategoryId == null;
  // Kategorinin alt kategori olup olmadığını kontrol eder
  // Yani ParentCategoryId'si NULL değilse alt kategoridir
  public bool IsSubCategory() => ParentCategoryId != null;
}

/*  ===>  Kullanım Senaryoları

Metod	                  Kontrol Eder	        Dönüş	      Kullanım Alanı
---------------------   --------------------- ----------- --------------------------
HasSubCategories()	    Alt kategori var mı?	bool	      Silme, UI, ağaç yapısı
IsParentCategory()	    Ana kategori mi?	    bool	      Filtreleme, UI, kampanyalar
IsSubCategory()	        Alt kategori mi?	    bool	      Filtreleme, UI, hiyerarşi

// Örnek 1: Alt kategorileri filtreleme
          var subCategories = categories
              .Where(c => c.IsSubCategory())
              .ToList();


// Örnek 2: Kategori hiyerarşisini gösterme
          if (category.IsSubCategory())
          {
              // Üst kategori bilgisini göster
              var parent = _repository.GetById(category.ParentCategoryId.Value);
              Console.WriteLine($"{parent.Name} → {category.Name}");
          }
          else
          {
              Console.WriteLine($"{category.Name} (Ana Kategori)");
          }

          
// Örnek 3: Filtreleme mantığı
          public List<Car> GetCarsByCategory(Category category)
          {
              if (category.IsParentCategory())
              {
                  // Ana kategori seçildi -> tüm alt kategorilerdeki araçları getir
                  var subCategoryIds = _repository.GetSubCategoryIds(category.Id);
                  return _carRepository.GetCarsByCategoryIds(subCategoryIds);
              }
              else
              {
                  // Alt kategori seçildi -> sadece bu kategorideki araçları getir
                  return _carRepository.GetCarsByCategoryId(category.Id);
              }
          }
*/

/*  ===>  Gerçek Kullanım Senaryosu
  // Kategori listesi oluşturma
          var categories = new List<Category>
          {
              new("Sedan", "sedan", userId),        // Ana kategori (ParentId = null)
              new("SUV", "suv", userId),            // Ana kategori (ParentId = null)
              new("Ekonomik Sedan", "ekonomik-sedan", userId, parentCategoryId: sedanId),
              new("Lüks Sedan", "luks-sedan", userId, parentCategoryId: sedanId)
          };

          // Kategori ağacını görüntüleme
          foreach (var category in categories)
          {
              if (category.IsParentCategory())
              {
                  Console.WriteLine($"📁 {category.Name}");
                  
                  if (category.HasSubCategories())
                  {
                      var subCats = categories.Where(c => c.ParentCategoryId == category.Id);
                      foreach (var sub in subCats)
                      {
                          Console.WriteLine($"  └── 📄 {sub.Name}");
                      }
                  }
              }
          }

// Çıktı:
// 📁 Sedan
//   └── 📄 Ekonomik Sedan
//   └── 📄 Lüks Sedan
// 📁 SUV
*/
