using Application.Common.Paging;
using Domain.Entities.Reservations;

namespace Application.Specifications;

public record ReservationSpecParams : BasePagedRequest
{
  // ============ SIRALAMA ============
  private string? _sort;
  public string? Sort
  {
    get => _sort;
    set => _sort = string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLower();
  }

  // ============ MÜŞTERİ FİLTRESİ ============
  private Guid? _customerId;
  public Guid? CustomerId
  {
    get => _customerId;
    set => _customerId = value != Guid.Empty ? value : null;
  }

  // ============ ARAÇ FİLTRESİ ============
  private Guid? _vehicleId;
  public Guid? VehicleId
  {
    get => _vehicleId;
    set => _vehicleId = value != Guid.Empty ? value : null;
  }


  // ============ ŞUBE FİLTRESİ ============
  private Guid? _pickupBranchId;
  public Guid? PickupBranchId
  {
    get => _pickupBranchId;
    set => _pickupBranchId = value != Guid.Empty ? value : null;
  }

  // ============ DURUM FİLTRESİ (Enum) ============
  private ReservationStatus? _status;
  public ReservationStatus? Status
  {
    get => _status;
    set => _status = value;
  }

  // ============ ⭐ ÇOKLU DURUM FİLTRESİ (YENİ) ============
  // Kullanım: ?statuses=PendingPayment,Confirmed,Active
  // Virgülle ayrılmış string VEYA dizi olarak gelebilir.
  private List<ReservationStatus> _statuses = [];
  public List<ReservationStatus> Statuses
  {
    get => _statuses;
    set
    {
      if (value == null || value.Count == 0)
      {
        _statuses = [];
        return;
      }

      // Enum olarak gelirse direkt kullan
      if (value.All(v => Enum.IsDefined(typeof(ReservationStatus), v)))
      {
        _statuses = value.Distinct().ToList();
      }
      else
      {
        _statuses = [];
      }
    }
  }

  // ⭐ Query string'den gelen "PendingPayment,Confirmed,Active" gibi
  // virgülle ayrılmış string'i parse eden yardımcı property.
  // ASP.NET Core model binding, "statuses" parametresini önce List<ReservationStatus>'a
  // çevirmeye çalışır. Başaramazsa bu property devreye girer.
  public string? StatusesRaw
  {
    get => _statuses.Count == 0 ? null : string.Join(",", _statuses.Select(s => s.ToString()));
    set
    {
      if (string.IsNullOrWhiteSpace(value))
      {
        _statuses = [];
        return;
      }

      _statuses = value
          .Split(',', StringSplitOptions.RemoveEmptyEntries)
          .Select(s => s.Trim())
          .Where(s => !string.IsNullOrEmpty(s))
          .Select(s => Enum.TryParse<ReservationStatus>(s, ignoreCase: true, out var parsed) ? parsed : (ReservationStatus?)null)
          .Where(s => s.HasValue)
          .Select(s => s!.Value)
          .Distinct()
          .ToList();
    }
  }

  // ============ ⭐ AKTİF SÜREÇ FİLTRESİ (YENİ) ============
  // true ise sadece "para güvenceye alınmış" rezervasyonlar döner:
  //   - Confirmed + DepositPaid (kaporası alınmış, tam ödeme beklemede)
  //   - Confirmed + FullPaymentPaid (tam ödemesi alınmış)
  //   - Active (kirada)
  private bool? _activeOnly;
  public bool? ActiveOnly
  {
    get => _activeOnly;
    set => _activeOnly = value;
  }

  // ⭐ ALT FİLTRE (opsiyonel)
  // "deposit" → sadece Kaporası Alınmış
  // "full"    → sadece Tam Ödemesi Alınmış
  // "active"  → sadece Kirada
  // null      → 3'ü birden
  private string? _paymentFilter;
  public string? PaymentFilter
  {
    get => _paymentFilter;
    set => _paymentFilter = string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLower();
  }
  // ============ TARİH FİLTRELERİ ============
  private DateTimeOffset? _startDate;
  public DateTimeOffset? StartDate
  {
    get => _startDate;
    set => _startDate = value != null && DateTimeOffset.TryParse(value.ToString(), out var result)
        ? result
        : null;
  }

  private DateTimeOffset? _endDate;
  public DateTimeOffset? EndDate
  {
    get => _endDate;
    set => _endDate = value != null && DateTimeOffset.TryParse(value.ToString(), out var result)
        ? result
        : null;
  }

  // ============ GENEL ARAMA FİLTRESİ ============
  private string? _searchTerm;
  public string? SearchTerm
  {
    get => _searchTerm;
    set => _searchTerm = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
  }

  // ============ AKTİFLİK DURUMU FİLTRESİ ============
  private bool? _isActive;
  public bool? IsActive
  {
    get => _isActive;
    set => _isActive = value != null && bool.TryParse(value.ToString(), out var result) ? result : null;
  }

  // ============ EKSTRA FİLTRELERİ (İleride gerekirse) ============
  // Rezervasyon numarasına göre tam eşleşme
  private string? _reservationNumber;
  public string? ReservationNumber
  {
    get => _reservationNumber;
    set => _reservationNumber = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
  }




}
