using Domain.Abstractions;
using Domain.Entities.Branchs;
using Domain.Entities.Customers;
using Domain.Entities.Extras;
using Domain.Entities.Vehicles;
using Domain.Exceptions;

namespace Domain.Entities.Reservations;

public sealed class Reservation : BaseEntity
{
  // ==================== PRIVATE CONSTRUCTOR (EF Core için) ====================
  private Reservation() : base(Guid.Empty)
  {
    ReservationNumber = string.Empty;
    Notes = null;
    AdminNotes = null;
    CancellationReason = null;
    Customer = null!;
    Vehicle = null!;
    PickupBranch = null!;
    ReturnBranch = null;
    Extras = new List<RentalExtra>();
  }

  // ==================== PUBLIC CONSTRUCTOR ====================
  public Reservation(
      string reservationNumber,
      Guid customerId,
      Guid vehicleId,
      Guid pickupBranchId,
      Guid? returnBranchId,
      DateTimeOffset plannedStartDate,
      DateTimeOffset plannedEndDate,
      decimal dailyPrice,
      string? notes,
      Guid createdBy
  ) : base(createdBy)
  {
    // ---------- Validasyonlar ----------
    if (string.IsNullOrWhiteSpace(reservationNumber))
      throw new DomainValidationException("Rezervasyon numarası boş olamaz!", nameof(reservationNumber));

    if (plannedStartDate >= plannedEndDate)
      throw new DomainValidationException("Başlangıç tarihi bitiş tarihinden önce olmalıdır!", nameof(plannedStartDate));

    if (plannedStartDate < DateTimeOffset.UtcNow.AddHours(-1))
      throw new DomainValidationException("Başlangıç tarihi geçmişte olamaz!", nameof(plannedStartDate));

    if (dailyPrice <= 0)
      throw new DomainValidationException("Günlük fiyat 0'dan büyük olmalıdır!", nameof(dailyPrice));

    // ---------- Atamalar ----------
    ReservationNumber = reservationNumber;
    CustomerId = customerId;
    VehicleId = vehicleId;
    PickupBranchId = pickupBranchId;
    ReturnBranchId = returnBranchId;
    PlannedStartDate = plannedStartDate;
    PlannedEndDate = plannedEndDate;
    DailyPrice = dailyPrice;          // Snapshot
    Notes = notes;

    // ---------- Hesaplamalar ----------
    PlannedDays = (int)Math.Ceiling((plannedEndDate - plannedStartDate).TotalDays);
    if (PlannedDays < 1) PlannedDays = 1;

    TotalPrice = DailyPrice * PlannedDays;          // Ekstralar hariç toplam planlanan tutar
    DepositAmount = CalculateDeposit(TotalPrice);   // %20 Kapora tutarı
    RemainingAmount = TotalPrice - DepositAmount;   // Ödenecek kalan tutar

    // ---------- Durum ----------
    Status = ReservationStatus.PendingPayment;            // İlk oluşturulduğunda ödeme beklenir
    PaymentDeadline = DateTimeOffset.UtcNow.AddHours(24); // 24 saat ödeme süresi
    DepositPaid = false;

    // ---------- Navigation ----------
    Customer = null!;
    Vehicle = null!;
    PickupBranch = null!;
    ReturnBranch = null;
    Extras = new List<RentalExtra>();
  }

  // ==================== PROPERTIES ====================

  // ---------- Kimlik ----------
  public string ReservationNumber { get; private set; }

  // ---------- İlişkiler ----------
  public Guid CustomerId { get; private set; }        /// Rezervasyonu yapan müşterinin eşsiz kimliği (Foreign Key).
  public Customer Customer { get; private set; }      /// Müşteriye ait detay verilerini barındıran navigation property.

  public Guid VehicleId { get; private set; }         /// Kiralanan aracın eşsiz kimliği (Foreign Key).
  public Vehicle Vehicle { get; private set; }        /// Araca ait detay verilerini barındıran navigation property.

  public Guid PickupBranchId { get; private set; }    /// Aracın teslim alınacağı (çıkış yapılacak) şubenin kimliği (Foreign Key).
  public Branch PickupBranch { get; private set; }    /// Teslim alma şubesine ait detay verileri

  public Guid? ReturnBranchId { get; private set; }   /// Aracın iade edileceği şubenin kimliği (Opsiyonel olabilir, farklı şuraya bırakma senaryosu).
  public Branch? ReturnBranch { get; private set; }   /// İade şubesine ait detay verileri (Nullable).

