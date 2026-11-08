using Domain.Abstractions;
using Domain.Entities.Branchs;
using Domain.Entities.Customers;
using Domain.Entities.Extras;
using Domain.Entities.Vehicles;
using Domain.Exceptions;

namespace Domain.Entities.Reservations;

public sealed class Reservation : BaseEntity
{
  // ==================== SABİTLER ====================
  private const int PaymentHoldMinutes = 15;   // Katman 1: Checkout hold
  private const int DepositLockHours = 24;     // Katman 2: Depozito sonrası tam ödeme süresi

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

    TotalPrice = DailyPrice * PlannedDays;
    DepositAmount = CalculateDeposit(TotalPrice);
    RemainingAmount = TotalPrice - DepositAmount;

    // ---------- Durum (Katman 1: 15 dk checkout hold) ----------
    Status = ReservationStatus.PendingPayment;
    PaymentDeadline = DateTimeOffset.UtcNow.AddMinutes(PaymentHoldMinutes);

    DepositPaid = false;
    FullPaymentPaid = false;

    // ---------- Navigation ----------
    Customer = null!;
    Vehicle = null!;
    PickupBranch = null!;
    ReturnBranch = null;
    Extras = new List<RentalExtra>();
  }

  // ==================== PROPERTIES ====================

  public string ReservationNumber { get; private set; }

  public Guid CustomerId { get; private set; }
  public Customer Customer { get; private set; }

  public Guid VehicleId { get; private set; }
  public Vehicle Vehicle { get; private set; }

  public Guid PickupBranchId { get; private set; }
  public Branch PickupBranch { get; private set; }

  public Guid? ReturnBranchId { get; private set; }
  public Branch? ReturnBranch { get; private set; }

  public DateTimeOffset PlannedStartDate { get; private set; }
  public DateTimeOffset PlannedEndDate { get; private set; }
  public int PlannedDays { get; private set; }

  public DateTimeOffset? ActualPickupAt { get; private set; }
  public DateTimeOffset? ActualReturnAt { get; private set; }

  public decimal DailyPrice { get; private set; }
  public decimal TotalPrice { get; private set; }
  public decimal? BillablePrice { get; private set; }
  public decimal DepositAmount { get; private set; }
  public decimal RemainingAmount { get; private set; }

  public decimal ExtrasTotalPrice => Extras.Sum(e => e.TotalPrice);

  // ---------- Ödeme: Depozito (Katman 2) ----------
  public bool DepositPaid { get; private set; }
  public DateTimeOffset? DepositPaidAt { get; private set; }
  public string? PaymentId { get; private set; }

  /// Depozito sonrası 24 saatlik tam ödeme süresi. Tam ödeme yapılınca NULL'lanır.
  public DateTimeOffset? DepositLockDeadline { get; private set; }

  // ---------- Ödeme: Tam Ödeme (Katman 3) ----------
  public bool FullPaymentPaid { get; private set; }

  public DateTimeOffset? FullPaymentPaidAt { get; private set; }
  public string? FullPaymentId { get; private set; }

  // ---------- İptal ve İade Yönetimi ----------
  public bool? IsDepositRefunded { get; private set; }     // true: Tamamen iade, false: Yandı/İade yok, null: Henüz iptal yok
  public decimal? RefundedAmount { get; private set; }     // İade edilen tutar (Kısmi iade için)
  public decimal? ForfeitedAmount { get; private set; }    // Yanan tutar (Şirkette kalan)

  // ---------- Durum ve Süreler ----------
  public ReservationStatus Status { get; private set; }

  /// Katman 1: Checkout hold (15 dk)
  public DateTimeOffset PaymentDeadline { get; private set; }

  public DateTimeOffset? ConfirmedAt { get; private set; }
  public DateTimeOffset? CompletedAt { get; private set; }
  public DateTimeOffset? CancelledAt { get; private set; }
  public string? CancellationReason { get; private set; }

  // ---------- Extralar ----------
  public ICollection<RentalExtra> Extras { get; private set; } = new List<RentalExtra>();

  // ---------- Notlar ----------
  public string? Notes { get; private set; }
  public string? AdminNotes { get; private set; }

  // ==================== DOMAIN METOTLARI ====================

  public void SetVehicle(Vehicle vehicle)
  {
    if (vehicle == null) throw new ArgumentNullException(nameof(vehicle));
    if (vehicle.Id != VehicleId) throw new DomainValidationException("Araç ID uyuşmuyor!");
    Vehicle = vehicle;
  }

  /// Kapora hesaplama — %20
  private static decimal CalculateDeposit(decimal totalPrice)
  {
    return Math.Round(totalPrice * 0.20m, 2);
  }

  /// Depozito yatırıldı → Durum 'Confirmed'. 24 saatlik tam ödeme süresi başlar (Katman 2).
  public void MarkDepositPaid(string paymentId, Guid updatedBy)
  {
    // 1. Durum kontrolü
    if (Status != ReservationStatus.PendingPayment)
      throw new DomainValidationException(
          $"Rezervasyon ödeme bekleniyor durumunda değil! Mevcut durum: {Status}");

    // 2. Katman 1: 15 dk checkout hold dolmuş mu?
    if (DateTimeOffset.UtcNow > PaymentDeadline)
      throw new DomainValidationException(
          "Ödeme süresi dolmuş! Rezervasyon iptal edilmelidir. Lütfen yeni rezervasyon oluşturun.");

    if (DepositPaid)
      throw new DomainValidationException("Kapora zaten ödenmiş!");

    if (string.IsNullOrWhiteSpace(paymentId))
      throw new DomainValidationException("Ödeme ID boş olamaz!", nameof(paymentId));

    var now = DateTimeOffset.UtcNow;

    // 3. Ödeme bilgileri
    Status = ReservationStatus.Confirmed;
    DepositPaid = true;
    DepositPaidAt = now;
    ConfirmedAt = now;
    PaymentId = paymentId;

    // 4. Katman 2: 24 saatlik tam ödeme süresi (depozito anından itibaren)
    DepositLockDeadline = now.AddHours(DepositLockHours);

    UpdateMetadata(updatedBy);
  }


  /// 24 saatlik depozito kilidi artık geçersiz.
  /// Tam ödeme yapıldı → Durum 'Confirmed' olur (eğer değilse) ve tarih aralığı hard-lock (Katman 3) başlar.
  /// Müşteri direkt tam ödeme yaparsa depozito aşamasını atlar.
  public void MarkFullPaymentPaid(string fullPaymentId, Guid updatedBy)
  {
    // 1. Zaten tam ödeme yapılmış mı?
    if (FullPaymentPaid)
      throw new DomainValidationException("Tam ödeme zaten yapılmış!");

    // 2. Süresi dolmuş mu? (Eğer PendingPayment ise 15 dk kuralı, Confirmed ise 24 saat kuralı)
    var now = DateTimeOffset.UtcNow;

    if (Status == ReservationStatus.PendingPayment && now > PaymentDeadline)
      throw new DomainValidationException("Ödeme süresi dolmuş! Rezervasyon iptal edilmelidir.");

    if (Status == ReservationStatus.Confirmed && !FullPaymentPaid && DepositLockDeadline.HasValue && now > DepositLockDeadline.Value)
      throw new DomainValidationException("Depozito sonrası ödeme süresi dolmuş! Rezervasyon iptal edilmelidir.");

    // 3. Sadece PendingPayment (direkt tam ödeme) veya Confirmed (depozito sonrası tam ödeme) durumunda kabul et
    if (Status != ReservationStatus.PendingPayment && Status != ReservationStatus.Confirmed)
      throw new DomainValidationException($"Mevcut durumda ({Status}) tam ödeme yapılamaz!");

    if (string.IsNullOrWhiteSpace(fullPaymentId))
      throw new DomainValidationException("Tam ödeme ID boş olamaz!", nameof(fullPaymentId));

    // 4. Tam ödeme bilgileri
    FullPaymentPaid = true;
    FullPaymentPaidAt = now;
    FullPaymentId = fullPaymentId;

    // 5. Eğer direkt tam ödeme yapıldıysa (PendingPayment idiyse) depozito atlanır, Confirmed'a geçer
    if (Status == ReservationStatus.PendingPayment)
    {
      Status = ReservationStatus.Confirmed;
      DepositPaid = true; // Tam ödeme yapıldığı için depozito ödenmiş sayılır
      DepositPaidAt = now;
      ConfirmedAt = now;
      PaymentId = fullPaymentId; // Depozito PaymentId'sini de buraya yazıyoruz
    }

    // 6. 24 saatlik süre (varsa) iptal edilir, tarih aralığı kilidi (Katman 3) devreye girer
    DepositLockDeadline = null;

    UpdateMetadata(updatedBy);
  }
  /// Araç müşteriye teslim edildi → Durum 'Active'.
  /// Tam ödeme yapılmadan araç teslim edilemez.

  public void MarkAsActive(DateTimeOffset actualPickupAt, Guid updatedBy)
  {
    // 1. Durum Kontrolü
    if (Status != ReservationStatus.Confirmed)
      throw new DomainValidationException("Sadece 'Onaylı' durumdaki rezervasyon aktifleştirilebilir!");

    // 2. Tam Ödeme Kontrolü
    if (!FullPaymentPaid)
      throw new DomainValidationException("Tam ödeme yapılmadan araç teslim edilemez!");

    // 3. Gelecek Tarih / Saat Kontrolü (Gelecekte teslim alınamaz)
    if (actualPickupAt > DateTimeOffset.UtcNow)
      throw new DomainValidationException("Teslim alma zamanı gelecekte olamaz!");

    // 4. Erken Teslim Kontrolü (Planlanan tarihten en fazla 1 gün önce olabilir)
    if (actualPickupAt < PlannedStartDate.AddDays(-1))
      throw new DomainValidationException(
          $"Teslim alma zamanı planlanan başlangıç tarihine çok uzak! " +
          $"Planlanan: {PlannedStartDate:dd.MM.yyyy HH:mm}, Girilen: {actualPickupAt:dd.MM.yyyy HH:mm}");

    // 5. Geçmiş Tarih Kontrolü (Tarih bazlı - saat farkı takıntısı olmadan en fazla 7 gün öncesi)
    var today = DateOnly.FromDateTime(DateTime.UtcNow);
    var pickupDate = DateOnly.FromDateTime(actualPickupAt.DateTime);
    var dayDifference = today.DayNumber - pickupDate.DayNumber;

    if (dayDifference > 7)
      throw new DomainValidationException(
          $"Teslim alma zamanı çok eski! En fazla 7 gün öncesi olabilir. Girilen: {actualPickupAt:dd.MM.yyyy HH:mm}");

    if (dayDifference < 0)
      throw new DomainValidationException("Teslim alma zamanı gelecekte olamaz!");

    // 6. Atamalar
    Status = ReservationStatus.Active;
    ActualPickupAt = actualPickupAt;

    // 7. Audit
    UpdateMetadata(updatedBy);
  }

  /// Araç iade alındı → Durum 'Completed'. BillablePrice hesaplanır.
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

    var start = ActualPickupAt.Value;

    // 1. FATURA HESABI (A Şıkkı: Erken iade edilse bile Planlanan Bitiş Tarihi üzerinden hesapla)
    var billableEnd = actualReturnAt < PlannedEndDate ? PlannedEndDate : actualReturnAt;
    var billableDays = (int)Math.Ceiling((billableEnd - start).TotalDays);
    if (billableDays < 1) billableDays = 1;

    var vehiclePrice = DailyPrice * billableDays;
    var extrasTotal = Extras.Sum(e => e.TotalPrice);
    BillablePrice = vehiclePrice + extrasTotal;

    // 2. ERKEN İADE KİLİDİNİ KIR (Aracı kalan günler için serbest bırak)
    // Eğer müşteri planlanan tarihten önce iade ettiyse, planlanan bitiş tarihini fiili iade tarihine çekiyoruz.
    // Böylece ConflictingReservationsSpecification bu rezervasyonu "bitmiş" sayar ve kalan günleri başkasına kiralayabiliriz.
    if (actualReturnAt < PlannedEndDate)
    {
      PlannedEndDate = actualReturnAt;
    }

    UpdateMetadata(updatedBy);
  }
  /// 15 dk checkout hold doldu → Durum 'Expired'. (Katman 1)
  public void MarkAsExpired(Guid updatedBy)
  {
    if (Status != ReservationStatus.PendingPayment)
      throw new DomainValidationException("Sadece 'PendingPayment' durumdaki rezervasyon expire olabilir!");

    if (DateTimeOffset.UtcNow < PaymentDeadline)
      throw new DomainValidationException("Ödeme süresi henüz dolmamış!");

    Status = ReservationStatus.Expired;

    UpdateMetadata(updatedBy);
  }

  /// Depozito yatırıldı ama 24 saat içinde tam ödeme yapılmadı → Durum 'Cancelled'.
  /// Depozito yanar (iade edilmez). (Katman 2)
  public void MarkAsCancelledAfterDeposit(Guid updatedBy)
  {
    if (Status != ReservationStatus.Confirmed)
      throw new DomainValidationException(
          "Sadece 'Confirmed' durumdaki rezervasyon bu şekilde iptal edilebilir!");

    if (FullPaymentPaid)
      throw new DomainValidationException("Tam ödeme yapılmış rezervasyon bu şekilde iptal edilemez!");

    if (!DepositLockDeadline.HasValue)
      throw new DomainValidationException("Depozito kilidi set edilmemiş!");

    if (DateTimeOffset.UtcNow < DepositLockDeadline.Value)
      throw new DomainValidationException("Depozito süresi henüz dolmamış!");

    Status = ReservationStatus.Cancelled;
    CancellationReason = "Depozito süresi doldu — tam ödeme yapılmadı. Depozito iade edilmez.";
    CancelledAt = DateTimeOffset.UtcNow;
    DepositLockDeadline = null;

    // DepositPaid = true KALIR → depozito yandı, muhasebe/raporlama için görünür.
    // FullPaymentPaid = false KALIR → tam ödeme hiç yapılmadı.

    UpdateMetadata(updatedBy);
  }

  /// Rezervasyon iptali (Aktif/Tamamlanmış iptal edilemez).
  /// İptal işlemini gerçekleştirir ve iade tutarlarını belirler.
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

    // 🚨 İADE HESABI
    if (DepositPaid && (Status == ReservationStatus.Confirmed)) // Sadece depozito ödenmişse ve araç teslim edilmemişse
    {
      var refundPercentage = CalculateRefundPercentage();

      RefundedAmount = Math.Round(DepositAmount * refundPercentage, 2);
      ForfeitedAmount = DepositAmount - RefundedAmount;

      IsDepositRefunded = refundPercentage > 0;

      if (refundPercentage == 1.0m)
        reason = $"{reason} (Depozito tam iade edildi)";
      else if (refundPercentage == 0.5m)
        reason = $"{reason} (Kısmi iade: %50, Yanan: {ForfeitedAmount} TL)";
      else
        reason = $"{reason} (Depozito yandı - Son 24 saat)";
    }
    else
    {
      // Eğer depozito ödenmemişse iade hesabı yapılmaz
      IsDepositRefunded = false;
      RefundedAmount = 0;
      ForfeitedAmount = 0;
    }

    Status = ReservationStatus.Cancelled;
    CancellationReason = reason;
    CancelledAt = DateTimeOffset.UtcNow;

    DepositLockDeadline = null;

    UpdateMetadata(updatedBy);
  }

  /// Planlanan başlangıç tarihine kalan süreye göre iade oranını hesaplar.
  /// Dönen değer: 1.0 (Tam iade), 0.5 (Kısmi iade), 0.0 (İade yok)
  public decimal CalculateRefundPercentage()
  {
    if (!DepositPaid) return 0m; // Depozito ödenmemişse iade yok

    var timeUntilStart = PlannedStartDate - DateTimeOffset.UtcNow;
    var hoursUntilStart = timeUntilStart.TotalHours;

    if (hoursUntilStart >= 48) return 1.0m;  // %100 İade
    if (hoursUntilStart >= 24) return 0.5m;  // %50 İade
    return 0.0m;                             // İade Yok (Yandı)
  }
  public void SetAdminNotes(string? adminNotes, Guid updatedBy)
  {
    AdminNotes = adminNotes;
    UpdateMetadata(updatedBy);
  }

  public void AddExtra(RentalExtra extra)
  {
    if (extra == null)
      throw new ArgumentNullException(nameof(extra));

    if (Extras.Any(e => e.ExtraId == extra.ExtraId))
      throw new DomainValidationException("Bu ekstra zaten eklenmiş!");

    Extras.Add(extra);
    RecalculateTotalPrice();
  }

  private void RecalculateTotalPrice()
  {
    var extrasTotal = Extras.Sum(e => e.TotalPrice);
    TotalPrice = (DailyPrice * PlannedDays) + extrasTotal;
    DepositAmount = CalculateDeposit(TotalPrice);
    RemainingAmount = TotalPrice - DepositAmount;
  }

  // ==================== İŞ KURALLARI ====================

  /// Araç şu an fiziksel olarak rezerve/kilitli mi? 3 katmanlı kilit mantığı.
  public bool IsVehicleLocked()
  {
    var now = DateTimeOffset.UtcNow;

    return Status switch
    {
      // Katman 3: Active → tarih aralığı boyunca kilitli
      ReservationStatus.Active => true,

      // Katman 3: Confirmed + tam ödeme → tarih aralığı boyunca kilitli
      ReservationStatus.Confirmed when FullPaymentPaid => true,

      // Katman 2: Confirmed + sadece depozito → 24 saatlik kilit
      ReservationStatus.Confirmed when !FullPaymentPaid =>
          DepositLockDeadline.HasValue && DepositLockDeadline.Value > now,

      // Katman 1: PendingPayment → 15 dk checkout hold
      ReservationStatus.PendingPayment =>
          PaymentDeadline > now,

      _ => false
    };
  }

  public bool CanBeCancelled()
  {
    return Status == ReservationStatus.PendingPayment ||
           Status == ReservationStatus.Confirmed;
  }

  public bool CanBeConfirmed()
  {
    return Status == ReservationStatus.PendingPayment &&
           DateTimeOffset.UtcNow <= PaymentDeadline;
  }

  public decimal GetPriceDifference()
  {
    if (BillablePrice is null) return 0;
    return BillablePrice.Value - TotalPrice;
  }
}