using Domain.Abstractions;
using Domain.Entities.Reservations;
using Domain.Exceptions;

namespace Domain.Entities.Vehicles;

public sealed class Vehicle : BaseEntity
{
  // ==================== PRIVATE CONSTRUCTOR (EF Core için) ====================
  private Vehicle() : base(Guid.Empty)
  {
    Brand = string.Empty;
    Model = string.Empty;
    Year = string.Empty;
    Plate = string.Empty;
    Color = string.Empty;
    FuelType = string.Empty;
    Transmission = string.Empty;
    TractionType = string.Empty;
    VinNumber = string.Empty;
    EngineNumber = string.Empty;
    TireStatus = string.Empty;
    GeneralStatus = string.Empty;
    Description = null;
    VehicleModel = null!;
    Images = new List<VehicleImage>();
    Reservations = new List<Reservation>();
  }

  // ==================== PUBLIC CONSTRUCTOR ====================
  public Vehicle(
      string brand,
      string model,
      string year,
      string plate,
      string color,
      Guid vehicleModelId,
      Guid branchId,
      string vinNumber,
      string engineNumber,
      string fuelType,
      string transmission,
      string tractionType,
      decimal engineVolume,
      int enginePower,
      decimal fuelConsumption,
      int seatCount,
      int doorCount,
      int? minAge,
      decimal dailyPrice,
      decimal depositAmount,
      decimal weeklyDiscountRate,
      decimal monthlyDiscountRate,
      int currentKilometer,
      int lastMaintenanceKm,
      int nextMaintenanceKm,
      DateTimeOffset lastMaintenanceDate,
      DateTimeOffset inspectionDate,
      DateTimeOffset insuranceEndDate,
      DateTimeOffset cascoEndDate,
      string tireStatus,
      string generalStatus,
      string? description,
      Guid createdBy,
      bool isActive = true
  ) : base(createdBy)
  {
    SetBrand(brand);
    SetModel(model);
    SetYear(year);
    SetPlate(plate);
    SetColor(color);
    SetVehicleModelId(vehicleModelId);
    SetBranchId(branchId);
    SetVinNumber(vinNumber);
    SetEngineNumber(engineNumber);
    SetFuelType(fuelType);
    SetTransmission(transmission);
    SetTractionType(tractionType);
    SetEngineVolume(engineVolume);
    SetEnginePower(enginePower);
    SetFuelConsumption(fuelConsumption);
    SetSeatCount(seatCount);
    SetDoorCount(doorCount);
    SetMinAge(minAge);
    SetDailyPrice(dailyPrice);
    SetDepositAmount(depositAmount);
    SetWeeklyDiscountRate(weeklyDiscountRate);
    SetMonthlyDiscountRate(monthlyDiscountRate);
    SetKilometer(currentKilometer);
    SetLastMaintenanceKm(lastMaintenanceKm);
    SetNextMaintenanceKm(nextMaintenanceKm);
    SetLastMaintenanceDate(lastMaintenanceDate);
    SetInspectionDate(inspectionDate);
    SetInsuranceEndDate(insuranceEndDate);
    SetCascoEndDate(cascoEndDate);
    SetTireStatus(tireStatus);
    SetGeneralStatus(generalStatus);
    SetDescription(description);

    Status = VehicleStatus.Available;

    VehicleModel = null!;
    Images = new List<VehicleImage>();
    Reservations = new List<Reservation>();

    if (!isActive) Deactivate();
  }


