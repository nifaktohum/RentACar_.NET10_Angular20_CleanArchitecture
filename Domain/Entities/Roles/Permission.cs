using Domain.Abstractions;

namespace Domain.Entities.Roles;

public class Permission
{
  // EF Core için boş constructor kanka
  private Permission() { }

  public Permission(string name, string description)
  {
    Id = Guid.NewGuid();
    SetName(name);
    SetDescription(description);
    IsActive = true;   // (Şalter açık)
    IsDeleted = false; 
  }
  

  public Guid Id { get; private set; }
  public string Name { get; private set; } = default!;
  public string Description { get; private set; } = default!;

  // 🌍 SİSTEM GENELİ DURUMU: Bu izin sistem genelinde kullanımdaysa true, dondurulduysa false.
  public bool IsActive { get; private set; }
  public bool IsDeleted { get; private set; }
  public DateTime? DeletedDate { get; private set; }

  // 🎯 İznin hangi rollerde olduğunu görebilmek için navigation property
  private readonly List<PermissionRole> _permissionRoles = new();
  public IReadOnlyCollection<PermissionRole> PermissionRoles => _permissionRoles.AsReadOnly();

  private void SetName(string name)
  {
    if (string.IsNullOrWhiteSpace(name))
      throw new ArgumentException("İzin adı boş olamaz!");

    if (!name.Contains("."))
      throw new ArgumentException("İzin adı 'Resource.Action' formatında olmalı!");

    Name = name.Trim();
  }

  private void SetDescription(string description)
  {
    if (string.IsNullOrWhiteSpace(description))
      throw new ArgumentException("İzin açıklaması boş olamaz!");
    Description = description;
  }

  public void Update(string name, string description, Guid updatedBy)
  {
    SetName(name);
    SetDescription(description);
  }

  // 🟡 1. Sadece Pasife Çekme (Sistem genelinde bu yetkiyi askıya alır, silmez!)
  public void Deactivate()
  {
    IsActive = false;
  }

  // 🟢 2. Sadece Aktif Etme (Askıdan indirir)
  public void Activate()
  {
    IsActive = true;
  }

  // 🚫 3. Gerçek Soft Delete (Sistemden tamamen arşive kaldırır Joe!)
  public void SoftDelete()
  {
    IsDeleted = true;
    IsActive = false; // Silinen yetki doğal olarak pasif olur kanks
    DeletedDate = DateTime.UtcNow;
  }

  // 🔄 4. Geri Döndürme (Arşivden çıkartır)
  public void RestoreFromSoftDelete()
  {
    IsDeleted = false;
    DeletedDate = null;
    IsActive = true;
  }

  public void AddRoleLink(PermissionRole permissionRole)
  {
    // EF Core'un takıldığı read-only dış koleksiyona değil, 
    // içerideki özgür gerçek listeye doğrudan ekleme yapıyoruz kanks!
    _permissionRoles.Add(permissionRole);
  }

  public void RemoveRoleLink(PermissionRole permissionRole)
  {
    _permissionRoles.Remove(permissionRole);
  }
}