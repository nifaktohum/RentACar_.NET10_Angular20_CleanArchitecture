using Application.Services;
using Domain.Entities.Users;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Extensions;

// İlk admini ayağa kaldırma ve zorunlu adminlik kontrolleri buraya ayrıldı.
public static class UserSeedExtension
{
  public static async Task<User> CreateFirstAdminUserAsync(this AppDbContext context, IConfiguration configuration, IPasswordHasher passwordHasher, Guid merkezBranchId)
  {
    string adminEmail = configuration["SeedData:AdminEmail"] ?? "admin@rentacar.com";
    var existingAdmin = await context.Users.IgnoreQueryFilters()
                                            .FirstOrDefaultAsync(u => u.Email == adminEmail);
    if (existingAdmin != null)
    {
      Console.WriteLine($"--> [Seed] 👑 {adminEmail} zaten mevcut, atlanıyor.");
      return existingAdmin;
    }

    Guid adminUserId = Guid.Parse(configuration["SeedData:AdminUserId"] ?? "00000000-0000-0000-0000-000000000001");
    Guid adminRoleId = Guid.Parse(configuration["SeedData:AdminRoleId"] ?? "ade11111-701e-0000-0000-000000000000");
    string hashedPassword = passwordHasher.HashPassword("Admin123!");

    var firstAdmin = new User(
    firstName: "Sistem",
    lastName: "Admin",
    email: adminEmail,
    phoneNumber: "+905555555555",
    passwordHash: hashedPassword,
    branchId: merkezBranchId,
    roleId: adminRoleId,
    createdBy: adminUserId );

    // ID'yi set et
    context.Entry(firstAdmin).Property(x => x.Id).CurrentValue = adminUserId;
    await context.Users.AddAsync(firstAdmin);
    await context.SaveChangesAsync();

    Console.WriteLine($"--> [Seed] 👑 {adminEmail} kullanıcısı oluşturuldu! (Id: {adminUserId})");
    return firstAdmin;
  }

  // ✅ Mevcut Kullanıcıyı Admin Yap
  public static async Task EnsureUserIsAdminAsync(
      this AppDbContext context,
      IConfiguration configuration,
      string email)
  {
    var user = await context.Users
        .IgnoreQueryFilters()
        .Include(u => u.UserRoles)
        .FirstOrDefaultAsync(u => u.Email == email);

    if (user == null)
    {
      Console.WriteLine($"--> [Seed] ⚠️ {email} kullanıcısı bulunamadı!");
      return;
    }

    Guid adminRoleId = Guid.Parse(configuration["SeedData:AdminRoleId"]!);
    bool hasAdminRole = user.UserRoles.Any(ur => ur.RoleId == adminRoleId);

    if (!hasAdminRole)
    {
      user.AddRole(adminRoleId);
      await context.SaveChangesAsync();
      Console.WriteLine($"--> [Seed] 👑 {email} kullanıcısı ADMIN yapıldı!");
    }
    else
    {
      Console.WriteLine($"--> [Seed] 👑 {email} zaten ADMIN!");
    }
  }
}