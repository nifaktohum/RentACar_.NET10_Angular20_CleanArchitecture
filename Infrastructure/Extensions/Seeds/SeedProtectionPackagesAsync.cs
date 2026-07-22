using Domain.Entities.Protection;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Extensions.Seeds;

public static class SeedProtectionPackages
{
  public static async Task SeedProtectionPackagesAsync(this AppDbContext context, IConfiguration configuration)
  {
    // Zaten paket varsa çık
    if (await context.ProtectionPackages.AnyAsync()) return;

    var adminUserId = Guid.Parse(configuration["SeedData:AdminUserId"] ?? "00000000-0000-0000-0000-000000000001");

    // Kategorileri ve benefit'leri al
    var benefits = await context.ProtectionBenefits.ToListAsync();

    var packages = new List<ProtectionPackage>
    {
        new ProtectionPackage(
            name: "Sınırlı Güvence",
            description: "Temel koruma paketi, uygun fiyatlı",
            icon: "ri-shield-line",
            displayOrder: 1,
            isRecommended: false,
            starRating: 2,
            protectionLevel: ProtectionLevel.Standard,
            deductibleType: DeductibleType.WithDeductible,
            createdBy: adminUserId,
            isActive: true
        ),
        new ProtectionPackage(
            name: "Gold Güvence",
            description: "Premium koruma paketi, muafiyetsiz",
            icon: "ri-shield-star-line",
            displayOrder: 2,
            isRecommended: true,
            starRating: 4,
            protectionLevel: ProtectionLevel.Premium,
            deductibleType: DeductibleType.ZeroDeductible,
            createdBy: adminUserId,
            isActive: true
        ),
        new ProtectionPackage(
            name: "Platinum Güvence",
            description: "En kapsamlı koruma paketi",
            icon: "ri-shield-star-fill",
            displayOrder: 3,
            isRecommended: true,
            starRating: 5,
            protectionLevel: ProtectionLevel.Platinum,
            deductibleType: DeductibleType.ZeroDeductible,
            createdBy: adminUserId,
            isActive: true
        ),
        new ProtectionPackage(
            name: "Ekonomik Güvence",
            description: "En uygun fiyatlı koruma paketi",
            icon: "ri-shield-dollar-line",
            displayOrder: 4,
            isRecommended: false,
            starRating: 1,
            protectionLevel: ProtectionLevel.Basic,
            deductibleType: DeductibleType.WithDeductible,
            createdBy: adminUserId,
            isActive: true
        )
    };

    await context.ProtectionPackages.AddRangeAsync(packages);
    await context.SaveChangesAsync();

    // Benefit'leri paketlere ekle (Many-to-Many)
    if (benefits.Any())
    {
      var goldPackage = packages.First(p => p.Name == "Gold Güvence");
      var platinumPackage = packages.First(p => p.Name == "Platinum Güvence");
      var limitedPackage = packages.First(p => p.Name == "Sınırlı Güvence");
      var economicPackage = packages.First(p => p.Name == "Ekonomik Güvence");

      // Gold Paketi - Tüm benefit'ler
      foreach (var benefit in benefits)
        goldPackage.AddBenefit(benefit);

      // Platinum Paketi - Tüm benefit'ler
      foreach (var benefit in benefits)
        platinumPackage.AddBenefit(benefit);

      // Sınırlı Paketi - Sadece ilk 3 benefit
      var limitedBenefits = benefits.Take(3).ToList();
      foreach (var benefit in limitedBenefits)
        limitedPackage.AddBenefit(benefit);

      // Ekonomik Paketi - Sadece 1 benefit
      if (benefits.Any())
        economicPackage.AddBenefit(benefits.First());

      await context.SaveChangesAsync();
    }

    // Pricing'leri ekle
    var pricings = new List<ProtectionPricing>
    {
        // Sınırlı Güvence
        new ProtectionPricing(
            protectionPackageId: packages.First(p => p.Name == "Sınırlı Güvence").Id,
            dailyPrice: null,
            deductibleAmount: 11500,
            isDefault: true,
            validityStart: DateTimeOffset.UtcNow,
            validityEnd: null,
            createdBy: adminUserId
        ),
        // Gold Güvence
        new ProtectionPricing(
            protectionPackageId: packages.First(p => p.Name == "Gold Güvence").Id,
            dailyPrice: 166,
            deductibleAmount: null,
            isDefault: true,
            validityStart: DateTimeOffset.UtcNow,
            validityEnd: null,
            createdBy: adminUserId
        ),
        // Platinum Güvence
        new ProtectionPricing(
            protectionPackageId: packages.First(p => p.Name == "Platinum Güvence").Id,
            dailyPrice: 250,
            deductibleAmount: null,
            isDefault: true,
            validityStart: DateTimeOffset.UtcNow,
            validityEnd: null,
            createdBy: adminUserId
        ),
        // Ekonomik Güvence
        new ProtectionPricing(
            protectionPackageId: packages.First(p => p.Name == "Ekonomik Güvence").Id,
            dailyPrice: null,
            deductibleAmount: 11500,
            isDefault: true,
            validityStart: DateTimeOffset.UtcNow,
            validityEnd: null,
            createdBy: adminUserId
        )
    };

    await context.ProtectionPricings.AddRangeAsync(pricings);
    await context.SaveChangesAsync();


    Console.WriteLine($"--> {packages.Count} koruma paketi oluşturuldu.");

  }

}
