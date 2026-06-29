using Infrastructure.Context;
using Application.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Infrastructure.Extensions.Seeds;

namespace Infrastructure.Extensions;

public static class DatabaseMasterInitializer
{
  public static async Task InitializeDatabaseSeedAsync(this IApplicationBuilder app, IConfiguration configuration)
  {
    using var scope = app.ApplicationServices.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

    try
    {
      Console.WriteLine("--> [Seed Master] Veritabanı yapılandırması başlatılıyor...");

      // 1️⃣ Roller
      await context.RolesAndPermissionsAsync(configuration);
      Console.WriteLine("--> [Seed Master] Adım 1 Roller tamamlandı.");

      // 2️⃣ İzinler
      await context.AdminPermissionsActiveAsync(configuration);
      Console.WriteLine("--> [Seed Master] Adım 2 İzinler tamamlandı.");

      // 3️⃣ Şube
      Guid adminUserId = Guid.Parse(configuration["SeedData:AdminUserId"] ?? "00000000-0000-0000-0000-000000000001");
      var merkezSube = await context.EnsureMerkezBranchCreatedAsync(configuration, adminUserId);
      Console.WriteLine("--> [Seed Master] Adım 3 Şube tamamlandı.");

      // 4️⃣ Kullanıcı
      await context.CreateFirstAdminUserAsync(configuration, passwordHasher, merkezSube.Id);
      Console.WriteLine("--> [Seed Master] Adım 4 Kullanıcı tamamlandı.");

      // 5️⃣ Customer Kullanıcı
      await context.CreateCustomerUserAsync(configuration, passwordHasher, merkezSube.Id);
      Console.WriteLine("--> [Seed Master] Adım 5 Customer kullanıcı tamamlandı.");

      // 6️⃣ 🆕 Kategoriler (YENİ)
      await context.SeedCategoriesAsync(configuration);
      Console.WriteLine("--> [Seed Master] Adım 6 Kategoriler tamamlandı.");

      Console.WriteLine("--> [Seed Master] ✅ Tüm parçalar başarıyla işlendi!");
    }
    catch (Exception ex)
    {
      Console.WriteLine("❌ KRİTİK HATA ŞURADA: " + ex.Message);
      Console.WriteLine("❌ DETAY: " + ex.ToString());
      throw;
    }
  }
}