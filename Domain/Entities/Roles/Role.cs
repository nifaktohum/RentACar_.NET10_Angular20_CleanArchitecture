using Domain.Abstractions;

namespace Domain.Entities.Roles;

public sealed class Role
{
  private Role() { }

  public Role(string name, string description)
  {
    Id = Guid.NewGuid(); // BaseEntity kalktığı için Id'yi burada üretiyoruz kanks
    SetName(name);
    SetDescription(description);
  }

  public Guid Id { get; private set; }
  public string Name { get; private set; } = default!; // "Admin", "Manager", "Customer" gibi
  public string Description { get; private set; } = default!; // "Admin", "Manager", "Customer" gibi


  // 🎯 Çoka çok ilişki için RolePermissions köprü listesi
  private readonly List<PermissionRole> _permissionRoles = new();
  public IReadOnlyCollection<PermissionRole> PermissionRoles => _permissionRoles.AsReadOnly();

  private readonly List<UserRole> _userRoles = new();
  public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

  public void SetName(string name)
  {
    // İş Kuralı 1: Boş veya sadece boşluktan oluşan bir rol adı girilemez!
    if (string.IsNullOrWhiteSpace(name))
      throw new ArgumentException("Rol adı boş veya geçersiz olamaz kanka!");

    // İş Kuralı 2: Rol adının başındaki ve sonundaki gereksiz boşlukları temizle
    var cleanedName = name.Trim();

    Name = cleanedName;
  }

  public void SetDescription(string description)
  {
    // Eğer açıklama null gelirse patlamasın diye boş string'e çekip temizliyoruz kanks
    Description = description?.Trim() ?? string.Empty;
  }

  public void AddPermission(Guid permissionId)
  {
    if (_permissionRoles.Any(pr => pr.PermissionId == permissionId)) return;
    _permissionRoles.Add(new PermissionRole(this.Id, permissionId));
  }

  // Statik varsayılan rolleri dönen metot kanks
  public static IEnumerable<Role> GetStaticRoles()
  {
    // Statik rollere de ilk açıklamalarını constructor üzerinden çakıyoruz kanka
    yield return new Role("Admin", "Sistemdeki tüm yetkilere sahip en üst düzey yönetici rolü.")
    {
      Id = Guid.Parse("00000000-0000-0000-0000-000000000001")
    };

    yield return new Role("Customer", "Araç kiralama yapabilen standart mobil ve web müşterisi.")
    {
      Id = Guid.Parse("c0555555-701e-0000-0000-000000000000")
    };
  }

  public void AddPermissionLink(PermissionRole permissionRole)
  {
    _permissionRoles.Add(permissionRole); // Role içindeki private listeye ekleme kanks
  }

  public void RemovePermissionLink(PermissionRole permissionRole)
  {
    _permissionRoles.Remove(permissionRole);
  }
}
