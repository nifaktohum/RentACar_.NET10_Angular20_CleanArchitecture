using Domain.Abstractions;
using Domain.Entities.Extras.Enum;

namespace Domain.Entities.Extras;
// Bu sınıf, araç kiralama sistemindeki ekstra ürünleri temsil eder.
// Örneğin: "Kış Lastiği", "Genç Sürücü Paketi", "Bebek Koltuğu" gibi.
public sealed class Extra : BaseEntity
{
  private Extra() : base(Guid.Empty)
  {
    Name = string.Empty;
    Description = string.Empty;
    RentalExtras = new List<RentalExtra>();
  }

  public Extra(
      string name,
      string? description,
      string? icon,
      decimal price,
      PriceType priceType,
      ExtraCategory category,
      int displayOrder,
      bool isRecommended,
      int? minAge,
      string? ageRange,
      int? stockLimit,
      Guid createdBy,
      bool isActive = true
  ) : base(createdBy)
  {
    Name = name;
    Description = description;
    Icon = icon;
    Price = price;
    PriceType = priceType;
    Category = category;
    DisplayOrder = displayOrder;
    IsRecommended = isRecommended;
    MinAge = minAge;
    AgeRange = ageRange;
    StockLimit = stockLimit;
    RentalExtras = new List<RentalExtra>();

    // BaseEntity'deki IsActive'i set et
    if (!isActive)
    {
      Deactivate(); // BaseEntity'den gelen metot
    }
  }

  // ==================== PROPERTIES ====================
  public string Name { get; private set; }
  public string? Description { get; private set; }
  public string? Icon { get; private set; }
  public decimal Price { get; private set; }
  public int DisplayOrder { get; private set; }
  public bool IsRecommended { get; private set; }

  public PriceType PriceType { get; private set; }
  public ExtraCategory Category { get; private set; }

  public int? MinAge { get; private set; }
  public string? AgeRange { get; private set; }
  public int? StockLimit { get; private set; }

  // ==================== NAVIGATION PROPERTIES ====================
  public ICollection<RentalExtra> RentalExtras { get; private set; } = new List<RentalExtra>();

  // ==================== DOMAIN METHODS ====================
  public void UpdateDetails(
      string name,
      string? description,
      string? icon,
      decimal price,
      PriceType priceType,
      ExtraCategory category,
      int displayOrder,
      bool isRecommended,
      int? minAge,
      string? ageRange,
      int? stockLimit)
  {
    Name = name;
    Description = description;
    Icon = icon;
    Price = price;
    PriceType = priceType;
    Category = category;
    DisplayOrder = displayOrder;
    IsRecommended = isRecommended;
    MinAge = minAge;
    AgeRange = ageRange;
    StockLimit = stockLimit;
  }

  public bool IsInStock()
  {
    return !StockLimit.HasValue || StockLimit.Value > 0;
  }

  public void DecreaseStock(int quantity = 1)
  {
    if (StockLimit.HasValue && StockLimit.Value >= quantity)
    {
      StockLimit -= quantity;
    }
    else
    {
      throw new InvalidOperationException("Yeterli stok yok!");
    }
  }

  public void IncreaseStock(int quantity = 1)
  {
    if (StockLimit.HasValue)
    {
      StockLimit += quantity;
    }
  }

  // Navigation property'ye ekleme metodu
  public void AddRentalExtra(RentalExtra rentalExtra)
  {
    RentalExtras.Add(rentalExtra);
  }

  // Navigation property'den çıkarma metodu
  public void RemoveRentalExtra(RentalExtra rentalExtra)
  {
    RentalExtras.Remove(rentalExtra);
  }
}

