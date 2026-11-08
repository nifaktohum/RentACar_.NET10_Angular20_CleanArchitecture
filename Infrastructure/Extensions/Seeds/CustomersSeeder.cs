using Domain.Entities.Customers;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Persistence.Seeders; // Kendi namespace yoluna göre düzenleyebilirsin

public static class CustomersSeeder
{
  public static async Task SeedCustomersAsync(this AppDbContext context, IConfiguration configuration)
  {
    if (await context.Customers.AnyAsync()) return;

    var systemUserId = Guid.Empty; // Sistem kullanıcısı ID'si veya sabit bir Guid

    var customers = new List<Customer>
        {
            // 1. Bireysel Müşteri (Normal - B Sınıfı Ehliyetli, Findeksli)
            Customer.CreateIndividual(
                email: "ahmet.yilmaz@example.com",
                phoneNumber: "+905321112233",
                passwordHash: "$2a$11$DummyHashedPasswordForSeeding12345", // Örnek hash
                firstName: "Ahmet",
                lastName: "Yılmaz",
                identityNumber: "12345678901",
                birthDate: new DateOnly(1995, 5, 12),
                address: "Atatürk Cad. No: 15 Kat: 2",
                city: "İstanbul",
                country: "Türkiye",
                createdBy: systemUserId,
                gender: "Erkek",
                postalCode: "34340"
            ),

            // 2. Bireysel Müşteri (Lüks Araç Kiralayabilecek Yüksek Findeksli)
            Customer.CreateIndividual(
                email: "elif.demir@example.com",
                phoneNumber: "+905332223344",
                passwordHash: "$2a$11$DummyHashedPasswordForSeeding12345",
                firstName: "Elif",
                lastName: "Demir",
                identityNumber: "98765432109",
                birthDate: new DateOnly(1990, 8, 25),
                address: "Tunalı Hilmi Cad. No: 45",
                city: "Ankara",
                country: "Türkiye",
                createdBy: systemUserId,
                gender: "Kadın",
                postalCode: "06680"
            ),

            // 3. Bireysel Müşteri (Genç / Sınırda)
            Customer.CreateIndividual(
                email: "can.kaya@example.com",
                phoneNumber: "+905353334455",
                passwordHash: "$2a$11$DummyHashedPasswordForSeeding12345",
                firstName: "Can",
                lastName: "Kaya",
                identityNumber: "45678912345",
                birthDate: new DateOnly(2002, 2, 10),
                address: "Kıbrıs Şehitleri Cad. No: 88",
                city: "İzmir",
                country: "Türkiye",
                createdBy: systemUserId,
                gender: "Erkek",
                postalCode: "35220"
            ),

            // 4. Kurumsal Müşteri (Şirket 1)
            Customer.CreateCorporate(
                email: "info@teknolojias.com",
                phoneNumber: "+902124445566",
                passwordHash: "$2a$11$DummyHashedPasswordForSeeding12345",
                companyName: "Nova Yazılım ve Teknoloji A.Ş.",
                taxNumber: "1234567890",
                taxOffice: "Maslak Vergi Dairesi",
                address: "Maslak Mah. Dereboyu Cad. No: 12",
                city: "İstanbul",
                country: "Türkiye",
                createdBy: systemUserId,
                postalCode: "34398"
            ),

            // 5. Kurumsal Müşteri (Şirket 2)
            Customer.CreateCorporate(
                email: "iletisim@anadolulojistik.com",
                phoneNumber: "+903125556677",
                passwordHash: "$2a$11$DummyHashedPasswordForSeeding12345",
                companyName: "Anadolu Lojistik Tic. Ltd. Şti.",
                taxNumber: "9876543210",
                taxOffice: "Ostim Vergi Dairesi",
                address: "Ostim OSB Mah. 1234. Sok. No: 5",
                city: "Ankara",
                country: "Türkiye",
                createdBy: systemUserId,
                postalCode: "06378"
            )
        };

    // Ekstra olarak oluşturulan müşterilere Ehliyet, Findeks veya Doğrulama statüleri eklemek istersek:

    // 1. Müşteriye ehliyet ve findeks ekleyelim (Aktif ve doğrulanmış hale getirelim)
    customers[0].UpdateProfile(
        "Ahmet", "Yılmaz", "12345678901", new DateOnly(1995, 5, 12), "Erkek",
        null, null, null, "+905321112233", "Atatürk Cad. No: 15", "İstanbul", "Türkiye", "34340",
        "B12345678", new DateOnly(2018, 1, 1), "B", systemUserId
    );
    customers[0].VerifyEmail(systemUserId);
    customers[0].VerifyPhone(systemUserId);
    customers[0].UpdateFindeksScore(1100, systemUserId);

    // 2. Müşteriye lüks araç kiralayabilecek düzeyde yüksek findeks ekleyelim
    customers[1].UpdateProfile(
        "Elif", "Demir", "98765432109", new DateOnly(1990, 8, 25), "Kadın",
        null, null, null, "+905332223344", "Tunalı Hilmi Cad. No: 45", "Ankara", "Türkiye", "06680",
        "B87654321", new DateOnly(2012, 6, 15), "B", systemUserId
    );
    customers[1].VerifyEmail(systemUserId);
    customers[1].VerifyPhone(systemUserId);
    customers[1].UpdateFindeksScore(1550, systemUserId); // Lüks kiralama için uygun (>1200)

    // 3. Müşteri (Can Kaya) - Email/Phone doğrulanmamış veya ehliyeti yeni basılmış örnek
    customers[2].UpdateProfile(
        "Can", "Kaya", "45678912345", new DateOnly(2002, 2, 10), "Erkek",
        null, null, null, "+905353334455", "Kıbrıs Şehitleri Cad. No: 88", "İzmir", "Türkiye", "35220",
        "B98765432", new DateOnly(2025, 1, 1), "B", systemUserId // Yeni ehliyet
    );

    // 4. Kurumsal Müşteri 1 onayları
    customers[3].VerifyEmail(systemUserId);
    customers[3].VerifyPhone(systemUserId);
    customers[3].UpdateFindeksScore(1700, systemUserId);

    // 5. Kurumsal Müşteri 2 onayları
    customers[4].VerifyEmail(systemUserId);
    customers[4].VerifyPhone(systemUserId);

    await context.Customers.AddRangeAsync(customers);
    await context.SaveChangesAsync();
  }
}