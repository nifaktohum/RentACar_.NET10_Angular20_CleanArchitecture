using System.Reflection;
using Application.Behaviors;
using Application.Features.Permissions.Commands;

namespace Application.Common.Helpers;

public class PermissionLoader
{
  private static List<string>? _cachedPermissions;
  private static readonly object _lock = new object();
  public static List<string> GetAllPermissions()
  {
    lock (_lock)
    {
      if (_cachedPermissions != null)
        return _cachedPermissions;

      // 🔥 Taramak istediğimiz tüm katmanları (Assembly) bir listeye koyuyoruz Joe!
      var assembliesToScan = new List<Assembly>();

      // 1. Web API Katmanı (Controller'ların olduğu yer)
      var entryAssembly = Assembly.GetEntryAssembly();
      if (entryAssembly != null) assembliesToScan.Add(entryAssembly);

      // 2. Application Katmanı (Command'lerin ve Handler'ların olduğu yer)
      var appAssembly = typeof(ActivatePermissionCommand).Assembly;
      assembliesToScan.Add(appAssembly);

      // Liste çakışmasın diye Distinct koyuyoruz kanks
      var uniqueAssemblies = assembliesToScan.Distinct();

      // 🔥 Tüm katmanlardaki sınıfları birleştirip tek seferde tarıyoruz kanks!
      _cachedPermissions = uniqueAssemblies
          .SelectMany(asm => asm.GetTypes())
          .Where(type => type.GetCustomAttribute<PermissionAttribute>() != null)
          .Select(type => type.GetCustomAttribute<PermissionAttribute>()!.PermissionName)
          .Distinct()
          .OrderBy(p => p)
          .ToList();
    }

    return _cachedPermissions;
  }

  public static bool HasPermission(string permissionName)
  {
    return GetAllPermissions().Contains(permissionName);
  }

  // Opsiyonel: Cache temizleme (örneğin runtime'da yeni attribute eklendiyse)
  public static void ClearCache()
  {
    _cachedPermissions = null;
  }
}