  // ---------- Planlanan Tarihler ----------
  public DateTimeOffset PlannedStartDate { get; private set; }  /// Rezervasyonun planlanan başlangıç (teslim alma) tarihi ve saati.
  public DateTimeOffset PlannedEndDate { get; private set; }    /// Rezervasyonun planlanan bitiş (iade) tarihi ve saati.
  public int PlannedDays { get; private set; }                  /// Başlangıç ve bitiş tarihleri arasında hesaplanan toplam planlanan gün sayısı

  // ---------- Fiili Tarihler ----------
  public DateTimeOffset? ActualPickupAt { get; private set; }   /// Müşterinin aracı şubeden fiziksel olarak teslim aldığı gerçek zaman (Aktifleştirildiğinde dolar).
  public DateTimeOffset? ActualReturnAt { get; private set; }   /// Müşterinin aracı şubeye iade ettiği gerçek zaman (Tamamlandığında dolar). 

  // ---------- Fiyatlandırma ----------
  public decimal DailyPrice { get; private set; }              // Kiralama anındaki aracın günlük fiyatı. /// İleride aracın kataloğundaki fiyat zamlansa bile bu rezervasyonun fiyatı etkilenmesin diye "Snapshot" olarak saklanır.
  public decimal TotalPrice { get; private set; }              // Planlanan toplam tutar
  public decimal? BillablePrice { get; private set; }          // Araç fiilen iade edildikten sonra (erken/geç iade durumlarına göre) net hesaplanan fatura tutarı.
  public decimal DepositAmount { get; private set; }           // Rezervasyonun onaylanması için alınması gereken güvence bedeli / kapora (%20).
  public decimal RemainingAmount { get; private set; }         // Aracın teslim alınacağı gün şubede ödenecek kalan tutar (%80)

  // Extraların toplam fiyatını anlık hesaplayan property (EF veritabanına kolon olarak yansıtmaz)
  public decimal ExtrasTotalPrice => Extras.Sum(e => e.TotalPrice);

  // ---------- Ödeme ----------
  public bool DepositPaid { get; private set; }               /// Kaporanın ödenip ödenmediğini belirten bayrak (flag)
  public DateTimeOffset? DepositPaidAt { get; private set; }  /// Kaporanın başarıyla tahsil edildiği tarih ve saat.
  public string? PaymentId { get; private set; }              /// Ödeme sağlayıcısından (örn. sanal pos) dönen işlem / ödeme referans ID'si.

  // ---------- Durum ve Süreler ----------
  public ReservationStatus Status { get; private set; }         /// Rezervasyonun anlık durumunu belirten Enum (PendingPayment, Confirmed, Active, Completed, Cancelled vb.).
  public DateTimeOffset PaymentDeadline { get; private set; }   /// Müşterinin kaporayı yatırması için tanınan son süre (Genellikle Oluşturulma + 24 saat).
  public DateTimeOffset? PickupDeadline { get; private set; }   /// Onaylanan rezervasyonda aracın teslim alınması gereken son süre ("No-show" takibi için Kapora + 24 saat).  
  public DateTimeOffset? ConfirmedAt { get; private set; }      /// Rezervasyonun kapora ödenerek onaylandığı tarih ve saat.
  public DateTimeOffset? CompletedAt { get; private set; }      /// Kiralama işleminin başarıyla tamamlanıp aracın teslim alındığı tarih ve saat.
  public DateTimeOffset? CancelledAt { get; private set; }      /// Rezervasyonun iptal edildiği tarih ve saat.
  public string? CancellationReason { get; private set; }       /// Rezervasyon iptal edildiyse girilen iptal sebebi açıklaması.

  // ---------- Extralar ----------
  public ICollection<RentalExtra> Extras { get; private set; } = new List<RentalExtra>();

  // ---------- Notlar ----------
  public string? Notes { get; private set; }          /// Müşteri tarafından rezervasyon aşamasında bırakılan genel notlar.
  public string? AdminNotes { get; private set; }     /// Sadece şirket çalışanlarının/adminlerin görebileceği dahili notlar.

  // ==================== DOMAIN METOTLARI ====================

  /// Kapora hesaplama — %20
  private static decimal CalculateDeposit(decimal totalPrice)
  {
    return Math.Round(totalPrice * 0.20m, 2);
  }


