using System.Reflection;
using Application.Features.Permissions.Queries;
using Application.Services;
using Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Configuration;
using TS.Result;

namespace Application.Behaviors;

// Uygulamana gelen her istek (Command veya Query), iş kodlarının çalışacağı Handler sınıfına gitmeden önce mutlaka bu kapıdan geçmek zorundadır.
public class PermissionBehavior<TRequest, TResponse>(
                                    IUserContext _userContext,
                                    IUserRepository _userRepo,
                                    IConfiguration _config
                              ) : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
  public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken _token)
  {
    // MÜHÜR-KONTROLÜ: Gelen istek sınıfının tepesinde [Permission] attribute'u var mı?
    var securedAttribute = request.GetType().GetCustomAttribute<PermissionAttribute>();

    // Akıllı UserContext üzerinden istek atan kullanıcının ID'sini çekiyoruz
    Guid userId = _userContext.GetUserId();

    // =====================================================================================
    // 🛡️ 1. AŞAMA: OTURUM KONTROLÜ VE GLOBAL GÜVENLİK BARİKATI
    // =====================================================================================
    if (userId != Guid.Empty)
    {
      // Tek bir akıllı sorguyla kullanıcının kendisini, rollerini ve izin ilişkilerini komple çekiyoruz kanks
      var dbUser = await _userRepo.GetUserWithRolesAsync(userId, _token);

      // A. Güvenlik Damgası (SecurityStamp) Kontrolü (Tüm cihazlardan çıkış veya şifre sıfırlama)
      var tokenStamp = _userContext.GetSecurityStamp();

      if (dbUser == null || dbUser.SecurityStamp != tokenStamp)
      {
        // 401 fırlatıp tüm sekmeleri anında nakavt ediyoruz!
        return (TResponse)(dynamic)Result<object>.Failure(statusCode: 401, "Güvenlik damgası geçersiz. Oturumunuz sonlandırıldı.");
      }

      // B. 👑 ADMİN AYRICALIĞI: Eğer giriş yapan kullanıcı Admin ise direkt vizesiz geçsin!
      Guid adminRoleId = Guid.Parse(_config["SeedData:AdminRoleId"]!);
      bool isUserAdmin = dbUser.UserRoles.Any(ur => ur.Role != null &&
                                              ur.Role.Name.Equals("Admin", StringComparison.OrdinalIgnoreCase));


      if (request is GetPermissionsByRoleIdQuery permissionQuery)
      {
        Console.WriteLine($"🔍 [DEBUG] UserId: {userId}, IsAdmin: {isUserAdmin}");
        Console.WriteLine($"🔍 [DEBUG] UserRoles: {string.Join(", ", dbUser.UserRoles.Select(ur => ur.Role?.Name))}");
        permissionQuery.IsAdmin = isUserAdmin;
      }

      if (isUserAdmin)
      {
        return await next(); // Admin her kapıdan geçer, aşağıdaki kontrollerle hiç yorulmasın kanks
      }


      // C. ÖZEL YETKİ KONTROLÜ İÇİN İZİNLERİ HAFIZADA HAZIRLA (Kullanıcı Admin değilse)
      if (securedAttribute != null && !string.IsNullOrWhiteSpace(securedAttribute.PermissionName))
      {
        var userPermissions = dbUser.UserRoles
            .Select(ur => ur.Role)
            .SelectMany(role => role.PermissionRoles)
            .Select(pr => pr.Permission.Name)
            .Distinct()
            .ToList();

        // İstekte talep edilen yetki ("Branches.Create" vb.) kullanıcının havuzunda yoksa 403 çak!
        // if (!userPermissions.Contains(securedAttribute.PermissionName))
        // {
        //   return (TResponse)(dynamic)Result<object>.Failure(statusCode: 403, "Bu işlem için yetkiniz bulunmamaktadır.");
        // }

        // 5. AŞAMA içinde:
        if (!userPermissions.Contains(securedAttribute.PermissionName))
        {
          return CreateFailure(403, "Bu işlem için yetkiniz bulunmamaktadır.");
        }
      }
    }
    else
    {
      // Eğer kullanıcı giriş yapmamışsa (userId == Empty) ama istek üzerinde [Permission] mühürü VARSA:
      if (securedAttribute != null && !string.IsNullOrWhiteSpace(securedAttribute.PermissionName))
      {
        return (TResponse)(dynamic)Result<object>.Failure(statusCode: 401, "Oturum açmanız gerekmektedir.");
      }
    }

    // Tüm barikatları başarıyla geçen elit istekler iş koduna (Handler) uçabilir!
    return await next();
  }

  // Güvenli helper metod:
private static TResponse CreateFailure(int statusCode, string message)
{
    var result = Result<object>.Failure(statusCode: statusCode, message);

    if (result is TResponse typedResponse)
        return typedResponse;

    throw new InvalidOperationException(
        $"PermissionBehavior: TResponse tipi '{typeof(TResponse).Name}', " +
        $"Result<object> ile uyumlu değil...");
}
}



[AttributeUsage(AttributeTargets.Class)]
public sealed class PermissionAttribute : Attribute
{
  public string PermissionName { get; }

  public PermissionAttribute(string permissionName)
  {
    PermissionName = permissionName;
  }
}