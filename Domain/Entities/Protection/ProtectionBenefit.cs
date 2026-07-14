using Domain.Abstractions;
using Domain.Protection;

namespace Domain.Entities.Protection;
/// Bu sınıf, bir koruma paketinin içindeki tek bir avantajı/kapsamı temsil eder. 
/// Örneğin: "Lastik Cam Far Güvencesi", "3. Şahıs Sorumluluk", "Mini Hasar" gibi.
/// Birden fazla pakette ortak olarak kullanılabilir (Many-to-Many).
public sealed class ProtectionBenefit : BaseEntity
{
  private ProtectionBenefit() : base(Guid.Empty)
  {
    Name = string.Empty;
    ProtectionPackages = new List<ProtectionPackage>();
  }
  public ProtectionBenefit(
                  string name,
                  string? description,
                  string? icon,
                  int displayOrder,
                  BenefitCategory category,
                  Guid createdBy
                                  ) : base(createdBy)
  {
    Name = name;
    Description = description;
    Icon = icon;
    DisplayOrder = displayOrder;
    Category = category;
    ProtectionPackages = new List<ProtectionPackage>();
  }

  // ==================== PROPERTIES ====================

  // Temel Bilgiler
  public string Name { get; private set; }              // "Lastik Cam Far Güvencesi"
  public string? Description { get; private set; }      // "Araç lastik, cam ve far güvencesi"
  public string? Icon { get; private set; }             // "ri-tire-line"
  public int DisplayOrder { get; private set; }         // 1, 2, 3 (sıralama)
  public BenefitCategory Category { get; private set; } // Tire, Glass, ThirdParty, vs.

  // ==================== NAVIGATION PROPERTIES ====================

  public ICollection<ProtectionPackage> ProtectionPackages { get; private set; }  // Bir kapsam birden fazla pakette kullanılabilir
  // Örnek: "Lastik Cam Far Güvencesi" hem "Gold Güvence" hem de "Sınırlı Güvence" paketlerinde olabilir

  // ==================== DOMAIN METHODS ====================

  public void UpdateDetails(
      string name,
      string? description,
      string? icon,
      int displayOrder,
      BenefitCategory category)
  {
    Name = name;
    Description = description;
    Icon = icon;
    DisplayOrder = displayOrder;
    Category = category;
  }

}

/* ==> Benefit Örnekleri
  [
    {
        "id": "ben-001",
        "name": "Lastik Cam Far Güvencesi",
        "description": "Araç lastik, cam ve far güvencesi",
        "icon": "ri-tire-line",
        "displayOrder": 1,
        "category": "Tire"
    },
    {
        "id": "ben-002",
        "name": "Genişletilmiş 3. Şahıs Sorumluluk Güvencesi",
        "description": "3. şahıs sorumluluk kapsamı",
        "icon": "ri-user-shield-line",
        "displayOrder": 2,
        "category": "ThirdParty"
    },
    {
        "id": "ben-003",
        "name": "Mini Hasar Güvencesi",
        "description": "Küçük hasar güvencesi",
        "icon": "ri-tools-line",
        "displayOrder": 3,
        "category": "MiniDamage"
    }
]
*/

// ==================== 🚀 KULLANIM SENARYOSU: ====================

/* ==> 1. Yeni Kapsam Oluşturma

      var tireBenefit = new ProtectionBenefit(
                                name: "Lastik Cam Far Güvencesi",
                                description: "Araç lastik, cam ve far güvencesi",
                                icon: "ri-tire-line",
                                displayOrder: 1,
                                category: BenefitCategory.Tire,
                                createdBy: currentUserId
                            );
*/

/* ==> 2. Kapsam Güncelleme

          tireBenefit.UpdateDetails(
              name: "Lastik Cam Far Güvencesi (Genişletilmiş)",
              description: "Araç lastik, cam, far ve yan ayna güvencesi",
              icon: "ri-tire-line",
              displayOrder: 1,
              category: BenefitCategory.Tire
          );
*/

/*  ==> 3. Kapsamı Pakete Ekleme

          // Benefit'i oluştur
          var tireBenefit = new ProtectionBenefit(...);

          // Paketi oluştur
          var goldPackage = new ProtectionPackage(...);

          // Kapsamı pakete ekle
          goldPackage.AddBenefit(tireBenefit);
          // Artık bu kapsam Gold Güvence paketinin içinde!

*/

/*  ==> 4. Tüm Kapsamları Listeleme

              var allBenefits = await _context.ProtectionBenefits
                                                  .OrderBy(b => b.Category)
                                                  .ThenBy(b => b.DisplayOrder)
                                                  .ToListAsync();

              foreach (var benefit in allBenefits)
              {
                  Console.WriteLine($"📋 {benefit.Name} ({benefit.Category})");
              }
*/