  /// Kapora yatırıldı → Durum 'Confirmed' (Onaylı) aşamasına geçer.
  public void MarkDepositPaid(string paymentId, Guid updatedBy)
  {
    if (Status != ReservationStatus.PendingPayment)
      throw new DomainValidationException("Sadece 'PendingPayment' durumundaki rezervasyon onaylanabilir!");

    if (DepositPaid)
      throw new DomainValidationException("Kapora zaten ödenmiş!");

    if (DateTimeOffset.UtcNow > PaymentDeadline)
      throw new DomainValidationException("Ödeme süresi dolmuş!");

    if (string.IsNullOrWhiteSpace(paymentId))
      throw new DomainValidationException("Ödeme ID boş olamaz!", nameof(paymentId));

    Status = ReservationStatus.Confirmed;
    DepositPaid = true;
    DepositPaidAt = DateTimeOffset.UtcNow;
    ConfirmedAt = DateTimeOffset.UtcNow;
    PickupDeadline = DateTimeOffset.UtcNow.AddHours(24);
    PaymentId = paymentId;

    UpdateMetadata(updatedBy);
  }

  /// Araç müşteriye teslim edildi → Durum 'Active' aşamasına geçer.
  public void MarkAsActive(DateTimeOffset actualPickupAt, Guid updatedBy)
  {
    // Henüz onaylanmamış ya da zaten iptal edilmiş/tamamlanmış bir rezervasyonun 
    // aktifleştirilmesini engelliyor. Yanlış state geçişlerinin önüne geçmek
    // Confirmed ==> Kapora alındı, araç kilitli, teslim bekleniyor (24 saat)
    if (Status != ReservationStatus.Confirmed)
      throw new DomainValidationException("Sadece 'Onaylı' durumdaki rezervasyon aktifleştirilebilir!");

    // Müşteri aracı teslim almaya çok geç geldiyse ("no-show" durumu) veya 
    // belirlenen tolerans süresi aşıldıysa sistemin koruma kalkanı devreye giriyor.
    if (PickupDeadline.HasValue && DateTimeOffset.UtcNow > PickupDeadline.Value)
      throw new DomainValidationException("Teslim alma süresi dolmuş!");

    // Fiziksel olarak henüz yaşanmamış bir zaman diliminin "araç teslim alındı" saati olarak girilmesini engelliyor
    if (actualPickupAt > DateTimeOffset.UtcNow)
      throw new DomainValidationException("Teslim alma zamanı gelecekte olamaz!");

    Status = ReservationStatus.Active;
    ActualPickupAt = actualPickupAt;

    UpdateMetadata(updatedBy);
  }

  /// Araç iade alındı → Durum 'Completed' olur ve fiili fatura tutarı (BillablePrice) hesaplanır.
  public void MarkAsCompleted(DateTimeOffset actualReturnAt, Guid updatedBy)
  {
    if (Status != ReservationStatus.Active)
      throw new DomainValidationException("Sadece 'Active' durumdaki rezervasyon tamamlanabilir!");

    if (ActualPickupAt is null)
      throw new DomainValidationException("Teslim alma zamanı set edilmemiş!");

    if (actualReturnAt > DateTimeOffset.UtcNow)
      throw new DomainValidationException("İade zamanı gelecekte olamaz!");

    if (actualReturnAt < ActualPickupAt.Value)
      throw new DomainValidationException("İade zamanı teslim alma zamanından önce olamaz!");

    Status = ReservationStatus.Completed;
    ActualReturnAt = actualReturnAt;
    CompletedAt = DateTimeOffset.UtcNow;

    // Erken iadelerde bile planlanan bitiş tarihi baz alınabilir veya fiili süre hesaplanır
    var start = ActualPickupAt.Value;
    var end = actualReturnAt < PlannedEndDate ? PlannedEndDate : actualReturnAt;

    var billableDays = (int)Math.Ceiling((end - start).TotalDays);
    if (billableDays < 1) billableDays = 1;

    var vehiclePrice = DailyPrice * billableDays;
    var extrasTotal = Extras.Sum(e => e.TotalPrice);
    BillablePrice = vehiclePrice + extrasTotal;

    UpdateMetadata(updatedBy);
  }

  /// 24 saat içinde kapora yatırılmadıysa → Durum 'Expired' (Zaşımı) olur.
  public void MarkAsExpired(Guid updatedBy)
  {
    if (Status != ReservationStatus.PendingPayment)
      throw new DomainValidationException("Sadece 'PendingPayment' durumdaki rezervasyon expire olabilir!");

    if (DateTimeOffset.UtcNow < PaymentDeadline)
      throw new DomainValidationException("Ödeme süresi henüz dolmamış!");

    Status = ReservationStatus.Expired;
    UpdateMetadata(updatedBy);
  }

