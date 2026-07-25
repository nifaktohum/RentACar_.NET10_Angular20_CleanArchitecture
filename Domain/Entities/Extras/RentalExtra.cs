using Domain.Abstractions;

namespace Domain.Entities.Extras;
//  RentalExtra, bir kiralamanın hangi ekstra ürünleri seçtiğini ve 
//  bu seçime ait detayları (fiyat, adet, toplam tutar) saklamak için kullanılan ara tablo (junction table) entity'sidir.
public sealed class RentalExtra : BaseEntity
{
  public RentalExtra() : base(Guid.Empty) { }

  public RentalExtra(
      Guid rentalId,
      Guid extraId,
      decimal unitPrice,
      int quantity,
      decimal totalPrice,
      Guid createdBy
  ) : base(createdBy)
  {
    RentalId = rentalId;
    ExtraId = extraId;
    UnitPrice = unitPrice;
    Quantity = quantity;
    TotalPrice = totalPrice;
  }

  // ==================== PROPERTIES ====================

  public Guid RentalId { get; private set; }          // Sadece ID
  public Guid ExtraId { get; private set; }    // Sadece ID

  public decimal UnitPrice { get; private set; }
  public int Quantity { get; private set; }
  public decimal TotalPrice { get; private set; }

  // ==================== NAVIGATION PROPERTIES ====================
  // public Rental Rental { get; private set; }  // ŞİMDİLİK YOK!     // ❌ KALDIR
  public Extra Extra { get; private set; } = null!;
}

/* 1️⃣ Kiralama Sırasında Extra Ekleme

  // RentalService.cs
public async Task AddExtraToRental(Guid rentalId, Guid extraId, int quantity, Guid userId)
{
    // 1. Kiralamayı bul
    var rental = await _rentalRepository.GetByIdAsync(rentalId);
    if (rental == null) throw new NotFoundException("Kiralama bulunamadı");
    
    // 2. Ekstra ürünü bul
    var extra = await _extraRepository.GetByIdAsync(extraId);
    if (extra == null) throw new NotFoundException("Ekstra ürün bulunamadı");
    
    // 3. Stok kontrolü
    if (!extra.IsInStock()) throw new InvalidOperationException("Ürün stokta yok!");
    
    // 4. Fiyat hesapla (günlük mü, kiralama başı mı?)
    var rentalDayCount = rental.EndDate.DayNumber - rental.StartDate.DayNumber;
    decimal unitPrice = extra.Price;
    decimal totalPrice = extra.PriceType == PriceType.Daily 
        ? unitPrice * rentalDayCount * quantity
        : unitPrice * quantity;
    
    // 5. RentalExtra oluştur ✅ BURADA KULLANILIYOR!
    var rentalExtra = new RentalExtra(
        rentalId: rentalId,
        extraId: extraId,
        unitPrice: unitPrice,
        quantity: quantity,
        totalPrice: totalPrice,
        createdBy: userId
    );
    
    // 6. Kiralamaya ekle
    rental.AddExtra(rentalExtra);
    
    // 7. Stoku azalt
    extra.DecreaseStock(quantity);
    
    // 8. Kaydet
    await _rentalExtraRepository.AddAsync(rentalExtra);
    await _unitOfWork.SaveChangesAsync();
}

*/

/*  2️⃣ Kiralama Detaylarını Getirirken

// RentalService.cs
public async Task<RentalDetailDto> GetRentalDetail(Guid rentalId)
{
    var rental = await _rentalRepository.GetRentalWithExtrasAsync(rentalId);
    
    // BURADA KULLANILIYOR!
    var extraDtos = rental.RentalExtras.Select(re => new RentalExtraDto
    {
        ExtraId = re.ExtraId,
        ProductName = re.Extra.Name,
        UnitPrice = re.UnitPrice,
        Quantity = re.Quantity,
        TotalPrice = re.TotalPrice,
        PriceType = re.Extra.PriceType.ToString()
    }).ToList();
    
    return new RentalDetailDto
    {
        RentalId = rental.Id,
        CustomerName = rental.Customer.Name,
        CarModel = rental.Car.Model,
        TotalPrice = rental.TotalPrice,
        Extras = extraDtos // ✅ BURADA KULLANILIYOR!
    };
}

*/

/* ==> Response:

  {
  "rentalId": "3fa85f64-5717-4562-b3fc-2c963f66b001",
  "customerName": "Ahmet Yılmaz",
  "carModel": "Fiat Egea Sedan",
  "totalPrice": 9435.00,
  "extras": [
    {
      "extraId": "3fa85f64-5717-4562-b3fc-2c963f66afa7",
      "productName": "Kış Lastiği",
      "unitPrice": 246.00,
      "quantity": 1,
      "totalPrice": 1722.00,
      "priceType": "Daily"
    },
    {
      "extraId": "3fa85f64-5717-4562-b3fc-2c963f66afab",
      "productName": "Koltuk Adaptörü",
      "unitPrice": 290.00,
      "quantity": 1,
      "totalPrice": 2030.00,
      "priceType": "Daily"
    }
  ]
}

*/