namespace Domain.Abstractions;

// ERİŞİM: Dış katmanların (Application, Infrastructure) bu şablonu kullanabilmesi için sınıfı public yaptık.
public abstract class BaseEntity
{
  public Guid Id { get; private set; }
  public DateTimeOffset CreatedAt { get; private set; }
  public Guid CreatedBy { get; private set; }  
  public DateTimeOffset? UpdatedAt { get; private set; }
  public Guid? UpdatedBy { get; private set; }
  public bool IsActive { get; private set; } = true;
  public bool IsDeleted { get; private set; }
  public DateTimeOffset? DeletedAt { get; private set; }
  public Guid? DeletedBy { get; private set; }


  protected BaseEntity(Guid createdBy)
  {
    Id = Guid.NewGuid();
    CreatedAt = DateTimeOffset.UtcNow;
    CreatedBy = createdBy; // Yaratıcı bilgisi ilk aşamada zorunlu tutulabilir.
  }

  // ALTERNATİF-YAPICI-METOT: Eğer Id'yi harici (örneğin DB'den veya seed datadan) elle geçmek istersen diye aşırı yüklenmiş metot.
  protected BaseEntity(Guid id, Guid createdBy)
  {
    Id = id;
    CreatedAt = DateTimeOffset.UtcNow;
    CreatedBy = createdBy;
  }

  public void Deactivate() => IsActive = false;
  // KONTROL-METODU: Nesneyi kullanıma kapatır (Örn: Bakıma giren veya kiralanan araç pasife çekilebilir).

  public void Activate() => IsActive = true;
  // KONTROL-METODU: Nesneyi tekrar kullanıma açar (Örn: Bakımdan çıkan araç tekrar kiralanabilir duruma gelir).

  // BaseEntity.cs içine ekle
  public void SetActiveStatus(bool isActive) => IsActive = isActive;
  
  public void SetCreateMetadata(Guid userId)
  {
    CreatedAt = DateTimeOffset.UtcNow;
    CreatedBy = userId;
    IsActive = true;
    IsDeleted = false;
  }

  // KONTROL-METODU: Bir kayıt güncellendiğinde zamanı ve güncelleyen kişiyi güvenli şekilde set ederim.
  public void UpdateMetadata(Guid userId)
  {
    UpdatedAt = DateTimeOffset.UtcNow;
    UpdatedBy = userId;
  }

  // KONTROL-METODU: Veritabanından fiziksel olarak silmek yerine kaydı arşivler ve erişime kapatırım.
  public void SoftDelete(Guid userId)
  {
    IsDeleted = true;
    DeletedAt = DateTimeOffset.UtcNow;
    DeletedBy = userId;
    Deactivate(); // Silinen bir kayıt doğal olarak pasif olmalıdır.
  }
}