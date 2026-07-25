using System.ComponentModel;
using Domain.Entities.Extras;
using Domain.Entities.Extras.Enum;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Extensions.Seeds;

/// Extra tablosuna seed verilerini ekler.
/// Eğer tabloda veri yoksa ekleme yapar.
public static class ExtrasSeed
{
  public static async Task ExtrasSeedAsync(this AppDbContext _context, IConfiguration _config)
  {
    // Eğer tabloda zaten veri varsa ekleme yapma
    if (await _context.Extras.AnyAsync()) return;

    // Varsayılan kullanıcı ID (System/Admin)
    var systemUserId = Guid.Parse(_config["SeedData:AdminUserId"]!);

    // ==================== EXTRA ÜRÜNLER ====================
    var extras = new List<Extra>
    {
        new Extra(
                name: "Mini Hasar Güvencesi",
                description: "Ehlüyeti ibrazı, kiralama ön şartlarının kabul (fındeksiz, deposito hariç) ve araç teslimi sırasında ek sürücünün de ofiste bizz bulunması gereklidir.",
                icon: "ri-shield-check-line",
                price: 114.00m,
                priceType: PriceType.Daily,
                category: ExtraCategory.Guarantee,
                displayOrder: 1,
                isRecommended: true,
                minAge: null,
                ageRange: null,
                stockLimit: null,
                createdBy: systemUserId,
                isActive: true
            ),

        new Extra(
                name: "Kış Lastiği",
                description: "Kış Lastiği (stoklarla sınırlıdır)",
                icon: "ri-snowflake-line",
                price: 246.00m,
                priceType: PriceType.Daily,
                category: ExtraCategory.Guarantee,
                displayOrder: 2,
                isRecommended: false,
                minAge: null,
                ageRange: null,
                stockLimit: 50,
                createdBy: systemUserId,
                isActive: true
            ),

        new Extra(
                name: "Genç Sürücü Paketi",
                description: "Yaş grubunuzun üst yaş grubundaki araç kiralayabilmenizi sağlamaktadır.",
                icon: "ri-user-star-line",
                price: 530.00m,
                priceType: PriceType.Daily,
                category: ExtraCategory.Driver,
                displayOrder: 3,
                isRecommended: false,
                minAge: 25,
                ageRange: null,
                stockLimit: null,
                createdBy: systemUserId,
                isActive: true
            ),

        new Extra(
                name: "Banka Kartı ile Kiralama",
                description: "Kiralama koşulları geçerli Banka Kartı ile kiralamalar için isteyen müşteriler bu ürünün satın alarak araç kiralamaya işlemleri devam edebilirler.",
                icon: "ri-bank-card-line",
                price: 3193.00m,
                priceType: PriceType.Rental,
                category: ExtraCategory.Other,
                displayOrder: 4,
                isRecommended: false,
                minAge: null,
                ageRange: null,
                stockLimit: null,
                createdBy: systemUserId,
                isActive: true
            ),

        new Extra(
                name: "Depozitosuz Kiralama",
                description: "Depozito ödemeyi yapmak istemeyen müşteriler bu ürünleri satın alarak araç kiralamaya yapabilir. Bu ürünün ücreti depozito gibi talep edilmektedir ve kontrat sonunda da olur şekilde çalışılabileceklerdir.",
                icon: "ri-wallet-3-line",
                price: 1064.00m,
                priceType: PriceType.Rental,
                category: ExtraCategory.Other,
                displayOrder: 5,
                isRecommended: false,
                minAge: null,
                ageRange: null,
                stockLimit: null,
                createdBy: systemUserId,
                isActive: true
            ),

        new Extra(
                name: "Koltuk Adaptörü",
                description: "4 yaşından sonra (15-36 kg.) arası çocuklar için arka koltuk yükseltici koltukları kullanılmalıdır. Çocuk aracın kemerine yükseltici yaşta kırılma başlarla.",
                icon: "ri-seat-line",
                price: 290.00m,
                priceType: PriceType.Daily,
                category: ExtraCategory.Seat,
                displayOrder: 6,
                isRecommended: false,
                minAge: null,
                ageRange: "15-36 kg",
                stockLimit: 10,
                createdBy: systemUserId,
                isActive: true
            ),

        new Extra(
                name: "Çocuk Koltuğu",
                description: "4 yaşına kadar (9-18 kg. arası) hareket çocuk güvenlik koltuğu tarzında arka koltuğa, öne bakan şekilde monte edilebilir.",
                icon: "ri-child-line",
                price: 290.00m,
                priceType: PriceType.Daily,
                category: ExtraCategory.Seat,
                displayOrder: 7,
                isRecommended: false,
                minAge: null,
                ageRange: "9-18 kg",
                stockLimit: 8,
                createdBy: systemUserId,
                isActive: true
            ),

        new Extra(
                name: "Bebek Koltuğu",
                description: "1 yaşına kadar (0 kiloya kadar) bebekler için yapılmış olan ana kuşağı modelinde, arka koluğa, arka başkaça şekilde monte edilebilir.",
                icon: "ri-baby-line",
                price: 290.00m,
                priceType: PriceType.Daily,
                category: ExtraCategory.Seat,
                displayOrder: 8,
                isRecommended: false,
                minAge: null,
                ageRange: "0-1 yaş",
                stockLimit: 5,
                createdBy: systemUserId,
                isActive: true
            )

    };

    await _context.Extras.AddRangeAsync(extras);
    await _context.SaveChangesAsync();
  }

}
