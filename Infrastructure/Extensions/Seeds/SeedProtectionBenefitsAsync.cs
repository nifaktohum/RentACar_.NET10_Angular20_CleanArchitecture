using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities.Protection;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Extensions.Seeds;

public static class SeedProtectionBenefits
{
  public static async Task SeedProtectionBenefitsAsync(this AppDbContext context, IConfiguration configuration)
  {
    // Zaten benefit varsa çık
    if (await context.ProtectionBenefits.AnyAsync())
      return;

    var adminUserId = Guid.Parse(configuration["SeedData:AdminUserId"] ?? "00000000-0000-0000-0000-000000000001");

    // Kategorileri al
    var categories = await context.BenefitCategories
        .ToDictionaryAsync(c => c.Name, c => c.Id);

    var benefits = new List<ProtectionBenefit>
    {
        new ProtectionBenefit(
            name: "Lastik Cam Far Güvencesi",
            description: "Araç lastik, cam ve far güvencesi",
            icon: "ri-tire-line",
            displayOrder: 1,
            categoryId: categories.GetValueOrDefault("Lastik", Guid.Empty),
            createdBy: adminUserId
        ),
        new ProtectionBenefit(
            name: "3. Şahıs Sorumluluk Güvencesi",
            description: "3. şahıs sorumluluk kapsamı",
            icon: "ri-user-shield-line",
            displayOrder: 2,
            categoryId: categories.GetValueOrDefault("3. Şahıs", Guid.Empty),
            createdBy: adminUserId
        ),
        new ProtectionBenefit(
            name: "Mini Hasar Güvencesi",
            description: "Küçük hasar güvencesi",
            icon: "ri-tools-line",
            displayOrder: 3,
            categoryId: categories.GetValueOrDefault("Mini Hasar", Guid.Empty),
            createdBy: adminUserId
        ),
        new ProtectionBenefit(
            name: "Anahtar Plaka ve Ruhsat Güvencesi",
            description: "Anahtar, plaka ve ruhsat güvencesi",
            icon: "ri-key-2-line",
            displayOrder: 4,
            categoryId: categories.GetValueOrDefault("Anahtar/Plaka/Ruhsat", Guid.Empty),
            createdBy: adminUserId
        ),
        new ProtectionBenefit(
            name: "Yola Devam Hizmeti",
            description: "Yol yardım ve çekici hizmeti",
            icon: "ri-roadster-line",
            displayOrder: 5,
            categoryId: categories.GetValueOrDefault("Yol Yardımı", Guid.Empty),
            createdBy: adminUserId
        ),
        new ProtectionBenefit(
            name: "Hırsızlık Güvencesi",
            description: "Araç hırsızlık güvencesi",
            icon: "ri-shield-user-line",
            displayOrder: 6,
            categoryId: categories.GetValueOrDefault("Hırsızlık", Guid.Empty),
            createdBy: adminUserId
        ),
        new ProtectionBenefit(
            name: "Motor Güvencesi",
            description: "Araç motor güvencesi",
            icon: "ri-engine-line",
            displayOrder: 7,
            categoryId: categories.GetValueOrDefault("Motor", Guid.Empty),
            createdBy: adminUserId
        ),
        new ProtectionBenefit(
            name: "Karoser Güvencesi",
            description: "Araç karoser güvencesi",
            icon: "ri-car-line",
            displayOrder: 8,
            categoryId: categories.GetValueOrDefault("Karoser", Guid.Empty),
            createdBy: adminUserId
        ),
        new ProtectionBenefit(
            name: "Far Güvencesi",
            description: "Araç far güvencesi",
            icon: "ri-flashlight-line",
            displayOrder: 9,
            categoryId: categories.GetValueOrDefault("Far", Guid.Empty),
            createdBy: adminUserId
        ),
        new ProtectionBenefit(
            name: "Cam Güvencesi",
            description: "Araç cam güvencesi",
            icon: "ri-glass-line",
            displayOrder: 10,
            categoryId: categories.GetValueOrDefault("Cam", Guid.Empty),
            createdBy: adminUserId
        )
    };

    await context.ProtectionBenefits.AddRangeAsync(benefits);
    await context.SaveChangesAsync();

    Console.WriteLine($"--> {benefits.Count} benefit başarıyla oluşturuldu.");
  }
}
