using Domain.Abstractions;
using Domain.Exceptions;

namespace Domain.Entities.Customers;

public sealed class Customer : BaseEntity
{

  #region // ==================== CONSTRUCTOR =========================
  private Customer() : base(Guid.Empty)
  {
    Type = CustomerType.Individual;
    Email = string.Empty;
    PhoneNumber = string.Empty;
    Address = string.Empty;
    City = string.Empty;
    Country = string.Empty;
    PasswordHash = string.Empty;

    // ⭐ Rental entity'si hazır olunca aç
    // Rentals = new List<Rental>();
  }


  private Customer(
      CustomerType type,
      string email,
      string phoneNumber,
      string passwordHash,
      string address,
      string city,
      string country,
      Guid createdBy
  ) : base(createdBy)
  {
    Type = type;
    Email = email;
    PhoneNumber = phoneNumber;
    PasswordHash = passwordHash;
    Address = address;
    City = city;
    Country = country;

    IsBlacklisted = false;
    IsEmailVerified = false;
    IsPhoneVerified = false;

    // ⭐ Rental entity'si hazır olunca aç
    // Rentals = new List<Rental>();
  }

  #endregion


  #region // ==================== FACTORY METOTLARI ====================
  /// Bireysel müşteri oluşturur
  public static Customer CreateIndividual(
      string email,
      string phoneNumber,
      string passwordHash,
      string firstName,
      string lastName,
      string identityNumber,
      DateOnly? birthDate,
      string address,
      string city,
      string country,
      Guid createdBy,
      string? gender = null,
      string? postalCode = null)
  {
    // ⭐ Validasyon
    if (string.IsNullOrWhiteSpace(email))
      throw new DomainValidationException("Email zorunludur!");

    if (string.IsNullOrWhiteSpace(phoneNumber))
      throw new DomainValidationException("Telefon numarası zorunludur!");

    if (string.IsNullOrWhiteSpace(passwordHash))
      throw new DomainValidationException("Şifre zorunludur!");

    if (string.IsNullOrWhiteSpace(firstName))
      throw new DomainValidationException("Ad zorunludur!");

    if (string.IsNullOrWhiteSpace(lastName))
      throw new DomainValidationException("Soyad zorunludur!");

    if (string.IsNullOrWhiteSpace(identityNumber))
      throw new DomainValidationException("T.C. Kimlik No zorunludur!");

    if (birthDate == default)
      throw new DomainValidationException("Doğum tarihi zorunludur!");

    if (string.IsNullOrWhiteSpace(address))
      throw new DomainValidationException("Adres zorunludur!");

    if (string.IsNullOrWhiteSpace(city))
      throw new DomainValidationException("Şehir zorunludur!");

    if (string.IsNullOrWhiteSpace(country))
      throw new DomainValidationException("Ülke zorunludur!");

    return new Customer(
           CustomerType.Individual,
           email,
           phoneNumber,
           passwordHash,
           address,
           city,
           country,
           createdBy)
    {
      FirstName = firstName,
      LastName = lastName,
      IdentityNumber = identityNumber,
      BirthDate = birthDate,
      Gender = gender,
      PostalCode = postalCode
    };
  }

  /// Kurumsal müşteri oluşturur
  public static Customer CreateCorporate(
      string email,
      string phoneNumber,
      string passwordHash,
      string companyName,
      string taxNumber,
      string taxOffice,
      string address,
      string city,
      string country,
      Guid createdBy,
      string? postalCode = null)
  {
    // ⭐ Validasyon
    if (string.IsNullOrWhiteSpace(email))
      throw new DomainValidationException("Email zorunludur!");

    if (string.IsNullOrWhiteSpace(phoneNumber))
      throw new DomainValidationException("Telefon numarası zorunludur!");

    if (string.IsNullOrWhiteSpace(passwordHash))
      throw new DomainValidationException("Şifre zorunludur!");

    if (string.IsNullOrWhiteSpace(companyName))
      throw new DomainValidationException("Şirket adı zorunludur!");

    if (string.IsNullOrWhiteSpace(taxNumber))
      throw new DomainValidationException("Vergi numarası zorunludur!");

    if (string.IsNullOrWhiteSpace(taxOffice))
      throw new DomainValidationException("Vergi dairesi zorunludur!");

    if (string.IsNullOrWhiteSpace(address))
      throw new DomainValidationException("Adres zorunludur!");

    if (string.IsNullOrWhiteSpace(city))
      throw new DomainValidationException("Şehir zorunludur!");

    if (string.IsNullOrWhiteSpace(country))
      throw new DomainValidationException("Ülke zorunludur!");

    return new Customer(
        CustomerType.Corporate,
        email,
        phoneNumber,
        passwordHash,
        address,
        city,
        country,
        createdBy)
    {
      CompanyName = companyName,
      TaxNumber = taxNumber,
      TaxOffice = taxOffice,
      PostalCode = postalCode
    };
  }

