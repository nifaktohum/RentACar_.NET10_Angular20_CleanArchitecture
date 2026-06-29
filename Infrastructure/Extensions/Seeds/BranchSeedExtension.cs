using Domain.Branchs;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Extensions;

// Sadece şube oluşturma, kontrol etme ve ID sabitleme sorumluluğu buraya ait.
public static class BranchSeedExtension
{
  public static async Task<Branch> EnsureMerkezBranchCreatedAsync(
     this AppDbContext context,
     IConfiguration configuration,
     Guid adminUserId)
  {
    Guid merkezBranchId = Guid.Parse(configuration["SeedData:MerkezBranchId"] ?? "11111111-1111-1111-1111-111111111111");

    // Sadece ID'ye göre ara
    var merkezSube = await context.Set<Branch>()
        .IgnoreQueryFilters()
        .FirstOrDefaultAsync(b => b.Id == merkezBranchId);

    if (merkezSube != null)
    {
      Console.WriteLine("--> [Seed] 🏢 Merkez Şube zaten var, atlanıyor.");
      return merkezSube;
    }

    // ID yoksa yeni oluştur (ismi kontrol etme, direk oluştur)
    var merkezAddress = new Address(
        City: configuration["SeedData:MerkezBranch:City"] ?? "İstanbul",
        District: configuration["SeedData:MerkezBranch:District"] ?? "Beşiktaş",
        FullAddress: configuration["SeedData:MerkezBranch:FullAddress"] ?? "Barbaros Bulvarı, No:100, Kat:4",
        Phone1: configuration["SeedData:MerkezBranch:Phone1"] ?? "+902125550100",
        Phone2: configuration["SeedData:MerkezBranch:Phone2"],
        Email: configuration["SeedData:MerkezBranch:Email"] ?? "hq@rentcarproject.com"
    );

    string branchName = configuration["SeedData:MerkezBranch:Name"] ?? "Merkez Şube";

    // Branch constructor'ına ID gönder
    merkezSube = new Branch(merkezBranchId, branchName, merkezAddress, adminUserId);

    await context.Set<Branch>().AddAsync(merkezSube);
    await context.SaveChangesAsync();
    Console.WriteLine($"--> [Seed] 🏢 {branchName} oluşturuldu! (Id: {merkezBranchId})");
    return merkezSube;
  }
}