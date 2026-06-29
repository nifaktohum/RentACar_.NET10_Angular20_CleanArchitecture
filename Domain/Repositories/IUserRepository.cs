using Domain.Users;
using GenericRepository;

namespace Domain.Repositories;

public interface IUserRepository : IRepository<User>
{
  // HttpContext'ten giriş yapan kullanıcının ID'sini döndürür, yoksa Empty
  Guid GetCurrentUserId();

  // Tüm aktif kullanıcıları Branch ve Rolleriyle birlikte getirir
  Task<List<User>> GetUsersAsync(CancellationToken token);

  // Verilen ID'lere göre kullanıcıların ID ve FullName eşleşmesini döndürür
  Task<Dictionary<Guid, string>> GetUserNamesByIdsAsync(List<Guid> userIds, CancellationToken cancellationToken);

  // Email ile kullanıcıyı ve rollerini getirir, yoksa null
  Task<User?> GetByEmailWithRolesAsync(string email, CancellationToken cancellationToken = default);

  // Kullanıcının SecurityStamp'ini getirir (token geçerlilik kontrolü için)
  Task<string?> GetSecurityStampByIdAsync(Guid userId, CancellationToken cancellationToken = default);

  // Kullanıcıyı ve rollerini getirir
  Task<User?> GetUserWithRolesAsync(Guid userId, CancellationToken cancellationToken);

  // Kullanıcının tüm izinlerini string listesi olarak getirir (Örn: "Branches.Create")
  Task<List<string>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken);

  // Kullanıcının rol isimlerini listeler
  Task<List<string>> GetUserRoleNamesAsync(Guid userId, CancellationToken cancellationToken);

  // Kullanıcı belirtilen role sahip mi kontrol eder
  Task<bool> IsUserInRoleAsync(Guid userId, string roleName, CancellationToken cancellationToken);
  Task<bool> HasRoleAsync(Guid userId, string[] roleNames, CancellationToken cancellationToken);


  // Giriş yapan kullanıcı belirtilen role sahip mi kontrol eder
  Task<bool> IsCurrentUserInRoleAsync(string roleName, CancellationToken cancellationToken);

  // Kullanıcıyı Branch, UserRoles ve Role bilgileriyle birlikte getirir
  Task<User?> GetUserWithDetailsByIdAsync(Guid userId, CancellationToken token = default);

  // Email unique mi kontrol eder (kendi ID'sini hariç tutar)
  Task<bool> IsEmailUniqueExceptUserAsync(string email, Guid userId, CancellationToken token = default);

  // Tüm aktif kullanıcıları Branch ve rollerle birlikte getirir
  Task<List<User>> GetUsersWithDetailsAsync(CancellationToken token = default);
  // Sadece silinmemiş (aktif) kullanıcılar arasında bak!
  Task<bool> IsEmailUniqueAsync(string email, CancellationToken token);
  // BranchId göre kullanıcıları getiren yeni metot
  Task<List<User>> GetUsersByBranchIdAsync(Guid branchId, CancellationToken token = default);
}

/* 

GetCurrentUserId() Metodu ==> 
  Görevi: Halihazırda sisteme giriş yapmış olan kullanıcının ID'sini döndürür. 
  HTTP context'ten JWT token veya cookie içindeki NameIdentifier claim'ini okuyarak kullanıcı ID'sini elde eder. 
  Eğer ID geçersiz veya boşsa Guid.Empty döner.

GetByEmailWithRolesAsync() Metodu ==> 
  Görevi: Verilen email adresine sahip kullanıcıyı,  
  rollerinin detaylarıyla birlikte getirir.
      Önce sadece User tablosundan çıplak veriyi çeker (hafif sorgu)
      Ardından  UserRoles üzerinden Role bilgilerini de dahil ederek eksiksiz bir User nesnesi oluşturur

GetSecurityStampByIdAsync() Metodu ==> 
  Görevi: Belirtilen kullanıcı ID'sine ait SecurityStamp (güvenlik damgası) değerini döndürür. 
  Token doğrulama işlemleri sırasında, kullanıcının rollerinde veya yetkilerinde 
  değişiklik olup olmadığını kontrol etmek için kullanılır. 
  Rol değişikliğinde token'ın geçersiz sayılmasını sağlar.

GetUserNamesByIdsAsync() Metodu ==> 
  Görevi: Birden fazla kullanıcı ID'si verildiğinde, bu ID'lere karşılık gelen kullanıcıların 
  tam adlarını (FullName) bir sözlük (Dictionary) olarak döndürür. 
  Toplu kullanıcı görüntüleme, listelemeler veya drop-down listeler için kullanılır.

GetUserPermissionsAsync() Metodu ==> 
  Görevi: Belirtilen kullanıcıya ait tüm izinleri (permissions) liste olarak döndürür.
  Kullanıcının rollerini bulur. Her rolün sahip olduğu izinleri toplar
  Tekrarlanan izinleri kaldırarak (Distinct) benzersiz izin listesi oluşturur
  Yetkilendirme kontrollerinde temel veri kaynağıdır

GetUserWithRolesAsync() Metodu ==> 
  Görevi: Verilen kullanıcı ID'sine sahip kullanıcıyı, tüm rollerinin detaylarıyla birlikte getirir. 
  GetByEmailWithRolesAsync metodunun email yerine ID'ye göre çalışan versiyonudur. 
  Kullanıcı profil görüntüleme veya rol yönetim ekranları için kullanılır.

GetUserRoleNamesAsync() Metodu ==>
  Görevi: Belirtilen kullanıcının sahip olduğu rollerin isimlerini (Name) listeler. 
  Silinmemiş (!IsDeleted) kullanıcıları baz alır. Kullanıcının hangi rollere sahip olduğunu göstermek 
  veya UI'da rol etiketi oluşturmak için kullanılır.

IsUserInRoleAsync() Metodu ==> 
  Görevi: Bir kullanıcının belirli bir role sahip olup olmadığını kontrol eder. bool sonuç döndürür. 
  Yetkilendirme kontrollerinin temel yapı taşıdır. Örneğin "Bu kullanıcı Admin mi?" sorgusu.

IsCurrentUserInRoleAsync() Metodu ==>
  Görevi: Şu an giriş yapmış olan kullanıcının belirtilen role sahip olup olmadığını kontrol eder. 
  IsUserInRoleAsync metodunu çağırarak, oturum açmış kullanıcı üzerinde rol kontrolü yapar. 
  Action bazlı yetkilendirme kontrollerinde kullanılır (örn: "Sadece Admin'ler bu butonu görebilir").

*/