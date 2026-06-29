using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Options;

public sealed class JwtSetupOptions(IConfiguration _config) : IPostConfigureOptions<JwtBearerOptions>
{
  public void PostConfigure(string? name, JwtBearerOptions options)
  {
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

    };
  }
}
