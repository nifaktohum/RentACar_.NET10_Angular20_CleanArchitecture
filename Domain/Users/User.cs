using Domain.Abstractions;
using Domain.Branchs;
using Domain.Roles;

namespace Domain.Users;

public sealed class User : BaseEntity
{

  private User() : base(Guid.Empty, Guid.Empty) { }
  // // Yeni kullanıcı oluşturma constructor'ı
  public User(
                string firstName,
                string lastName,
                string email,
                string phoneNumber,
                string passwordHash,
                Guid branchId,
                Guid roleId,
                Guid createdBy) : base(createdBy)
  {
    SetFirstName(firstName);
    SetLastName(lastName);
    SetEmail(email);
    SetPhoneNumber(phoneNumber);
    SetPasswordHash(passwordHash);
    SetBranch(branchId);
    AddRole(roleId);
    UpdateSecurityStamp();
  }


  // // --- ÖZELLİKLER (PROPERTIES) ---
  public string FirstName { get; private set; } = default!;
  public string LastName { get; private set; } = default!;
  public string FullName { get; private set; } = default!;
  public string Email { get; private set; } = default!;
  public string PhoneNumber { get; private set; } = default!;
  public string PasswordHash { get; private set; } = default!;

  public Guid? BranchId { get; private set; }
  public Branch? Branch { get; private set; } = default!;

  // // Kullanıcı şifre sıfırlama istediğinde üreteceğimiz 6 haneli geçici kod
  public string? PasswordResetCode { get; set; }
  // // Kullanıcı SMS ile şifre sıfırlama istediğinde üreteceğimiz 6 haneli kod
  public string? SmsResetCode { get; private set; }
  public string SecurityStamp { get; private set; } = default!;


  // 🎯 Kural: Dışarıdan doğrudan eleman eklenmesini engellemek için private field ve ReadOnly property kullanıyoruz.
  private readonly List<UserRole> _userRoles = new();
  public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();


  // // --- DOMAIN METOTLARI (ENCAPSULATION & VALIDATION) ---
  private void SetFirstName(string firstName)
  {
    if (string.IsNullOrWhiteSpace(firstName))
      throw new ArgumentException("Ad boş olamaz");
    FirstName = firstName;
    UpdateFullName(); // // 🔥 GÜNCELLEME: Tekrarlı kodu engellemek için ortak metoda bağladık
  }

  private void SetLastName(string lastName)
  {
    if (string.IsNullOrWhiteSpace(lastName))
      throw new ArgumentException("Soyad boş olamaz");
    LastName = lastName;
    UpdateFullName(); // // 🔥 GÜNCELLEME: Tekrarlı kodu engellemek için ortak metoda bağladık
  }

  private void UpdateFullName()
  {
    FullName = $"{FirstName} {LastName}".Trim();
  }

  private void SetEmail(string email)
  {
    if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
      throw new ArgumentException("Email geçerli değil");
    Email = email;
  }

  private void SetPhoneNumber(string phoneNumber)
  {
    if (string.IsNullOrWhiteSpace(phoneNumber))
      throw new ArgumentException("Telefon boş olamaz");
    PhoneNumber = phoneNumber;
  }

  private void SetPasswordHash(string passwordHash)
  {
    if (string.IsNullOrWhiteSpace(passwordHash))
      throw new ArgumentException("Şifre boş olamaz");
    PasswordHash = passwordHash;
  }

  // --- GÜNCELLEME METOTLARI (İhtiyaç anında dışarıdan çağrılacak public metotlar) ---

  public void UpdateInfo(string firstName, string lastName, string email, string phoneNumber)
  {
    SetFirstName(firstName);
    SetLastName(lastName);
    SetEmail(email);
    SetPhoneNumber(phoneNumber);
    // // 🔥 GÜNCELLEME: Yukarıdaki Set metotları UpdateFullName'i tetiklediği için artık burası da kusursuz çalışacak!
  }

  public void UpdatePassword(string newPasswordHash)
  {
    SetPasswordHash(newPasswordHash); // // Kod tekrarını engellemek için private kuralı tetikliyoruz
    UpdateSecurityStamp(); // 👈 Şifre sıfırlanınca eski tüm cihazları kapı dışarı et!
    // // Güvenlik amacıyla kullanılmış tek kullanımlık kodları sıfırlıyoruz
    PasswordResetCode = null;
    ResetCodeExpires = null;
  }

  public void ChangePassword(string newPasswordHash)
  {
    SetPasswordHash(newPasswordHash);
    UpdateSecurityStamp(); // 👈 Profil içinden şifre değiştirince de tüm cihazlar patlasın!
  }

  public void UpdateSecurityStamp()
  {
    SecurityStamp = Guid.NewGuid().ToString("N"); // "N" formatı çizgileri kaldırıp temiz bir string verir
  }