  // ==================== PROPERTIES ====================
  public string Brand { get; private set; } = string.Empty;                    // Araç markası (Örn: BMW, Toyota)
  public string Model { get; private set; } = string.Empty;                    // Araç modeli (Örn: Corolla, 320d)
  public string Year { get; private set; } = string.Empty;                     // Aracın üretim yılı
  public string Plate { get; private set; } = string.Empty;                    // Aracın resmi plakası
  public string Color { get; private set; } = string.Empty;                    // Aracın dış renk bilgisi
  public string FuelType { get; private set; } = string.Empty;                 // Yakıt tipi (Örn: Benzin, Dizel, Hibrit)
  public string Transmission { get; private set; } = string.Empty;             // Vites tipi (Örn: Manuel, Otomatik)
  public string TractionType { get; private set; } = string.Empty;             // Çekiş tipi (Örn: Önden Çeker, 4x4)
  public string VinNumber { get; private set; } = string.Empty;                // Şasi numarası (17 haneli benzersiz VIN)
  public string EngineNumber { get; private set; } = string.Empty;             // Motor seri numarası
  public decimal EngineVolume { get; private set; }                            // Motor hacmi (Örn: 1.6, 2.0)
  public int EnginePower { get; private set; }                                 // Motor gücü (Beygir gücü - HP)
  public decimal FuelConsumption { get; private set; }                         // Ortalama yakıt tüketimi
  public int SeatCount { get; private set; }                                   // Koltuk (yolcu) kapasitesi
  public int DoorCount { get; private set; }                                   // Kapı sayısı
  public int? MinAge { get; private set; }                                     // Bu aracı kiralamak için gereken minimum yaş sınırı
  public decimal DailyPrice { get; private set; }                              // Aracın günlük kiralama bedeli
  public decimal DepositAmount { get; private set; }                           // Araç için alınacak güvence/depozito tutarı
  public decimal WeeklyDiscountRate { get; private set; }                      // Haftalık kiralamalarda uygulanacak indirim yüzdesi
  public decimal MonthlyDiscountRate { get; private set; }                     // Aylık kiralamalarda uygulanacak indirim yüzdesi
  public int CurrentKilometer { get; private set; }                            // Aracın anlık güncel kilometre bilgisi
  public int LastMaintenanceKm { get; private set; }                           // Aracın en son bakım gördüğü kilometre
  public int NextMaintenanceKm { get; private set; }                           // Sonraki bakımın yapılması gereken kilometre sınırı
  public DateTimeOffset LastMaintenanceDate { get; private set; }              // Aracın en son bakım gördüğü tarih
  public DateTimeOffset InspectionDate { get; private set; }                   // Aracın resmi muayene geçerlilik tarihi
  public DateTimeOffset InsuranceEndDate { get; private set; }                 // Zorunlu trafik sigortası bitiş tarihi
  public DateTimeOffset CascoEndDate { get; private set; }                     // Kasko poliçesi bitiş tarihi
  public string TireStatus { get; private set; } = string.Empty;                // Lastiklerin genel durumu
  public string GeneralStatus { get; private set; } = string.Empty;             // Aracın genel fiziksel/kozmetik durumu
  public string? Description { get; private set; }                             // Araç hakkında ek açıklama veya notlar

  public VehicleStatus Status { get; private set; }                            // Aracın anlık operasyonel durumu (Available, Rented, Maintenance vb.)

  // ==================== RELATIONSHIP PROPERTIES ====================
  public Guid VehicleModelId { get; private set; }                             // Bağlı olduğu üst araç modelinin ID'si
  public Guid BranchId { get; private set; }                                   // Aracın kayıtlı olduğu şubenin ID'si
  public VehicleModel VehicleModel { get; private set; } = null!;              // Bağlı olduğu üst araç modelinin navigation property'si
  public ICollection<VehicleImage> Images { get; private set; } = new List<VehicleImage>();          // Araca ait yüklenen görsellerin koleksiyonu
  public ICollection<Reservation> Reservations { get; private set; } = new List<Reservation>();     // Bu araca ait geçmiş ve aktif rezervasyonların listesi

  // ==================== COMPUTED PROPERTIES ====================

  /// Araç şu an operasyonel olarak kiraya verilebilir mi?
  /// (Tarih bazlı müsaitlik için IsAvailableForPeriod() kullanılmalı.)
  public bool IsAvailable => Status == VehicleStatus.Available
                             && IsActive && !IsDeleted;

  /// Bakım zamanı gelmiş mi? (Güncel km, sonraki bakım km'sini geçmişse true)
  public bool IsMaintenanceDue => CurrentKilometer >= NextMaintenanceKm;

  /// Aracın yasal olarak kiraya verilebilirliği. Tarih parametresi almaz;
  /// "şu an" (UTC) baz alınır. Farklı tarih için IsLegallyRentable(DateTimeOffset) kullan.
  public bool IsLegallyCompliantNow => IsLegallyRentable(DateTimeOffset.UtcNow);