/*     DTO'lar (Data Transfer Objects)
  // ExtraDto.cs - Tüm bilgileri içerir (Response)
public class ExtraDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public decimal Price { get; set; }
    public string PriceType { get; set; }  // "Daily" veya "Rental"
    public string Category { get; set; }   // "Guarantee", "Driver", "Seat", "Other"
    public int DisplayOrder { get; set; }
    public bool IsRecommended { get; set; }
    public int? MinAge { get; set; }
    public string? AgeRange { get; set; }
    public int? StockLimit { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

// CreateExtraDto.cs - Yeni ürün oluşturma (Request)
public class CreateExtraDto
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public decimal Price { get; set; }
    public PriceType PriceType { get; set; }
    public ExtraCategory Category { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsRecommended { get; set; }
    public int? MinAge { get; set; }
    public string? AgeRange { get; set; }
    public int? StockLimit { get; set; }
    public bool IsActive { get; set; }
}

// UpdateExtraDto.cs - Ürün güncelleme (Request)
public class UpdateExtraDto
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public decimal Price { get; set; }
    public PriceType PriceType { get; set; }
    public ExtraCategory Category { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsRecommended { get; set; }
    public int? MinAge { get; set; }
    public string? AgeRange { get; set; }
    public int? StockLimit { get; set; }
    public bool IsActive { get; set; }
}

📊 ÖZET TABLOSU
Metot	  Endpoint	                        Request Body	                    Response
GET	    /api/extras	                        -	                            ExtraDto[]
GET	    /api/extras/category/{category}	    -	                            ExtraDto[]
GET	    /api/extras/{id}	                  -	                            ExtraDto
POST	  /api/extras	                      CreateExtraDto	          ExtraDto
PUT	    /api/extras/{id}	                UpdateExtraDto	          ExtraDto
DELETE	/api/extras/{id}	                  -	                            { success: true }
POST	  /api/rentals/{id}/extras	        { extraId, quantity }	  { totalPrice, extras: [] }



*/

/*  1️⃣ Tüm Ekstra Ürünleri Listeleme (GET)
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Mini Hasar Güvencesi",
    "description": "Ehlüyeti ibrazı, kiralama ön şartlarının kabul (fındeksiz, deposito hariç) ve araç teslimi sırasında ek sürücünün de ofiste bizz bulunması gereklidir.",
    "icon": "ri-shield-check-line",
    "price": 114.00,
    "priceType": "Daily",
    "category": "Guarantee",
    "displayOrder": 1,
    "isRecommended": true,
    "minAge": null,
    "ageRange": null,
    "stockLimit": null,
    "isActive": true
  },
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa7",
    "name": "Kış Lastiği",
    "description": "Kış Lastiği (stoklarla sınırlıdır)",
    "icon": "ri-snowflake-line",
    "price": 246.00,
    "priceType": "Daily",
    "category": "Guarantee",
    "displayOrder": 2,
    "isRecommended": false,
    "minAge": null,
    "ageRange": null,
    "stockLimit": 50,
    "isActive": true
  },
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa8",
    "name": "Genç Sürücü Paketi",
    "description": "Yaş grubunuzun üst yaş grubundaki araç kiralayabilmenizi sağlamaktadır.",
    "icon": "ri-user-star-line",
    "price": 530.00,
    "priceType": "Daily",
    "category": "Driver",
    "displayOrder": 3,
    "isRecommended": false,
    "minAge": 25,
    "ageRange": null,
    "stockLimit": null,
    "isActive": true
  },
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa9",
    "name": "Banka Kartı ile Kiralama",
    "description": "Kiralama koşulları geçerli Banka Kartı ile kiralamalar için isteyen müşteriler bu ürünün satın alarak araç kiralamaya işlemleri devam edebilirler.",
    "icon": "ri-bank-card-line",
    "price": 3193.00,
    "priceType": "Rental",
    "category": "Other",
    "displayOrder": 4,
    "isRecommended": false,
    "minAge": null,
    "ageRange": null,
    "stockLimit": null,
    "isActive": true
  },
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afaa",
    "name": "Depozitosuz Kiralama",
    "description": "Depozito ödemeyi yapmak istemeyen müşteriler bu ürünleri satın alarak araç kiralamaya yapabilir.",
    "icon": "ri-wallet-3-line",
    "price": 1064.00,
    "priceType": "Rental",
    "category": "Other",
    "displayOrder": 5,
    "isRecommended": false,
    "minAge": null,
    "ageRange": null,
    "stockLimit": null,
    "isActive": true
  },
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afab",
    "name": "Koltuk Adaptörü",
    "description": "4 yaşından sonra (15-36 kg.) arası çocuklar için arka koltuk yükseltici koltukları kullanılmalıdır.",
    "icon": "ri-seat-line",
    "price": 290.00,
    "priceType": "Daily",
    "category": "Seat",
    "displayOrder": 6,
    "isRecommended": false,
    "minAge": null,
    "ageRange": "15-36 kg",
    "stockLimit": 10,
    "isActive": true
  },
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afac",
    "name": "Çocuk Koltuğu",
    "description": "4 yaşına kadar (9-18 kg. arası) hareket çocuk güvenlik koltuğu tarzında arka koltuğa, öne bakan şekilde monte edilebilir.",
    "icon": "ri-child-line",
    "price": 290.00,
    "priceType": "Daily",
    "category": "Seat",
    "displayOrder": 7,
    "isRecommended": false,
    "minAge": null,
    "ageRange": "9-18 kg",
    "stockLimit": 8,
    "isActive": true
  },
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afad",
    "name": "Bebek Koltuğu",
    "description": "1 yaşına kadar (0 kiloya kadar) bebekler için yapılmış olan ana kuşağı modelinde, arka koluğa, arka başkaça şekilde monte edilebilir.",
    "icon": "ri-baby-line",
    "price": 290.00,
    "priceType": "Daily",
    "category": "Seat",
    "displayOrder": 8,
    "isRecommended": false,
    "minAge": null,
    "ageRange": "0-1 yaş",
    "stockLimit": 5,
    "isActive": true
  }
]
*/