  // --- TÜM CİHAZLARDAN ÇIKIŞ ---
  public void LogoutAllDevices()
  {
    UpdateSecurityStamp(); // Security Stamp'i yenile → tüm eski token'lar ölür!

    // 🔐 Güvenlik için bekleyen sıfırlama kodlarını da temizle
    PasswordResetCode = null;
    ResetCodeExpires = null;
    SmsResetCode = null;
    SmsResetCodeExpires = null;
  }
  //============> EMAİL <===============//

  // // 🔥 GÜNCELLEME: PostgreSQL UTC hatası vermesin diye arkada private bir field ile zamanı hep UTC'ye zorluyoruz
  private DateTime? _resetCodeExpires;
  public DateTime? ResetCodeExpires
  {
    get => _resetCodeExpires;
    set => _resetCodeExpires = value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null;
  }


  //============> SMS <===============//
  private DateTime? _smsResetCodeExpires;
  public DateTime? SmsResetCodeExpires
  {
    get => _smsResetCodeExpires;
    // // PostgreSQL UTC hatası vermesin diye aynı akıllı lojiği buraya da çaktık!
    set => _smsResetCodeExpires = value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null;
  }

  // // 🔥 SMS Kodu üretildiğinde tetiklenecek Domain Metodu (Encapsulation)
  public void SmsResetCodeGenerate(string code)
  {
    if (string.IsNullOrWhiteSpace(code) || code.Length != 6)
      throw new ArgumentException("Sıfırlama kodu 6 haneli ve geçerli olmalıdır.");

    SmsResetCode = code;
    SmsResetCodeExpires = DateTime.UtcNow.AddMinutes(5); // SMS kodları kurumsal dünyada maksimum 5 dakika geçerlidir!
  }

  // // Güvenlik için şifre başarıyla değiştiğinde SMS kodlarını temizleyecek metot
  public void ClearSmsResetCode()
  {
    SmsResetCode = null;
    SmsResetCodeExpires = null;
  }

  // --- ROL YÖNETİMİ DOMAIN METOTLARI ---
  public void AddRole(Guid roleId)
  {
    // 1. İş Kuralı: Kullanıcıya aynı rol zaten verilmişse tekrar ekleme, içinden çık!
    if (_userRoles.Any(ur => ur.RoleId == roleId)) return;

    // 2. Yeni hafif UserRole nesnesini oluşturuyoruz.
    // İlk parametre bu sınıfın kendi Id'si (this.Id), ikinci parametre gelen roleId.
    _userRoles.Add(new UserRole(this.Id, roleId));

    // 🚀 Güvenlik damgasını güncelliyoruz ki yetki değişince eski token'lar patlasın
    UpdateSecurityStamp();
  }

  // --- ROL GÜNCELLEME (Tek hamlede rolü değiştir) ---
  public void UpdateRoles(List<Guid> newRoleIds)
  {
    _userRoles.Clear();
    foreach (var roleId in newRoleIds)
    {
      _userRoles.Add(new UserRole(this.Id, roleId));
    }
    UpdateSecurityStamp();
  }

  // --- ROL SIFIRLAMA (Update işleminde çok lazım olacak) ---
  // Mevcut tüm rolleri silip sadece birini eklemek için: _userRoles.Clear() yapman gerekir.
  public void ClearRoles()
  {
    if (_userRoles.Any())
    {
      _userRoles.Clear();
      UpdateSecurityStamp();
    }
  }

  public void RemoveRole(Guid roleId)
  {
    // 1. Kullanıcının rollerinin içinden silinmek istenen eşleşmeyi buluyoruz
    var existingRole = _userRoles.FirstOrDefault(ur => ur.RoleId == roleId);

    if (existingRole != null)
    {
      // 🎯 ARTIK SOFT DELETE YOK! İlişkiyi doğrudan içerideki private listeden söküp atıyoruz.
      // EF Core, SaveChanges dendiğinde bu satırı veritabanından DELETE komutuyla kazıyacak.
      _userRoles.Remove(existingRole);

      // 🚀 Rolü alınan kullanıcının eski token'ları patlasın diye stamp'i güncellemeye aynen devam!
      UpdateSecurityStamp();
    }
  }


  // --- BRANCH YÖNETİMİ  ---
  public void SetBranch(Guid branchId)
  {
    if (branchId == Guid.Empty)
      throw new ArgumentException("Kullanıcının şubesi boş olamaz! Geçerli bir şube ID'si girilmelidir.");

    BranchId = branchId;
    UpdateSecurityStamp(); // Şube değişirse kullanıcının token'ları patlasın, yeni şube yetkileriyle login olsun!
  }





}