  /// <summary>
  /// Araç operasyonel olarak rezervasyon alabilir mi? (ANLIK durum)
  /// - Maintenance / OutOfService → HAYIR
  /// - Rented → EVET (gelecek tarih için rezervasyon alınabilir)
  /// - Available → EVET
  /// 
  /// Tarih bazlı müsaitlik için IsAvailableForPeriod kullanılmalı.
  /// </summary>
  public bool CanBeReservedNow => IsActive && !IsDeleted
      && Status != VehicleStatus.Maintenance
      && Status != VehicleStatus.OutOfService;

  // ==================== DOMAIN METHODS ====================

  public void UpdateDetails(
      string brand,
      string model,
      string year,
      string plate,
      string color,
      Guid vehicleModelId,
      Guid branchId,
      string vinNumber,
      string engineNumber,
      string fuelType,
      string transmission,
      string tractionType,
      decimal engineVolume,
      int enginePower,
      decimal fuelConsumption,
      int seatCount,
      int doorCount,
      int? minAge,
      decimal dailyPrice,
      decimal depositAmount,
      decimal weeklyDiscountRate,
      decimal monthlyDiscountRate,
      int lastMaintenanceKm,
      int nextMaintenanceKm,
      DateTimeOffset lastMaintenanceDate,
      DateTimeOffset inspectionDate,
      DateTimeOffset insuranceEndDate,
      DateTimeOffset cascoEndDate,
      string tireStatus,
      string generalStatus,
      string? description
  )
  {
    if (Status == VehicleStatus.Rented)
      throw new DomainValidationException("Kiradaki aracın bilgileri değiştirilemez!");

    SetBrand(brand);
    SetModel(model);
    SetYear(year);
    SetPlate(plate);
    SetColor(color);
    SetVehicleModelId(vehicleModelId);
    SetBranchId(branchId);
    SetVinNumber(vinNumber);
    SetEngineNumber(engineNumber);
    SetFuelType(fuelType);
    SetTransmission(transmission);
    SetTractionType(tractionType);
    SetEngineVolume(engineVolume);
    SetEnginePower(enginePower);
    SetFuelConsumption(fuelConsumption);
    SetSeatCount(seatCount);
    SetDoorCount(doorCount);
    SetMinAge(minAge);
    SetDailyPrice(dailyPrice);
    SetDepositAmount(depositAmount);
    SetWeeklyDiscountRate(weeklyDiscountRate);
    SetMonthlyDiscountRate(monthlyDiscountRate);
    SetLastMaintenanceKm(lastMaintenanceKm);
    SetNextMaintenanceKm(nextMaintenanceKm);
    SetLastMaintenanceDate(lastMaintenanceDate);
    SetInspectionDate(inspectionDate);
    SetInsuranceEndDate(insuranceEndDate);
    SetCascoEndDate(cascoEndDate);
    SetTireStatus(tireStatus);
    SetGeneralStatus(generalStatus);
    SetDescription(description);
  }


  // ==================== KİRALAMA YAŞAM DÖNGÜSÜ ====================
  public void Rent()
  {
    EnsureActive();

    if (Status != VehicleStatus.Available)
      throw new DomainValidationException(
          $"Araç kiraya uygun değil! Mevcut durum: {Status}");

    Status = VehicleStatus.Rented;
    // TODO: İlerleyen aşamada işlemi yapan personelin ID'si (processedBy) 
    // buraya parametre olarak alınıp audit (izlenebilirlik) loglarına eklenecek.

  }

  // iade alma
  public void Return(int returnKilometer)
  {
    // sadece kiradaki bir aracın iade alınabilir.
    if (Status != VehicleStatus.Rented)
      throw new DomainValidationException("Araç kirada değil, iade alınamaz!");

    if (returnKilometer < CurrentKilometer)
      throw new DomainValidationException(
          $"İade kilometresi ({returnKilometer}) mevcut kilometreden ({CurrentKilometer}) küçük olamaz!");

    CurrentKilometer = returnKilometer;
    Status = VehicleStatus.Available;

    // TODO: Aracı iade alan personelin ID'si (processedBy) ve audit loglaması eklenecek.
  }

  public void SendToMaintenance()
  {
    EnsureActive();

    if (Status == VehicleStatus.Rented)
      throw new DomainValidationException("Kiradaki araç bakıma alınamaz!");

    if (Status == VehicleStatus.Maintenance)
      throw new DomainValidationException("Araç zaten bakımda!");

    Status = VehicleStatus.Maintenance;
  }