/*  2️⃣ Kategoriye Göre Filtreleme (GET)

      ==> GET /api/extras/category/Seat
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afab",
    "name": "Koltuk Adaptörü",
    "price": 290.00,
    "priceType": "Daily",
    "category": "Seat",
    "ageRange": "15-36 kg"
  },
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afac",
    "name": "Çocuk Koltuğu",
    "price": 290.00,
    "priceType": "Daily",
    "category": "Seat",
    "ageRange": "9-18 kg"
  },
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afad",
    "name": "Bebek Koltuğu",
    "price": 290.00,
    "priceType": "Daily",
    "category": "Seat",
    "ageRange": "0-1 yaş"
  }
]
*/

/*  3️⃣ Yeni Ekstra Ürün Ekleme (POST)

  ==> Backend Request Body:

{
  "name": "Ek Sürücü Paketi",
  "description": "Araç teslimi sırasında ek sürücünün ofiste bizzat bulunması gereklidir.",
  "icon": "ri-user-add-line",
  "price": 150.00,
  "priceType": "Daily",
  "category": "Driver",
  "displayOrder": 9,
  "isRecommended": false,
  "minAge": 21,
  "ageRange": null,
  "stockLimit": null,
  "isActive": true
}

  ==> Backend Response:

  {
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afae",
  "name": "Ek Sürücü Paketi",
  "description": "Araç teslimi sırasında ek sürücünün ofiste bizzat bulunması gereklidir.",
  "icon": "ri-user-add-line",
  "price": 150.00,
  "priceType": "Daily",
  "category": "Driver",
  "displayOrder": 9,
  "isRecommended": false,
  "minAge": 21,
  "ageRange": null,
  "stockLimit": null,
  "isActive": true,
  "createdAt": "2026-07-23T10:30:00.000Z",
  "createdBy": "3fa85f64-5717-4562-b3fc-2c963f66a001",
  "updatedAt": null,
  "updatedBy": null
}

*/