  #endregion


  #region // ==================== PROPERTİES ===========================   

  // ==================== KİMLİK ====================
  public CustomerType Type { get; private set; } // Müşteri tipi (Bireysel / Kurumsal ayrımı yapan Enum)

  // ==================== BİREYSEL ====================
  public string? FirstName { get; private set; } // Müşterinin adı (Bireysel müşteriler için)
  public string? LastName { get; private set; } // Müşterinin soyadı (Bireysel müşteriler için)
  public string? IdentityNumber { get; private set; } // T.C. Kimlik veya Pasaport Numarası
  public DateOnly? BirthDate { get; private set; } // Doğum tarihi (Minimum yaş sınırı kontrolü için)
  public string? Gender { get; private set; } // Cinsiyet
  

  // ==================== KURUMSAL ====================
  public string? CompanyName { get; private set; } // Şirket / Ticari unvan (Kurumsal müşteriler için)
  public string? TaxNumber { get; private set; } // Vergi Kimlik Numarası (VKN)
  public string? TaxOffice { get; private set; } // Bağlı olunan Vergi Dairesi

  // ==================== İLETİŞİM ====================
  public string Email { get; private set; } // E-posta adresi (Sisteme giriş ve faturalandırma için)
  public string PhoneNumber { get; private set; } // Telefon numarası (İletişim ve SMS doğrulamaları için)
  public string Address { get; private set; } // Açık ikamet/fatura adresi
  public string City { get; private set; } // İl / Şehir
  public string Country { get; private set; } // Ülke
  public string? PostalCode { get; private set; } // Posta kodu

  // ==================== SÜRÜCÜ BELGESİ ====================
  public string? DriverLicenseNumber { get; private set; } // Ehliyet / Sürücü belgesi numarası
  public DateOnly? DriverLicenseIssueDate { get; private set; } // Ehliyet veriliş tarihi (Kaç yıllık sürücü olduğunu doğrulamak için)
  public string? DriverLicenseClass { get; private set; } // Ehliyet sınıfı (B, C vb. kiralayabileceği araç segmenti uyumu için)

  // ==================== FİNANSAL ====================
  public int? FindeksScore { get; private set; } // Findeks kredi notu (Lüks araç kiralama ve risk analizi kontrolü için)

  // ==================== AUTH ====================
  public string PasswordHash { get; private set; } // Güvenlik amacıyla hash'lenmiş kullanıcı şifresi
  public string? RefreshToken { get; private set; } // JWT oturumunu uzatmak için kullanılan yenileme anahtarı
  public DateTimeOffset? RefreshTokenExpiresAt { get; private set; } // Refresh token'ın son geçerlilik tarihi
  public string? PasswordResetCode { get; private set; } // Şifre sıfırlama talebinde üretilen doğrulama kodu
  public DateTimeOffset? PasswordResetCodeExpiresAt { get; private set; } // Şifre sıfırlama kodunun geçerlilik süresi
  public string? EmailVerificationCode { get; private set; } // E-posta doğrulama için gönderilen onay kodu
  public DateTimeOffset? EmailVerificationCodeExpiresAt { get; private set; } // E-posta doğrulama kodunun geçerlilik süresi
  public string? PhoneVerificationCode { get; private set; } // SMS doğrulama için gönderilen onay kodu
  public DateTimeOffset? PhoneVerificationCodeExpiresAt { get; private set; } // SMS doğrulama kodunun geçerlilik süresi
  public string? SmsResetCode { get; private set; } // SMS resetleme için gönderilen onay kodu               
  public DateTimeOffset? SmsResetCodeExpiresAt { get; private set; } // SMS resetleme kodunun geçerlilik süresi
  public string? SecurityStamp { get; private set; } // Tüm cihazlardan çıkış

  // ==================== DURUM ====================
  public bool IsBlacklisted { get; private set; } // Müşteri kara listede mi? (Kiralama engeli koymak için)
  public string? BlacklistReason { get; private set; } // Kara listeye alınma gerekçesi / açıklaması
  public bool IsEmailVerified { get; private set; } // E-posta adresinin doğrulanıp doğrulanmadığı bilgisi
  public bool IsPhoneVerified { get; private set; } // Telefon numarasının doğrulanıp doğrulanmadığı bilgisi

