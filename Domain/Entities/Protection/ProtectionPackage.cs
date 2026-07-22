

using Domain.Abstractions;

namespace Domain.Entities.Protection;

// Bu sınıf, araç kiralama sistemindeki koruma paketlerini(güvence paketleri) temsil eder.
// Örneğin: "Gold Güvence", "Sınırlı Güvence", "Minimum Güvence" gibi.
public sealed class ProtectionPackage : BaseEntity
{
  private ProtectionPackage() : base(Guid.Empty)
  {
    Name = string.Empty;
    Benefits = new List<ProtectionBenefit>();
    Pricing = new List<ProtectionPricing>();
  }
  public ProtectionPackage(
            string name,
            string? description,
            string? icon,
            int displayOrder,
            bool isRecommended,
            int starRating,
            ProtectionLevel protectionLevel,
            DeductibleType deductibleType,
            Guid createdBy,
            bool isActive = true
        ) : base(createdBy)
  {
    Name = name;
    Description = description;
    Icon = icon;
    DisplayOrder = displayOrder;
    IsRecommended = isRecommended;
    StarRating = starRating;
    ProtectionLevel = protectionLevel;
    DeductibleType = deductibleType;
    Benefits = new List<ProtectionBenefit>();
    Pricing = new List<ProtectionPricing>();
    IsActive = isActive;
  }


  // ==================== PROPERTIES ====================

  // Temel Bilgiler
  public string Name { get; private set; }              // "Gold Güvence"
  public string? Description { get; private set; }      // "En kapsamlı koruma..."
  public string? Icon { get; private set; }             // "ri-shield-star-line"
  public int DisplayOrder { get; private set; }         // 1, 2, 3 (sıralama)
  public bool IsRecommended { get; private set; }       // true/false
  public int StarRating { get; private set; }           // 1-5 yıldız

  // Tipler
  public ProtectionLevel ProtectionLevel { get; private set; }  // Basic, Premium...
  public DeductibleType DeductibleType { get; private set; }    // Muafiyetli, Muafiyetsiz

  // ==================== NAVIGATION PROPERTIES ====================

  public ICollection<ProtectionBenefit> Benefits { get; private set; }  // Bir paketin birden çok kapsamı olabilir(Lastik, Cam, vs.)
  public ICollection<ProtectionPricing> Pricing { get; private set; }   // Bir paketin birden çok fiyatlandırması olabilir (mevsimsel)


  // ==================== DOMAIN METHODS ====================
  // Paket Bilgilerini Güncelle
  public void UpdateDetails(
      string name,
      string? description,
      string? icon,
      int displayOrder,
      bool isRecommended,
      int starRating,
      ProtectionLevel protectionLevel,
      DeductibleType deductibleType)
  {
    Name = name;
    Description = description;
    Icon = icon;
    DisplayOrder = displayOrder;
    IsRecommended = isRecommended;
    StarRating = starRating;
    ProtectionLevel = protectionLevel;
    DeductibleType = deductibleType;
  }

  // Pakete yeni bir güvence özelliği ekler.
  public void AddBenefit(ProtectionBenefit benefit)
  {
    if (!Benefits.Contains(benefit))
      Benefits.Add(benefit);
  }

  // Paket içerisinden mevcut bir güvence özelliğini kaldırır.
  public void RemoveBenefit(ProtectionBenefit benefit)
  {
    if (Benefits.Contains(benefit))
      Benefits.Remove(benefit);
  }

  // Tüm Kapsamları Temizle  
  public void ClearBenefits()
  {
    Benefits.Clear();
  }

  // Pakete yeni bir fiyatlandırma ekler.
  public void AddPricing(ProtectionPricing pricing)
  {
    if (!Pricing.Contains(pricing))
      Pricing.Add(pricing);
  }

  public void RemovePricing(ProtectionPricing pricing)
  {
    if (Pricing.Contains(pricing))
      Pricing.Remove(pricing);
  }

  public void ClearPricing()
  {
    Pricing.Clear();
  }

  // Paketin şu an aktif olan fiyatlandırma kaydını döner.
  // Geçerlilik tarihlerini (ValidityStart/End) otomatik kontrol eder.
  public ProtectionPricing? GetActivePricing()
  {
    return Pricing.FirstOrDefault(p => p.IsDefault && p.IsCurrentlyValid());
  }

}

// ==================== 🚀 KULLANIM SENARYOSU: ====================

/* ==> 1. Yeni Paket Oluşturma

          var goldPackage = new ProtectionPackage(
            name: "Gold Güvence",
            description: "En kapsamlı koruma paketi",
            icon: "ri-shield-star-line",
            displayOrder: 1,
            isRecommended: true,
            starRating: 3,
            protectionLevel: ProtectionLevel.Premium,
            deductibleType: DeductibleType.ZeroDeductible,
            createdBy: currentUserId
        );

        // Kapsam ekle
        goldPackage.AddBenefit(tireBenefit);
        goldPackage.AddBenefit(glassBenefit);
        goldPackage.AddBenefit(thirdPartyBenefit);

        // Fiyat ekle
        var pricing = new ProtectionPricing(
            protectionPackageId: goldPackage.Id,
            dailyPrice: 166,
            deductibleAmount: null,
            isDefault: true,
            validityStart: DateTimeOffset.UtcNow,
            validityEnd: null,
            createdBy: currentUserId
        );
        goldPackage.AddPricing(pricing);

*/

/* ==> 2. Paket Bilgilerini Güncelleme

        goldPackage.UpdateDetails(
          name: "Gold Güvence (Yeni)",
          description: "Güncellenmiş açıklama",
          icon: "ri-shield-star-fill",
          displayOrder: 1,
          isRecommended: true,
          starRating: 4,
          protectionLevel: ProtectionLevel.Platinum,
          deductibleType: DeductibleType.ZeroDeductible
      );

*/

/* ==> 3. Aktif Fiyatı Getirme

          var activePrice = goldPackage.GetActivePricing();

          if (activePrice != null)
          {
              Console.WriteLine($"Günlük Fiyat: {activePrice.DailyPrice} TL");
              Console.WriteLine($"Muafiyet: {activePrice.DeductibleAmount ?? 0} TL");
          }

*/