  public void ReturnFromMaintenance()
  {
    if (Status != VehicleStatus.Maintenance)
      throw new DomainValidationException("Araç bakımda değil!");

    Status = VehicleStatus.Available;
  }

  public void SendToOutOfService()
  {
    if (Status == VehicleStatus.Rented)
      throw new DomainValidationException("Kiradaki araç operasyon dışına alınamaz!");

    Status = VehicleStatus.OutOfService;

    // TODO: Aracı bakıma gönderen personelin ID'si (processedBy) ve audit loglaması eklenecek.
  }

  public void ReturnFromOutOfService()
  {
    if (Status != VehicleStatus.OutOfService)
      throw new DomainValidationException("Araç operasyon dışında değil!");

    Status = VehicleStatus.Available;

    // TODO: Aracı tekrar operasyona alan personelin ID'si (processedBy) ve audit loglaması eklenecek.
  }

  // ==================== MÜSAİTLİK KONTROLLERİ ====================

  /// <summary>
  /// Belirtilen tarih aralığında araç kiralanabilir mi? (TARİH BAZLI)
  /// 3 katmanlı kilit:
  ///   - PendingPayment (15 dk checkout hold)
  ///   - Confirmed + sadece depozito (24 saat)
  ///   - Confirmed + tam ödeme / Active (tarih aralığı boyunca)
  /// 
  /// NOT: Bu metot anlık Vehicle.Status'a BAKMAZ. Araç şu an Rented olsa bile
  /// gelecekteki boş tarih aralıkları için true dönebilir.
  /// Anlık operasyonel uygunluk için CanBeReservedNow kullanılmalı.
  /// </summary>
  public bool IsAvailableForPeriod(DateTimeOffset startDate, DateTimeOffset endDate)
  {
    if (startDate >= endDate)
      throw new DomainValidationException("Başlangıç tarihi bitiş tarihinden önce olmalı!");

    // Araç aktif ve silinmemiş olmalı (anlık Status'a BAKMA)
    if (!IsActive || IsDeleted)
      return false;

    var now = DateTimeOffset.UtcNow;

    return !Reservations.Any(r =>
        r.IsActive && !r.IsDeleted &&
        IsBlockingStatus(r, now) &&
        r.PlannedStartDate < endDate &&
        r.PlannedEndDate > startDate);
  }

  /// <summary>
  /// Bu rezervasyon aracı kilitler mi? 3 katmanlı kilit mantığı.
  /// </summary>
  private static bool IsBlockingStatus(Reservation r, DateTimeOffset now)
  {
    // ── Katman 3: Tam ödeme yapıldı veya araç teslim edildi → her zaman bloklar
    if (r.Status == ReservationStatus.Active)
      return true;

    if (r.Status == ReservationStatus.Confirmed && r.FullPaymentPaid)
      return true;

    // ── Katman 2: Depozito ödendi, tam ödeme yok → 24 saat dolmadıysa bloklar
    if (r.Status == ReservationStatus.Confirmed
        && !r.FullPaymentPaid
        && r.DepositLockDeadline.HasValue
        && r.DepositLockDeadline.Value > now)
      return true;

    // ── Katman 1: PendingPayment → 15 dk checkout hold dolmadıysa bloklar
    if (r.Status == ReservationStatus.PendingPayment
        && r.PaymentDeadline > now)
      return true;

    return false;
  }

  public bool CanBeRentedBy(int customerAge)
  {
    if (customerAge <= 0)
      throw new DomainValidationException("Müşteri yaşı geçersiz!");

    return MinAge == null || customerAge >= MinAge.Value;
  }

  // ==================== YASAL UYGUNLUK & BAKIM ====================
  public bool IsLegallyRentable(DateTimeOffset today)
  {
    return InspectionDate >= today
        && InsuranceEndDate >= today
        && CascoEndDate >= today;
  }

  public bool NeedsMaintenance()
  {
    return CurrentKilometer >= NextMaintenanceKm;
  }

