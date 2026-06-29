using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Services;
using Domain.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services;

public sealed class JwtProvider(IConfiguration _config) : IJwtProvider
{
  public string CreateToken(User user, List<string> roles, string branchName, List<string> permissions, bool rememberMe = false)
  {
    // 1. Token içine gömülecek kullanıcı bilgilerini (Claims) hazırlıyoruz
    var claims = new List<Claim>
    {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new("Id", user.Id.ToString()),
            new("Email", user.Email),
            new("FullName", user.FullName),
            new("SecurityStamp", user.SecurityStamp),
            new("BranchId", user.BranchId?.ToString() ?? string.Empty),
            new("BranchName", branchName ?? "Merkez")
        };


    // 2. Kullanıcının sahip olduğu tüm rolleri standart .NET ClaimTypes.Role olarak ekliyoruz
    foreach (var role in roles)
    {
      claims.Add(new Claim(ClaimTypes.Role, role));
      claims.Add(new Claim("Role", role));
    }

    // Aynı yetkinin mükerrer eklenmesini engellemek için Distinct atılmış listeyi "Permission" tipiyle claims'e gömüyoruz
    foreach (var permission in permissions.Distinct())
    {
      claims.Add(new Claim("Permission", permission));
    }

    // 2. appsettings.json içerisinden gizli anahtarımızı ve diğer ayarları okuyoruz
    string secretKey = _config["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey bulunamadı.");
    string issuer = _config["Jwt:Issuer"] ?? "RentACarBackend";
    string audience = _config["Jwt:Audience"] ?? "RentACarAngular";
    int expiresInMinutes = int.TryParse(_config["Jwt:ExpiresInMinutes"], out var minutes) ? minutes : 1440;

    // // 4. 🔥 SÜRE UZATMA LOJİĞİ: Eğer 'rememberMe' true geldiyse token ömrünü 7 gün yapıyoruz, 
    // // false geldiyse appsettings içindeki varsayılan dakikayı (örn: 1440 dk) baz alıyoruz kanka.
    DateTime expirationTime = rememberMe
        ? DateTime.UtcNow.AddDays(7)
        : DateTime.UtcNow.AddMinutes(expiresInMinutes);

    // 3. Şifreleme anahtarını (Symmetric Security Key) oluşturuyoruz
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    // 4. Token ayarlarını mühürlüyoruz
    var tokenOptions = new JwtSecurityToken(
        issuer: issuer,
        audience: audience,
        claims: claims,
        expires: expirationTime,
        signingCredentials: credentials);

    // 5. Token'ı string formatına çevirip fırlatıyoruz
    string token = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

    return token;
  }
}
