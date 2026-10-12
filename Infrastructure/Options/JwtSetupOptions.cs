using System.Text;
using Domain.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Options;

public sealed class JwtSetupOptions(IConfiguration _config) : IPostConfigureOptions<JwtBearerOptions>
{
  public void PostConfigure(string? name, JwtBearerOptions options)
  {
    options.MapInboundClaims = false;
    // .NET'in JWT Bearer ayarlarındaki TokenValidationParameters alt nesnesini doldurmaya başlıyoruz kanka
    options.TokenValidationParameters = new TokenValidationParameters
    {
      ValidateIssuer = true,           // Token'ı üreten sunucu kontrol edilsin mi? (Evet)
      ValidateAudience = true,         // Token'ı kullanacak istemci kontrol edilsin mi? (Evet)
      ValidateLifetime = true,         // Token süresi dolmuş mu kontrol edilsin mi? (Evet)
      ValidateIssuerSigningKey = true,   // Güvenlik anahtarı kontrol edilsin mi? (Evet)

      // Ayarlarımızı daha önce oluşturduğumuz appsettings.json mimarisinden çekiyoruz kanka
      ValidIssuer = _config["Jwt:Issuer"],
      ValidAudience = _config["Jwt:Audience"],
      IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]!) // Gizli anahtarı byte dizisine çeviriyoruz
        ),

      ClockSkew = TimeSpan.Zero, // .NET'in varsayılan 5 dakikalık esneme süresini sıfırlıyoruz ki token bittiği an kapı kapansın.
      RoleClaimType = "Role"
    };

    options.Events = new JwtBearerEvents
    {
      OnTokenValidated = async context =>
      {
        // 1. Token'daki bilgileri al
        var userType = context.Principal?.FindFirst("UserType")?.Value;
        var userIdClaim = context.Principal?.FindFirst("Id")?.Value;
        var stampFromToken = context.Principal?.FindFirst("SecurityStamp")?.Value;

        // ⭐ DEBUG LOG (test için)
        Console.WriteLine("=== JWT VALIDATION ===");
        Console.WriteLine($"UserType: {userType}");
        Console.WriteLine($"UserId: {userIdClaim}");
        Console.WriteLine($"StampFromToken: {stampFromToken}");

        // 2. Eksikse token'ı reddet
        if (string.IsNullOrEmpty(userIdClaim) || string.IsNullOrEmpty(stampFromToken))
        {
          Console.WriteLine("❌ Claim eksik!");
          context.Fail("Token geçersiz!");
          return;
        }

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
          Console.WriteLine($"❌ Guid parse hatası: {userIdClaim}");
          context.Fail("Token geçersiz!");
          return;
        }

        // 3. DB'deki güncel stamp'i al
        var serviceProvider = context.HttpContext.RequestServices;
        string? currentStamp;

        if (userType == "Customer")
        {
          // ⭐ Customer token'ı
          var customerRepo = serviceProvider.GetRequiredService<ICustomerRepository>();
          currentStamp = await customerRepo.GetSecurityStampByIdAsync(userId);
          Console.WriteLine($"Customer repo → CurrentStamp: {currentStamp}");
        }
        else
        {
          // ⭐ User (personel) token'ı — UserType yoksa veya "Staff" ise
          var userRepo = serviceProvider.GetRequiredService<IUserRepository>();
          currentStamp = await userRepo.GetSecurityStampByIdAsync(userId);
          Console.WriteLine($"User repo → CurrentStamp: {currentStamp}");
        }

        // 4. Stamp eşleşmiyorsa token'ı reddet
        Console.WriteLine($"Eşleşiyor mu: {currentStamp == stampFromToken}");

        // 4. Stamp eşleşmiyorsa token'ı reddet
        if (currentStamp != stampFromToken)
        {
          Console.WriteLine($"❌ STAMP FARKLI! Token: {stampFromToken}, DB: {currentStamp}");
          context.Fail("Oturumunuz sonlandırılmış. Lütfen tekrar giriş yapın!");

          return;
        }

        Console.WriteLine("✅ STAMP EŞLEŞİYOR!");
      }
    };
  }
}