  // ==================== İLİŞKİLER ====================
  // public ICollection<Rental> Rentals { get; private set; } = new List<Rental>(); // Navigation Property: Müşterinin tüm kiralama geçmişi (1-N ilişki, Rental entity'si hazır olunca yorum satırı kaldırılacak)
  #endregion


  #region // ==================== DOMAIN METOTLARI =====================
  /// Profil bilgilerini günceller
  public void UpdateProfile(
      string? firstName,
      string? lastName,
      string? identityNumber,
      DateOnly? birthDate,
      string? gender,
      string? companyName,
      string? taxNumber,
      string? taxOffice,
      string phoneNumber,
      string address,
      string city,
      string country,
      string? postalCode,
      string? driverLicenseNumber,
      DateOnly? driverLicenseIssueDate,
      string? driverLicenseClass,
      Guid updatedBy)
  {
    FirstName = firstName;
    LastName = lastName;
    IdentityNumber = identityNumber;
    BirthDate = birthDate;
    Gender = gender;
    CompanyName = companyName;
    TaxNumber = taxNumber;
    TaxOffice = taxOffice;
    PhoneNumber = phoneNumber;
    Address = address;
    City = city;
    Country = country;
    PostalCode = postalCode;
    DriverLicenseNumber = driverLicenseNumber;
    DriverLicenseIssueDate = driverLicenseIssueDate;
    DriverLicenseClass = driverLicenseClass;

    UpdateMetadata(updatedBy);
  }

  /// Findeks skorunu günceller
  public void UpdateFindeksScore(int score, Guid updatedBy)
  {
    if (score < 0 || score > 1900)
      throw new DomainValidationException("Findeks skoru 0-1900 arasında olmalıdır!");

    FindeksScore = score;
    UpdateMetadata(updatedBy);
  }

  /// Şifreyi günceller
  public void UpdatePassword(string newPasswordHash, Guid updatedBy)
  {
    if (string.IsNullOrWhiteSpace(newPasswordHash))
      throw new DomainValidationException("Şifre boş olamaz!");

    PasswordHash = newPasswordHash;
    UpdateMetadata(updatedBy);
  }

  /// Refresh token set eder
  public void SetRefreshToken(string refreshToken, DateTimeOffset expiresAt)
  {
    RefreshToken = refreshToken;
    RefreshTokenExpiresAt = expiresAt;
  }

  /// Refresh token'ı temizler
  public void ClearRefreshToken()
  {
    RefreshToken = null;
    RefreshTokenExpiresAt = null;
  }

  /// Şifre sıfırlama kodu set eder
  public void SetPasswordResetCode(string code, DateTimeOffset expiresAt)
  {
    PasswordResetCode = code;
    PasswordResetCodeExpiresAt = expiresAt;
  }

  /// Şifre sıfırlama kodunu temizler
  public void ClearPasswordResetCode()
  {
    PasswordResetCode = null;
    PasswordResetCodeExpiresAt = null;
  }

  /// Email doğrulama kodu set eder
  public void SetEmailVerificationCode(string code, DateTimeOffset expiresAt)
  {
    EmailVerificationCode = code;
    EmailVerificationCodeExpiresAt = expiresAt;
  }

  /// Email'i doğrulanmış olarak işaretler
  public void VerifyEmail(Guid updatedBy)
  {
    IsEmailVerified = true;
    EmailVerificationCode = null;
    EmailVerificationCodeExpiresAt = null;
    UpdateMetadata(updatedBy);
  }

  /// Telefon doğrulama kodu set eder
  public void SetPhoneVerificationCode(string code, DateTimeOffset expiresAt)
  {
    PhoneVerificationCode = code;
    PhoneVerificationCodeExpiresAt = expiresAt;
  }

  /// Telefonu doğrulanmış olarak işaretler
  public void VerifyPhone(Guid updatedBy)
  {
    IsPhoneVerified = true;
    PhoneVerificationCode = null;
    PhoneVerificationCodeExpiresAt = null;
    UpdateMetadata(updatedBy);
  }

  /// Telefon değiştiğinde doğrulamayı sıfırlar
  public void ResetPhoneVerification()
  {
    IsPhoneVerified = false;
    PhoneVerificationCode = null;
    PhoneVerificationCodeExpiresAt = null;
  }
  /// Telefon doğrulama kodunu temizler
  public void ClearPhoneVerificationCode()
  {
    PhoneVerificationCode = null;
    PhoneVerificationCodeExpiresAt = null;
  }

