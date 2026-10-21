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
