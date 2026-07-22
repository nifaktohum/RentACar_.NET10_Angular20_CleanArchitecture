using Application.Services;
using Domain.Entities.Users;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Extensions.Seeds;

public static class UserSeedExtension
{

  public static async Task<User> CreateCustomerUserAsync(this AppDbContext context, IConfiguration configuration, IPasswordHasher passwordHasher, Guid merkezBranchId)
  {
    string customerEmail = configuration["SeedData:CustomerEmail"] ?? "customer@rentacar.com";
    var existingCustomer = await context.Users
        .IgnoreQueryFilters()
        .FirstOrDefaultAsync(u => u.Email == customerEmail);

    if (existingCustomer != null)
    {
      Console.WriteLine($"--> [Seed] 👤 {customerEmail} zaten mevcut, atlanıyor.");
      return existingCustomer;
    }

    // 🔥 DOĞRU ID'LER AppSettings'den okunuyor
    Guid customerUserId = Guid.Parse(configuration["SeedData:CustomerUserId"] ?? "22222222-2222-2222-2222-222222222222");
    Guid customerRoleId = Guid.Parse(configuration["SeedData:CustomerRoleId"] ?? "c0555555-701e-0000-0000-000000000000");
    Guid adminUserId = Guid.Parse(configuration["SeedData:AdminUserId"] ?? "00000000-0000-0000-0000-000000000001");
    string hashedPassword = passwordHasher.HashPassword("Customer123!");

    var customerUser = new User(
    firstName: "Standart",
    lastName: "Müşteri",
    email: customerEmail,
    phoneNumber: "+905555555556",
    passwordHash: hashedPassword,
    branchId: merkezBranchId,
    roleId: customerRoleId,
    createdBy: adminUserId);

    // ID'yi set et
    context.Entry(customerUser).Property(x => x.Id).CurrentValue = customerUserId;
    await context.Users.AddAsync(customerUser);
    await context.SaveChangesAsync();

    Console.WriteLine($"--> [Seed] 👤 {customerEmail} kullanıcısı oluşturuldu! (Id: {customerUserId})");
    return customerUser;
  }
  
  





}