  // ==================== SMS RESET ====================
  /// SMS şifre sıfırlama kodu set eder
  public void SetSmsResetCode(string code, DateTimeOffset expiresAt)
  {
    SmsResetCode = code;
    SmsResetCodeExpiresAt = expiresAt;
  }

  /// SMS şifre sıfırlama kodunu temizler
  public void ClearSmsResetCode()
  {
    SmsResetCode = null;
    SmsResetCodeExpiresAt = null;
  }

  /// SMS şifre sıfırlama kodunu doğrular (kullanım sonrası temizler)
  public bool VerifySmsResetCode(string code)
  {
    if (string.IsNullOrEmpty(SmsResetCode)) return false;
    if (SmsResetCode != code) return false;
    if (SmsResetCodeExpiresAt is null || SmsResetCodeExpiresAt < DateTimeOffset.UtcNow) return false;

    // Doğrulama başarılı → temizle
    ClearSmsResetCode();
    return true;
  }

  /// Kara listeye ekler
  public void Blacklist(string reason, Guid updatedBy)
  {
    if (string.IsNullOrWhiteSpace(reason))
      throw new DomainValidationException("Kara liste sebebi boş olamaz!");

    IsBlacklisted = true;
    BlacklistReason = reason;
    UpdateMetadata(updatedBy);
  }

  /// Kara listeden çıkarır
  public void RemoveFromBlacklist(Guid updatedBy)
  {
    IsBlacklisted = false;
    BlacklistReason = null;
    UpdateMetadata(updatedBy);
  }

  // ==================== SECURITY STAMP ====================
  /// Yeni SecurityStamp üretir (stamp yoksa veya değiştirilmek istenirse)
  public void UpdateSecurityStamp()
  {
    SecurityStamp = Guid.NewGuid().ToString("N");
  }

  /// Tüm cihazlardan çıkış — stamp'i değiştirir, eski JWT'ler geçersiz olur
  public void LogoutAllDevices()
  {
    SecurityStamp = Guid.NewGuid().ToString("N");
    ClearRefreshToken();      // Refresh token'ı da temizle
    ClearSmsResetCode();      // Bekleyen SMS reset kodunu temizle
    ClearPasswordResetCode(); // Bekleyen email reset kodunu temizle
  }

  #endregion


  #region  // =================== İŞ KURALLARI =========================


  /// Müşterinin yaşını hesaplar
  public int? GetAge()
  {
    if (!BirthDate.HasValue) return null;

    var today = DateOnly.FromDateTime(DateTime.UtcNow);
    var age = today.Year - BirthDate.Value.Year;

    if (BirthDate.Value > today.AddYears(-age)) age--;

    return age;
  }


  /// Geçerli sürücü belgesi var mı? (En az 1 yıllık)
  public bool HasValidDriverLicense()
  {
    if (string.IsNullOrWhiteSpace(DriverLicenseNumber)) return false;
    if (!DriverLicenseIssueDate.HasValue) return false;

    var oneYearAgo = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1));   // ⭐
    return DriverLicenseIssueDate.Value <= oneYearAgo;
  }



  /// Araç kiralayabilir mi? (Genel kontrol)
  public bool CanRent(int minAge = 18)
  {
    // ⭐ Ortak kontroller (her iki tip için)
    if (!IsActive) return false;
    if (IsBlacklisted) return false;
    if (!IsEmailVerified) return false;

    // ⭐ Bireysel müşteri kontrolleri
    if (Type == CustomerType.Individual)
    {
      if (!HasValidDriverLicense()) return false;

      var age = GetAge();
      if (!age.HasValue || age.Value < minAge) return false;
    }
    // ⭐ Kurumsal müşteri kontrolleri
    else if (Type == CustomerType.Corporate)
    {
      if (string.IsNullOrWhiteSpace(CompanyName)) return false;
      if (string.IsNullOrWhiteSpace(TaxNumber)) return false;
      if (string.IsNullOrWhiteSpace(TaxOffice)) return false;
    }

    return true;
  }


  /// Lüks araç kiralayabilir mi? (Findeks skoru kontrolü)
  public bool CanRentLuxury(int minFindeksScore = 1200)
  {
    if (!CanRent()) return false;

    // ⭐ Bireysel müşteri için Findeks ZORUNLU
    if (Type == CustomerType.Individual)
    {
      if (!FindeksScore.HasValue) return false;
      return FindeksScore.Value >= minFindeksScore;
    }

    // ⭐ Kurumsal müşteri için Findeks opsiyonel
    if (Type == CustomerType.Corporate)
    {
      if (FindeksScore.HasValue)
        return FindeksScore.Value >= minFindeksScore;

      return true;
    }

    return false;
  }

  #endregion

}