  /// Onaylandığı halde 24 saat içinde araç teslim alınmadıysa → Durum 'NoShow' (Gelmedi) olur.
  public void MarkAsNoShow(Guid updatedBy)
  {
    if (Status != ReservationStatus.Confirmed)
      throw new DomainValidationException("Sadece 'Confirmed' durumdaki rezervasyon NoShow olabilir!");

    if (PickupDeadline is null)
      throw new DomainValidationException("Teslim alma süresi set edilmemiş!");

    if (DateTimeOffset.UtcNow < PickupDeadline.Value)
      throw new DomainValidationException("Teslim alma süresi henüz dolmamış!");

    Status = ReservationStatus.NoShow;
    UpdateMetadata(updatedBy);
  }

  /// Rezervasyon iptal işlemi (Aktif durumdayken iptal edilemez, önce iade alınmalıdır).
  public void Cancel(string reason, Guid updatedBy)
  {
    if (Status == ReservationStatus.Cancelled)
      throw new DomainValidationException("Rezervasyon zaten iptal edilmiş!");

    if (Status == ReservationStatus.Completed)
      throw new DomainValidationException("Tamamlanmış rezervasyon iptal edilemez!");

    if (Status == ReservationStatus.Active)
      throw new DomainValidationException("Aktif rezervasyon iptal edilemez! Önce iade alınmalı.");

    if (string.IsNullOrWhiteSpace(reason))
      throw new DomainValidationException("İptal sebebi zorunludur!", nameof(reason));

    Status = ReservationStatus.Cancelled;
    CancellationReason = reason;
    CancelledAt = DateTimeOffset.UtcNow;

    UpdateMetadata(updatedBy);
  }

  /// Yönetici notunu günceller.
  public void SetAdminNotes(string? adminNotes, Guid updatedBy)
  {
    AdminNotes = adminNotes;
    UpdateMetadata(updatedBy);
  }

  //// Rezervasyona ek ürün (bebek koltuğu vb.) ekler ve toplam tutarları tetikler
  public void AddExtra(RentalExtra extra)
  {
    if (extra == null)
      throw new ArgumentNullException(nameof(extra));

    if (Extras.Any(e => e.ExtraId == extra.ExtraId))
      throw new DomainValidationException("Bu ekstra zaten eklenmiş!");

    Extras.Add(extra);
    RecalculateTotalPrice();
  }

  /// Ekstra eklendiğinde fiyatları güncelleyen özel yardımcı metot.
  private void RecalculateTotalPrice()
  {
    var extrasTotal = Extras.Sum(e => e.TotalPrice);
    TotalPrice = (DailyPrice * PlannedDays) + extrasTotal;
    DepositAmount = CalculateDeposit(TotalPrice);
    RemainingAmount = TotalPrice - DepositAmount;
  }

  // ==================== İŞ KURALLARI ====================

  /// Araç şu an fiziksel olarak rezerve/kilitli mi? (Onaylı veya Aktif durumdaysa True döner)
  public bool IsVehicleLocked()
  {
    return Status == ReservationStatus.Confirmed ||
           Status == ReservationStatus.Active;
  }

  /// Rezervasyon iptal edilebilir durumda mı?
  public bool CanBeCancelled()
  {
    return Status == ReservationStatus.PendingPayment ||
           Status == ReservationStatus.Confirmed;
  }

  /// Kapora ödemesi alınıp onaylanabilir mi?
  public bool CanBeConfirmed()
  {
    return Status == ReservationStatus.PendingPayment &&
           DateTimeOffset.UtcNow <= PaymentDeadline;
  }

  /// İade sonrasında oluşan fiyat farkını (Fiili - Planlanan) hesaplar.
  public decimal GetPriceDifference()
  {
    if (BillablePrice is null) return 0;
    return BillablePrice.Value - TotalPrice;
  }

}


/*

  Bu Entity'de Bilinçli Olarak Yapmadıklarımız

  Konu	                                  Neden
  Uzatma metodu (Extend)	                Sonra ekleyeceğiz, şimdilik yok
  Payment entity'si	                      Payment gateway yok, mock ödeme
  Vehicle.Rent() çağrısı	                Service katmanında yapılacak, domain'de değil
  VehicleModel.RentVehicle() çağrısı	    Service katmanında yapılacak


*/