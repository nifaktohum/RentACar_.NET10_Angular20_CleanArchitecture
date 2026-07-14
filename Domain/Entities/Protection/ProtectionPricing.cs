using Domain.Abstractions;

namespace Domain.Entities.Protection;
// Bu sınıf, bir koruma paketinin fiyat, muafiyet ve geçerlilik bilgilerini temsil eder. 
// Örneğin: "166 TL / Gün", "Muafiyet: 11.500 TL" gibi.
/// Belirli bir zaman aralığında geçerli olan fiyatlandırma kurallarını tutar.
/// Araç grubu veya kampanya dönemine göre esnek fiyatlandırma imkanı sunar.
public class ProtectionPricing : BaseEntity
{
  private ProtectionPricing() : base(Guid.Empty)
  {
  }
  public ProtectionPricing(
        Guid protectionPackageId,
        decimal? dailyPrice,
        decimal? deductibleAmount,
        bool isDefault,
        DateTimeOffset validityStart,
        DateTimeOffset? validityEnd,
        Guid createdBy) : base(createdBy)
  {
    ProtectionPackageId = protectionPackageId;
    DailyPrice = dailyPrice;
    DeductibleAmount = deductibleAmount;
    IsDefault = isDefault;
    ValidityStart = validityStart;
    ValidityEnd = validityEnd;
  }

  // ==================== PROPERTIES ====================
  // Foreign Key
  public Guid ProtectionPackageId { get; private set; }

  // Fiyat Bilgileri
  public decimal? DailyPrice { get; private set; }        // Günlük fiyat (166 TL)
  public decimal? DeductibleAmount { get; private set; }  // Muafiyet (11.500 TL)
  public bool IsDefault { get; private set; }             // Varsayılan fiyat mı?

  // Geçerlilik
  public DateTimeOffset ValidityStart { get; private set; }   // Başlangıç
  public DateTimeOffset? ValidityEnd { get; private set; }    // Bitiş (null = süresiz)

  // ==================== NAVIGATION PROPERTIES ====================
  public ProtectionPackage ProtectionPackage { get; private set; } = null!; // Bir fiyatlandırma tek bir pakete aittir


  // ==================== DOMAIN METHODS ====================
  public void UpdatePricing(
      decimal? dailyPrice,
      decimal? deductibleAmount,
      bool isDefault,
      DateTimeOffset validityStart,
      DateTimeOffset? validityEnd)
  {
    DailyPrice = dailyPrice;
    DeductibleAmount = deductibleAmount;
    IsDefault = isDefault;
    ValidityStart = validityStart;
    ValidityEnd = validityEnd;
  }

  /// Fiyatlandırmanın şu an geçerli olup olmadığını kontrol eder.
  public bool IsCurrentlyValid()
  {
    var now = DateTimeOffset.UtcNow;
    return now >= ValidityStart && (!ValidityEnd.HasValue || now <= ValidityEnd.Value);
  }

}

// ==================== 📊 GERÇEK DÜNYA ÖRNEĞİ: ====================

/* ==>Sınırlı Güvence Paketi

    {
      "protectionPackageId": "pkg-001",
      "dailyPrice": null,
      "deductibleAmount": 11500,
      "isDefault": true,
      "validityStart": "2024-01-01",
      "validityEnd": null
    }
*/

/*  ==> Gold Güvence Paketi

    {
      "protectionPackageId": "pkg-002",
      "dailyPrice": 166,
      "deductibleAmount": null,
      "isDefault": true,
      "validityStart": "2024-01-01",
      "validityEnd": null
    }
*/

/* ==> Kampanyalı Fiyat (Yaz Dönemi)

  {
    "protectionPackageId": "pkg-002",
    "dailyPrice": 125,
    "deductibleAmount": null,
    "isDefault": false,
    "validityStart": "2024-06-01",
    "validityEnd": "2024-09-01"
}
*/


// ==================== 🚀 KULLANIM SENARYOSU: ====================

/*  ==> 1. Yeni Fiyatlandırma Oluşturma

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

/*  ==> 2. Fiyat Güncelleme

    pricing.UpdatePricing(
              dailyPrice: 150,
              deductibleAmount: null,
              isDefault: true,
              validityStart: DateTimeOffset.UtcNow,
              validityEnd: null
          );
*/

/*  ==> 3. Geçerlilik Kontrolü

        if (pricing.IsCurrentlyValid())
        {
            Console.WriteLine($"Güncel Fiyat: {pricing.DailyPrice} TL");
        }
        else
        {
            Console.WriteLine("Bu fiyat geçerliliğini yitirmiştir.");
        }
*/

/*  ==> 4. Aktif Fiyatı Bulma (ProtectionPackage içinde)


      var activePricing = package.GetActivePricing();

      if (activePricing != null)
      {
          Console.WriteLine($"Fiyat: {activePricing.DailyPrice} TL / Gün");
          Console.WriteLine($"Muafiyet: {activePricing.DeductibleAmount ?? 0} TL");
      }
*/
