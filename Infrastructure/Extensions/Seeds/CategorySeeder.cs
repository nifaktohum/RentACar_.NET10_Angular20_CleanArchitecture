using Domain.Categories;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Extensions.Seeds;

public static class CategorySeeder
{
  public static async Task SeedCategoriesAsync(this AppDbContext context, IConfiguration configuration)
  {
    if (await context.Categories.AnyAsync())
      return;

    // Sistem kullanıcısı (varsayılan)
    var systemUserId = Guid.Parse(configuration["SeedData:AdminUserId"] ?? "00000000-0000-0000-0000-000000000001");

    Console.WriteLine("--> Kategoriler oluşturuluyor...");

    // 1️⃣ Önce ANA Kategorileri oluştur
    var sedan = new Category("Sedan", "sedan", systemUserId, "Konforlu sedan araçlar", 1);
    var suv = new Category("SUV", "suv", systemUserId, "Geniş SUV araçlar", 2);
    var sport = new Category("Spor", "spor", systemUserId, "Sportif araçlar", 3);
    var ticari = new Category("Ticari", "ticari", systemUserId, "Ticari araçlar", 4);

    // Ana kategorileri veritabanına ekle (Id'ler oluşsun)
    await context.Categories.AddRangeAsync(sedan, suv, sport, ticari);
    await context.SaveChangesAsync(); // ✅ Burada Id'ler oluşur!

    // 2️⃣ Şimdi ALT Kategorileri oluştur (artık Id'ler var!)
    var ekonomikSedan = new Category("Ekonomik Sedan", "ekonomik-sedan", systemUserId,
        "Ekonomik sedan araçlar", 1, sedan.Id); // ✅ sedan.Id artık geçerli!

    var ortaSedan = new Category("Orta Sınıf Sedan", "orta-sinif-sedan", systemUserId,
        "Orta sınıf sedan araçlar", 2, sedan.Id); // ✅ sedan.Id geçerli!

    var luksSedan = new Category("Lüks Sedan", "luks-sedan", systemUserId,
        "Lüks sedan araçlar", 3, sedan.Id); // ✅ sedan.Id geçerli!

    var kompaktSuv = new Category("Kompakt SUV", "kompakt-suv", systemUserId,
        "Kompakt SUV araçlar", 1, suv.Id); // ✅ suv.Id geçerli!

    var ortaSuv = new Category("Orta SUV", "orta-suv", systemUserId,
        "Orta SUV araçlar", 2, suv.Id); // ✅ suv.Id geçerli!

    var buyukSuv = new Category("Büyük SUV", "buyuk-suv", systemUserId,
        "Büyük SUV araçlar (7 Koltuklu)", 3, suv.Id); // ✅ suv.Id geçerli!

    var coupe = new Category("Coupe", "coupe", systemUserId,
        "Coupe spor araçlar", 1, sport.Id); // ✅ sport.Id geçerli!

    var roadster = new Category("Roadster", "roadster", systemUserId,
        "Roadster spor araçlar", 2, sport.Id); // ✅ sport.Id geçerli!

    var minibus = new Category("Minibüs", "minibus", systemUserId,
        "Minibüsler", 1, ticari.Id); // ✅ ticari.Id geçerli!

    var kamyonet = new Category("Kamyonet", "kamyonet", systemUserId,
        "Kamyonetler", 2, ticari.Id); // ✅ ticari.Id geçerli!

    // Alt kategorileri veritabanına ekle
    await context.Categories.AddRangeAsync(
        ekonomikSedan, ortaSedan, luksSedan,
        kompaktSuv, ortaSuv, buyukSuv,
        coupe, roadster,
        minibus, kamyonet
    );

    await context.SaveChangesAsync();

    Console.WriteLine($"--> {await context.Categories.CountAsync()} kategori başarıyla eklendi.");
  }
}