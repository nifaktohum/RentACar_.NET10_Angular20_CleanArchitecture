using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using Application.Services;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services;

// Bu sınıf, API'ye istek atan kullanıcının JWT Token'ı içerisindeki benzersiz ID'sini (NameIdentifier) cımbızla çekip alır. 
// ve projedeki diğer katmanların (özellikle Application katmanının) kullanımına sunar.
public sealed class UserContext(IHttpContextAccessor _http) : IUserContext
{
  public Guid GetUserId()
  {
    // 1. HTTP Context null ise (Arka plan görevleri veya testlerde), sistem patlamasın diye güvenle boş Guid dönüyoruz.
    var httpContext = _http.HttpContext;
    if (httpContext is null) return Guid.Empty;

    // 2. Kullanıcının claims listesinden NameIdentifier (User ID) olanı cımbızla çekiyoruz.
    // .FindFirst() metodu FirstOrDefault'a göre bu işlem için daha performanslı ve standardı temsil eder.
    var userIdClaim = httpContext.User.FindFirst("Id");

    // 3. Eğer claim yoksa (Kullanıcı giriş yapmamış, anonim bir istek ise) hata fırlatmak yerine Guid.Empty dönüyoruz.
    if (userIdClaim is null) return Guid.Empty;

    // 4. Eğer bir ID değeri bulunduysa bunu Guid'e çevirmeyi deniyoruz. Başarılıysa ID'yi teslim ediyoruz.
    if (Guid.TryParse(userIdClaim.Value, out Guid id)) return id;

    // 5. Değer var ama Guid değilse, bu bir güvenlik ihlali veya bozuk veri göstergesidir. Burada acımadan hata fırlatıyoruz.
    throw new ArgumentException($"'{userIdClaim.Value}' değeri geçerli bir Guid formatında değil.", nameof(userIdClaim));
  }

  //  * 🔥 YENİ: İstek atan kullanıcının JWT Token'ı içindeki "SecurityStamp" değerini okur.
  //  * Bu değer, kullanıcının o anki oturumunun geçerli olup olmadığını doğrulamak için kullanılır.
  public string? GetSecurityStamp()  {
    // 1. HTTP Context kontrolü
    var httpContext = _http.HttpContext;
    if (httpContext is null) return null;
    // 2. Token içinden bizim JwtProvider ile bastığımız "SecurityStamp" claim'ini yakalıyoruz
    var stampClaim = httpContext.User.FindFirst("SecurityStamp");
    // 3. Eğer claim varsa string değerini döner, yoksa null fırlatır kanka
    return stampClaim?.Value;
  }

  public string GetRoleName()
  {
    var httpContext = _http.HttpContext ?? throw new ArgumentNullException("Context bilgisi bulunamadı..");

    var claims = httpContext.User.Claims;
    string? roleName = claims.FirstOrDefault(b => b.Type == ClaimTypes.Role)?.Value ?? throw new ArgumentNullException("Role bilgisi bulunamadı.."); ;

    return roleName;
  }

  public Guid GetBranchId()
  {
    var httpContext = _http.HttpContext ?? throw new ArgumentNullException("Context bilgisi bulunamadı..");

    var claims = httpContext.User.Claims;
    string? branchId = claims.FirstOrDefault(b => b.Type == "BranchId")?.Value ?? throw new ArgumentNullException("Şube bilgisi bulunamdı..");

    try
    {
      Guid id = Guid.Parse(branchId);
      return id;
    }
    catch (Exception)
    {
      throw new ArgumentNullException("Şubu Id Guid formatı hatası..");
    }
  }
}