/*  4️⃣ Ekstra Ürün Güncelleme (PUT)

  {
  "name": "Mini Hasar Güvencesi (Premium)",
  "description": "Güncellenmiş açıklama",
  "price": 150.00,
  "priceType": "Daily",
  "category": "Guarantee",
  "displayOrder": 1,
  "isRecommended": true,
  "minAge": null,
  "ageRange": null,
  "stockLimit": null,
  "isActive": true
}

Backend Response:

{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Mini Hasar Güvencesi (Premium)",
  "description": "Güncellenmiş açıklama",
  "price": 150.00,
  "priceType": "Daily",
  "category": "Guarantee",
  "displayOrder": 1,
  "isRecommended": true,
  "minAge": null,
  "ageRange": null,
  "stockLimit": null,
  "isActive": true,
  "updatedAt": "2026-07-23T11:00:00.000Z",
  "updatedBy": "3fa85f64-5717-4562-b3fc-2c963f66a001"
}

*/

/*  6️⃣ Kiralama Sırasında Extra Seçme (Frontend + Backend)

// rental-extra.service.ts
addExtraToRental(rentalId: string, extraId: string, quantity: number): Observable<any> {
  const request = {
    rentalId: rentalId,
    extraId: extraId,
    quantity: quantity
  };
  return this.http.post(`${this.apiUrl}/rentals/${rentalId}/extras`, request);
}

Backend Request Body:

{
  "extraId": "3fa85f64-5717-4562-b3fc-2c963f66afa7",
  "quantity": 1,
  "rentalDayCount": 7
}

Backend Hesaplama:

// Service katmanında hesaplama
var extra = await _extraRepository.GetByIdAsync(extraId);
decimal price = extra.PriceType == PriceType.Daily 
    ? extra.Price * rentalDayCount  // 246 * 7 = 1722 TL
    : extra.Price;                  // 3193 TL (Banka Kartı gibi)

Backend Response:

{
  "rentalId": "3fa85f64-5717-4562-b3fc-2c963f66b001",
  "extras": [
    {
      "extraId": "3fa85f64-5717-4562-b3fc-2c963f66afa7",
      "name": "Kış Lastiği",
      "unitPrice": 246.00,
      "quantity": 1,
      "totalPrice": 1722.00,
      "priceType": "Daily"
    },
    {
      "extraId": "3fa85f64-5717-4562-b3fc-2c963f66afab",
      "name": "Koltuk Adaptörü",
      "unitPrice": 290.00,
      "quantity": 1,
      "totalPrice": 2030.00,
      "priceType": "Daily"
    }
  ],
  "totalExtraPrice": 3752.00
}

*/

/*  7️⃣ Özet Fiyat Hesaplama (UI için)
    Frontend (Angular) - Component:

    // rental-summary.component.ts
calculateTotal(): void {
  const basePrice = 1490; // Araç fiyatı
  const protectionPrice = 1000; // Güvence paketi fiyatı
  
  let extraTotal = 0;
  this.selectedExtras.forEach(extra => {
    if (extra.priceType === 'Daily') {
      extraTotal += extra.price * this.rentalDayCount;
    } else {
      extraTotal += extra.price;
    }
  });
  
  this.totalPrice = basePrice + protectionPrice + extraTotal;
}

UI Gösterimi:

📊 ÖZET
━━━━━━━━━━━━━━━━━━━━━
Araç Kiralama:     1.490 TL
Güvence Paketleri: 1.000 TL
━━━━━━━━━━━━━━━━━━━━━
Seçili Extralar:   
  ✅ Kış Lastiği       1.722 TL (7 gün x 246 TL)
  ✅ Koltuk Adaptörü   2.030 TL (7 gün x 290 TL)
  ✅ Banka Kartı        3.193 TL (Kiralama Başı)
━━━━━━━━━━━━━━━━━━━━━
TOPLAM:              9.435 TL
*/