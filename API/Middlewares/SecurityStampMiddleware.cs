using System.Text.Json;
using Application.Services;
using Domain.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace API.Middlewares;


// "Tüm cihazlardan çıkış yap" (Logout from all devices)
public sealed class SecurityStampMiddleware(
                  IUserContext _userContext,
                  IUserRepository _userRepo
) : IMiddleware
{
  public async Task InvokeAsync(HttpContext context, RequestDelegate next)
  {
    // 1. Auth isteklerini, anonim endpoint'leri veya giriş yapmamış istekleri atla
    // if (context.Request.Path.StartsWithSegments("/api/auth")
    //       || context.Request.Path.StartsWithSegments("/api/CustomersAuth")
    //       || context.Request.Path.StartsWithSegments("/api/public")
    //       || context.User.Identity is null
    //       || !context.User.Identity.IsAuthenticated)


    // 1. Endpoint üzerinde [AllowAnonymous] var mı kontrol et
    // Bu sayede Login, Register veya anonim işaretlenmiş TÜM endpoint'ler otomatik muaf olur.
    var endpoint = context.GetEndpoint();
    if (endpoint?.Metadata.GetMetadata<IAllowAnonymous>() != null)
    {
      await next(context);
      return;
    }

    // 2. Token'dan bilgileri al
    var userId = _userContext.GetUserId();
    var tokenStamp = _userContext.GetSecurityStamp();

    if (userId != Guid.Empty && !string.IsNullOrEmpty(tokenStamp))
    {
      // 3. Tokuşturma sorgusu
      var dbUser = await _userRepo.FirstOrDefaultAsync(
          expression: u => u.Id == userId,
          cancellationToken: context.RequestAborted,
          isTrackingActive: false
      );

      // 4. KRİTİK KONTROL
      if (dbUser == null || dbUser.SecurityStamp != tokenStamp)
      {
        context.Response.Headers.Remove("Authorization");
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/json";

        var response = new
        {
          statusCode = 401,
          message = "Oturumunuz sonlandırıldı. Lütfen tekrar giriş yapın.",
          error = "SecurityStampMismatch"
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response), context.RequestAborted);
        return; // 🔥 Zinciri kır!
      }
    }

    await next(context);
  }

}

/* Gerçek Hayat Örneği

    - Kullanıcı şifresini değiştirdiğinde veya bir admin kullanıcının yetkilerini sıfırladığında 
       veritabanındaki SecurityStamp kolonu güncellenir (örn: yeni bir Guid atanır).

    - Kullanıcının telefonunda veya tarayıcısında kalan eski JWT token'ın içinde eski Guid yazılı kalmıştır.

    - Kullanıcı uygulama içinde bir işlem yapmak istediğinde bu middleware araya girer, 
       damgaların uyuşmadığını görür ve eski token'ı anında çöpe atarak kullanıcıyı login sayfasına yönlendirir.
*/