  public void EnsureReadyForRent(DateTimeOffset today, int customerAge)
  {
    EnsureActive();

    if (Status != VehicleStatus.Available)
      throw new DomainValidationException($"Araç kiraya uygun değil! Durum: {Status}");

    if (!IsLegallyRentable(today))
      throw new DomainValidationException("Aracın muayene/sigorta/kasko süresi geçmiş!");

    if (NeedsMaintenance())
      throw new DomainValidationException("Aracın bakım zamanı gelmiş!");

    if (!CanBeRentedBy(customerAge))
      throw new DomainValidationException(
          $"Müşteri yaşı yetersiz! Min: {MinAge}");
  }

  // ==================== REZERVASYON YÖNETİMİ ====================

  public void AddReservation(Reservation reservation)
  {
    if (reservation == null)
      throw new ArgumentNullException(nameof(reservation));

    if (reservation.VehicleId != Id)
      throw new DomainValidationException("Rezervasyon bu araca ait değil!");

    if (!IsAvailableForPeriod(reservation.PlannedStartDate, reservation.PlannedEndDate))
      throw new DomainValidationException(
          "Araç belirtilen tarih aralığında müsait değil!");

    Reservations.Add(reservation);
  }

  // ==================== SETTER'LAR (VALIDASYONLU) ====================

  public void SetBrand(string brand)
  {
    if (string.IsNullOrWhiteSpace(brand))
      throw new DomainValidationException("Marka boş olamaz!", nameof(brand));
    if (brand.Length > 50)
      throw new DomainValidationException("Marka 50 karakterden uzun olamaz!", nameof(brand));
    Brand = brand.Trim();
  }

  public void SetModel(string model)
  {
    if (string.IsNullOrWhiteSpace(model))
      throw new DomainValidationException("Model boş olamaz!", nameof(model));
    if (model.Length > 50)
      throw new DomainValidationException("Model 50 karakterden uzun olamaz!", nameof(model));
    Model = model.Trim();
  }

  public void SetYear(string year)
  {
    if (string.IsNullOrWhiteSpace(year))
      throw new DomainValidationException("Yıl boş olamaz!", nameof(year));
    if (!int.TryParse(year, out var y) || y < 1950 || y > DateTimeOffset.UtcNow.Year + 1)
      throw new DomainValidationException("Geçersiz yıl!", nameof(year));
    Year = year.Trim();
  }

  public void SetPlate(string plate)
  {
    if (string.IsNullOrWhiteSpace(plate))
      throw new DomainValidationException("Plaka boş olamaz!", nameof(plate));
    if (plate.Length > 20)
      throw new DomainValidationException("Plaka 20 karakterden uzun olamaz!", nameof(plate));
    Plate = plate.Trim().ToUpperInvariant();
  }

  public void SetColor(string color)
  {
    if (string.IsNullOrWhiteSpace(color))
      throw new DomainValidationException("Renk boş olamaz!", nameof(color));
    Color = color.Trim();
  }

  public void SetVehicleModelId(Guid vehicleModelId)
  {
    if (vehicleModelId == Guid.Empty)
      throw new DomainValidationException("VehicleModelId boş olamaz!", nameof(vehicleModelId));
    VehicleModelId = vehicleModelId;
  }

  public void SetBranchId(Guid branchId)
  {
    if (branchId == Guid.Empty)
      throw new DomainValidationException("BranchId boş olamaz!", nameof(branchId));
    BranchId = branchId;
  }

  public void SetVinNumber(string vinNumber)
  {
    if (string.IsNullOrWhiteSpace(vinNumber))
      throw new DomainValidationException("Şasi numarası boş olamaz!", nameof(vinNumber));
    if (vinNumber.Length != 17)
      throw new DomainValidationException("Şasi numarası 17 karakter olmalı!", nameof(vinNumber));
    VinNumber = vinNumber.Trim().ToUpperInvariant();
  }

  public void SetEngineNumber(string engineNumber)
  {
    if (string.IsNullOrWhiteSpace(engineNumber))
      throw new DomainValidationException("Motor numarası boş olamaz!", nameof(engineNumber));
    if (engineNumber.Length > 30)
      throw new DomainValidationException("Motor numarası 30 karakterden uzun olamaz!", nameof(engineNumber));
    EngineNumber = engineNumber.Trim().ToUpperInvariant();
  }

