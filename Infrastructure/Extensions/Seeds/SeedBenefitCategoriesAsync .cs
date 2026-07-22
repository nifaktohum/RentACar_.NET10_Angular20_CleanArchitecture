using Domain.Entities.Protection;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Extensions.Seeds;

public static class AppDbContextSeedExtensions
{
  /// <summary>
  /// Varsayılan Benefit Kategorilerini oluşturur
  /// </summary>
  public static async Task SeedBenefitCategoriesAsync(this AppDbContext context, IConfiguration configuration)
  {
    // Zaten veri varsa çık
    if (await context.BenefitCategories.AnyAsync())
      return;

    var adminUserId = Guid.Parse(configuration["SeedData:AdminUserId"] ?? "00000000-0000-0000-0000-000000000001");

    var categories = new List<BenefitCategory>
        {
            new BenefitCategory(
                name: "Lastik",
                slug: "lastik",
                description: "Araç lastik güvencesi",
                icon: "ri-tire-line",
                displayOrder: 1,
                createdBy: adminUserId
            ),
            new BenefitCategory(
                name: "Cam",
                slug: "cam",
                description: "Araç cam güvencesi",
                icon: "ri-glass-line",
                displayOrder: 2,
                createdBy: adminUserId
            ),
            new BenefitCategory(
                name: "Far",
                slug: "far",
                description: "Araç far güvencesi",
                icon: "ri-flashlight-line",
                displayOrder: 3,
                createdBy: adminUserId
            ),
            new BenefitCategory(
                name: "Motor",
                slug: "motor",
                description: "Araç motor güvencesi",
                icon: "ri-engine-line",
                displayOrder: 4,
                createdBy: adminUserId
            ),
            new BenefitCategory(
                name: "Karoser",
                slug: "karoser",
                description: "Araç karoser güvencesi",
                icon: "ri-car-line",
                displayOrder: 5,
                createdBy: adminUserId
            ),
            new BenefitCategory(
                name: "3. Şahıs",
                slug: "3-sahis",
                description: "3. şahıs sorumluluk güvencesi",
                icon: "ri-user-shield-line",
                displayOrder: 6,
                createdBy: adminUserId
            ),
            new BenefitCategory(
                name: "Mini Hasar",
                slug: "mini-hasar",
                description: "Mini hasar güvencesi",
                icon: "ri-tools-line",
                displayOrder: 7,
                createdBy: adminUserId
            ),
            new BenefitCategory(
                name: "Anahtar/Plaka/Ruhsat",
                slug: "anahtar-plaka-ruhsat",
                description: "Anahtar, plaka ve ruhsat güvencesi",
                icon: "ri-key-2-line",
                displayOrder: 8,
                createdBy: adminUserId
            ),
            new BenefitCategory(
                name: "Yol Yardımı",
                slug: "yol-yardimi",
                description: "Yol yardım ve çekici hizmeti",
                icon: "ri-roadster-line",
                displayOrder: 9,
                createdBy: adminUserId
            ),
            new BenefitCategory(
                name: "Hırsızlık",
                slug: "hirsizlik",
                description: "Araç hırsızlık güvencesi",
                icon: "ri-shield-user-line",
                displayOrder: 10,
                createdBy: adminUserId
            )
        };

    await context.BenefitCategories.AddRangeAsync(categories);
    await context.SaveChangesAsync();

    // ✅ Mevcut orphaned (yetim) benefit'leri ilk kategoriye bağla
    var firstCategory = await context.BenefitCategories
        .OrderBy(c => c.DisplayOrder)
        .FirstOrDefaultAsync();

    if (firstCategory != null)
    {
      // ✅ CategoryId'si boş olan benefit'leri bul (Guid.Empty)
      var orphanedBenefits = await context.ProtectionBenefits
          .Where(b => b.CategoryId == Guid.Empty)
          .ToListAsync();

      foreach (var benefit in orphanedBenefits)
      {
        // ✅ UpdateDetails ile kategori ata
        benefit.UpdateDetails(
            name: benefit.Name,
            description: benefit.Description,
            icon: benefit.Icon,
            displayOrder: benefit.DisplayOrder,
            categoryId: firstCategory.Id
        );
      }

      if (orphanedBenefits.Any())
      {
        await context.SaveChangesAsync();
        Console.WriteLine($"--> {orphanedBenefits.Count} orphaned benefit 'Diğer' kategorisine bağlandı.");
      }
    }
  }
}