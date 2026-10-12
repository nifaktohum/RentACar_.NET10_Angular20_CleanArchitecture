using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace Infrastructure.Options;

public class RateLimiterSetupOptions : IConfigureOptions<RateLimiterOptions>
{
  public void Configure(RateLimiterOptions _opt)
  {
    // 1. İstek hakkı dolup reddedildiğinde istemciye dönecek ortak HTTP durum kodu (429: Too Many Requests)
    _opt.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // 2. GENEL POLİTİKA (Genel API endpoint'leri için)
    _opt.AddFixedWindowLimiter(policyName: "FixedWindowPolicy", fixedOptions =>
    {
      fixedOptions.PermitLimit = 100; // Maksimum 100 istek hakkı
      fixedOptions.Window = TimeSpan.FromSeconds(10); // 10 saniyelik bir pencere içinde
      fixedOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
      fixedOptions.QueueLimit = 100; // Kontenjan dolarsa, sıraya alınacak maksimum istek sayısı
    });

    // 3. GİRİŞ POLİTİKASI (Brute force koruması - AuthController için)
    _opt.AddFixedWindowLimiter(policyName: "LoginFixedWindowPolicy", fixedOptions =>
    {
      fixedOptions.PermitLimit = 5; // Maksimum 3 istek hakkı
      fixedOptions.Window = TimeSpan.FromMinutes(1); // 1 dakikalık bir pencere içinde (Comment'i düzelttik kanka)
      fixedOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
      fixedOptions.QueueLimit = 0; // Kontenjan dolarsa, sıraya alınacak maksimum istek sayısı
    });

    // Limit aşıldığında dönülecek HTTP durum kodu
    _opt.RejectionStatusCode = StatusCodes.Status429TooManyRequests;


    // 3. ŞİFREMİ UNUTTUM POLİTİKASI (İşte buraya ekliyoruz kanka)
    _opt.AddPolicy("ForgotPasswordFixedWindowPolicy", context =>
    {
      // İsteği atan adamın IP'sini alıyoruz
      var userIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

      return RateLimitPartition.GetFixedWindowLimiter(
              partitionKey: userIp, 
              factory: _ => new FixedWindowRateLimiterOptions
              {
                PermitLimit = 3, // Her IP'ye özel 3 hak tanımlıyoruz
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 1
              });
    });
    

  }
}