  public void SetFuelType(string fuelType)
  {
    if (string.IsNullOrWhiteSpace(fuelType))
      throw new DomainValidationException("Yakıt tipi boş olamaz!", nameof(fuelType));
    FuelType = fuelType.Trim();
  }

  public void SetTransmission(string transmission)
  {
    if (string.IsNullOrWhiteSpace(transmission))
      throw new DomainValidationException("Vites tipi boş olamaz!", nameof(transmission));
    Transmission = transmission.Trim();
  }

  public void SetTractionType(string tractionType)
  {
    if (string.IsNullOrWhiteSpace(tractionType))
      throw new DomainValidationException("Çekiş tipi boş olamaz!", nameof(tractionType));
    TractionType = tractionType.Trim();
  }

  public void SetEngineVolume(decimal engineVolume)
  {
    if (engineVolume <= 0)
      throw new DomainValidationException("Motor hacmi 0'dan büyük olmalı!", nameof(engineVolume));
    EngineVolume = engineVolume;
  }

  public void SetEnginePower(int enginePower)
  {
    if (enginePower <= 0)
      throw new DomainValidationException("Motor gücü 0'dan büyük olmalı!", nameof(enginePower));
    EnginePower = enginePower;
  }

  public void SetFuelConsumption(decimal fuelConsumption)
  {
    if (fuelConsumption <= 0)
      throw new DomainValidationException("Yakıt tüketimi 0'dan büyük olmalı!", nameof(fuelConsumption));
    FuelConsumption = fuelConsumption;
  }

  public void SetSeatCount(int seatCount)
  {
    if (seatCount <= 0 || seatCount > 60)
      throw new DomainValidationException("Koltuk sayısı 1-60 arasında olmalı!", nameof(seatCount));
    SeatCount = seatCount;
  }

  public void SetDoorCount(int doorCount)
  {
    if (doorCount <= 0 || doorCount > 10)
      throw new DomainValidationException("Kapı sayısı 1-10 arasında olmalı!", nameof(doorCount));
    DoorCount = doorCount;
  }

  public void SetMinAge(int? minAge)
  {
    if (minAge.HasValue && (minAge.Value < 18 || minAge.Value > 80))
      throw new DomainValidationException("Min yaş 18-80 arasında olmalı!", nameof(minAge));
    MinAge = minAge;
  }

  public void SetDailyPrice(decimal dailyPrice)
  {
    if (dailyPrice <= 0)
      throw new DomainValidationException("Günlük fiyat 0'dan büyük olmalı!", nameof(dailyPrice));
    DailyPrice = dailyPrice;
  }

  public void SetDepositAmount(decimal depositAmount)
  {
    if (depositAmount < 0)
      throw new DomainValidationException("Depozito negatif olamaz!", nameof(depositAmount));
    DepositAmount = depositAmount;
  }

  public void SetWeeklyDiscountRate(decimal rate)
  {
    if (rate < 0 || rate > 100)
      throw new DomainValidationException("Haftalık indirim oranı 0-100 arasında olmalı!", nameof(rate));
    WeeklyDiscountRate = rate;
  }

  public void SetMonthlyDiscountRate(decimal rate)
  {
    if (rate < 0 || rate > 100)
      throw new DomainValidationException("Aylık indirim oranı 0-100 arasında olmalı!", nameof(rate));
    MonthlyDiscountRate = rate;
  }

  public void SetKilometer(int kilometer)
  {
    if (kilometer < 0)
      throw new DomainValidationException("Kilometre negatif olamaz!", nameof(kilometer));
    if (kilometer < CurrentKilometer)
      throw new DomainValidationException("Kilometre geriye alınamaz!", nameof(kilometer));
    CurrentKilometer = kilometer;
  }

  public void SetLastMaintenanceKm(int km)
  {
    if (km < 0)
      throw new DomainValidationException("Son bakım km'si negatif olamaz!", nameof(km));
    if (km > CurrentKilometer)
      throw new DomainValidationException("Son bakım km'si mevcut km'den büyük olamaz!", nameof(km));
    LastMaintenanceKm = km;
  }

  public void SetNextMaintenanceKm(int km)
  {
    if (km < 0)
      throw new DomainValidationException("Sonraki bakım km'si negatif olamaz!", nameof(km));
    if (km <= LastMaintenanceKm)
      throw new DomainValidationException("Sonraki bakım km'si son bakım km'sinden büyük olmalı!", nameof(km));
    NextMaintenanceKm = km;
  }

  public void SetLastMaintenanceDate(DateTimeOffset date)
  {
    if (date > DateTimeOffset.UtcNow)
      throw new DomainValidationException("Son bakım tarihi gelecekte olamaz!", nameof(date));
    LastMaintenanceDate = date;
  }

  public void SetInspectionDate(DateTimeOffset date)
  {
    InspectionDate = date;
  }

  public void SetInsuranceEndDate(DateTimeOffset date)
  {
    InsuranceEndDate = date;
  }

  public void SetCascoEndDate(DateTimeOffset date)
  {
    CascoEndDate = date;
  }

  public void SetTireStatus(string tireStatus)
  {
    if (string.IsNullOrWhiteSpace(tireStatus))
      throw new DomainValidationException("Lastik durumu boş olamaz!", nameof(tireStatus));
    TireStatus = tireStatus.Trim();
  }

  public void SetGeneralStatus(string generalStatus)
  {
    if (string.IsNullOrWhiteSpace(generalStatus))
      throw new DomainValidationException("Genel durum boş olamaz!", nameof(generalStatus));
    GeneralStatus = generalStatus.Trim();
  }

  public void SetDescription(string? description)
  {
    if (description?.Length > 1000)
      throw new DomainValidationException("Açıklama 1000 karakterden uzun olamaz!", nameof(description));
    Description = description?.Trim();
  }

  // ==================== YARDIMCI METOTLAR ====================

  private void EnsureActive()
  {
    if (!IsActive || IsDeleted)
      throw new DomainValidationException("Araç aktif değil veya silinmiş!");
  }

  public void SetVehicleModel(VehicleModel vehicleModel)
  {
    if (vehicleModel == null)
      throw new ArgumentNullException(nameof(vehicleModel));

    VehicleModel = vehicleModel;
    SetVehicleModelId(vehicleModel.Id);  // validasyonlu
  }

  // ==================== IMAGE METHODS ====================

  public string? GetMainImageUrl()
  {
    return Images.FirstOrDefault(i => i.IsMain && i.IsActive && !i.IsDeleted)?.ImageUrl
           ?? Images.Where(i => i.IsActive && !i.IsDeleted)
                   .OrderBy(i => i.DisplayOrder)
                   .FirstOrDefault()?.ImageUrl;
  }

  public List<string> GetAllImageUrls()
  {
    return Images
        .Where(i => i.IsActive && !i.IsDeleted)
        .OrderBy(i => i.DisplayOrder)
        .Select(i => i.ImageUrl)
        .ToList();
  }

  public VehicleImage AddImage(string imageUrl, bool isMain = false, string? description = null)
  {
    if (string.IsNullOrWhiteSpace(imageUrl))
      throw new DomainValidationException("Resim URL'si boş olamaz!");

    if (isMain)
    {
      foreach (var img in Images.Where(i => i.IsMain && i.IsActive && !i.IsDeleted))
        img.SetAsNonMain();
    }

    var image = new VehicleImage(
        vehicleId: Id,
        imageUrl: imageUrl,
        displayOrder: Images.Count(i => i.IsActive && !i.IsDeleted) + 1,
        isMain: isMain,
        description: description,
        createdBy: CreatedBy
    );

    Images.Add(image);
    return image;
  }

  public void RemoveImage(Guid imageId)
  {
    var image = Images.FirstOrDefault(i => i.Id == imageId);
    if (image != null && image.IsActive && !image.IsDeleted)
      image.SoftDelete(CreatedBy);
  }

  public void SetMainImage(Guid imageId)
  {
    var newMain = Images.FirstOrDefault(i => i.Id == imageId);
    if (newMain == null || !newMain.IsActive || newMain.IsDeleted)
      throw new DomainValidationException("Geçersiz resim!");

    foreach (var img in Images.Where(i => i.IsActive && !i.IsDeleted))
      img.SetAsNonMain();

    newMain.SetAsMain();
  }

  public void ClearImages()
  {
    foreach (var img in Images.Where(i => i.IsActive && !i.IsDeleted))
      img.SoftDelete(CreatedBy);
  }
}