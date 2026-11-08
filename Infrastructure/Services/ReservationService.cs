using Application.Features.Reservations.Dto.Requests;
using Application.Features.Reservations.Dto.Responses;
using Application.Features.Reservations.Specifications;
using Application.Services;
using Application.Specifications;
using Domain.Entities.Customers;
using Domain.Entities.Extras;
using Domain.Entities.Reservations;
using Domain.Entities.Vehicles;
using Domain.Exceptions;
using Domain.Repositories;
using Domain.Repositories.Extras;
using Domain.Repositories.Vehicles;
using GenericRepository;

namespace Infrastructure.Services;

public sealed class ReservationService(
                      ICustomerRepository _customerRepo,
                      IVehicleRepository _vehicleRepo,
                      IReservationRepository _reservationRepo,
                      IExtraRepository _extraRepo,
                      IReservationNumberGenerator _reservationNumberGenerator,
                      IUnitOfWork _unit
                    ) : IReservationService
{
  // ==================== ACTIVATE ====================
  public async Task ActivateAsync(Guid id, ActivateReservationDto dto, Guid userId, CancellationToken cancellationToken = default)
  {
    var spec = new ReservationByIdSpecification(id);
    var reservation = await _reservationRepo.GetFirstAsync(spec, cancellationToken);

    if (reservation == null) throw new DomainValidationException("Rezervasyon bulunamadı!");

    // Domain metodu (İçeride FullPaymentPaid kontrolü var)
    reservation.MarkAsActive(dto.ActualPickupAt, userId);

    // ⭐ Araç fiziken kirada (Stok düşümü burada gerçekleşir)
    var vehicle = await _vehicleRepo.GetByIdAsync(reservation.VehicleId, cancellationToken);
    if (vehicle == null) throw new DomainValidationException("Araç bulunamadı!");

    if (vehicle.Status != VehicleStatus.Rented)
    {
      vehicle.Rent();
      _vehicleRepo.Update(vehicle);
    }

    _reservationRepo.Update(reservation);
    await _unit.SaveChangesAsync(cancellationToken);
  }

  // ==================== CANCEL ====================
  public async Task CancelAsync(Guid id, CancelReservationDto dto, Guid userId, CancellationToken cancellationToken = default)
  {
    var spec = new ReservationByIdSpecification(id);
    var reservation = await _reservationRepo.GetFirstAsync(spec, cancellationToken);

    if (reservation == null) throw new DomainValidationException("Rezervasyon bulunamadı!");

    var oldStatus = reservation.Status;

    // Domain metodu
    reservation.Cancel(dto.Reason, userId);

    // ⭐ Confirmed veya Active iptalinde aracı serbest bırak (stoğa döndür)
    if (oldStatus == ReservationStatus.Confirmed || oldStatus == ReservationStatus.Active)
    {
      var vehicle = await _vehicleRepo.GetByIdAsync(reservation.VehicleId, cancellationToken);
      if (vehicle != null && vehicle.Status == VehicleStatus.Rented)
      {
        vehicle.Return(vehicle.CurrentKilometer); // Mevcut km ile stoğa dön
        _vehicleRepo.Update(vehicle);
      }
    }

    _reservationRepo.Update(reservation);
    await _unit.SaveChangesAsync(cancellationToken);
  }

  // ==================== COMPLETE ====================
  public async Task CompleteAsync(Guid id, CompleteReservationDto dto, Guid userId, CancellationToken cancellationToken = default)
  {
    var spec = new ReservationByIdWithDetailsSpecification(id);
    var reservation = await _reservationRepo.GetFirstAsync(spec, cancellationToken);

    if (reservation == null) throw new DomainValidationException("Rezervasyon bulunamadı!");

    // Domain metodu (BillablePrice hesabı vs.)
    reservation.MarkAsCompleted(dto.ActualReturnAt, userId);

    // ⭐ Araç iade alındı → fiziken Available (stoğa döner)
    var vehicle = await _vehicleRepo.GetByIdAsync(reservation.VehicleId, cancellationToken);
    if (vehicle != null && vehicle.Status == VehicleStatus.Rented)
    {
      // DTO'da returnKilometer yoksa mevcut km ile iade alıyoruz
      vehicle.Return(vehicle.CurrentKilometer);
      _vehicleRepo.Update(vehicle);
    }

    _reservationRepo.Update(reservation);
    await _unit.SaveChangesAsync(cancellationToken);
  }

  // ==================== CREATE ====================
  public async Task<Guid> CreateAsync(CreateReservationDto dto, Guid userId, CancellationToken cancellationToken = default)
  {
    // 1. Müşteri kontrolü
    var customer = await _customerRepo.GetByExpressionAsync(c => c.Id == dto.CustomerId, cancellationToken);
    if (customer == null) throw new DomainValidationException("Müşteri bulunamadı!");
    if (!customer.CanRent()) throw new DomainValidationException("Müşteri araç kiralamak için uygun değil! (Kara liste, ehliyet, yaş vb.)");

    // 2. Araç kontrolü
    var vehicle = await _vehicleRepo.GetVehicleWithDetailsAsync(dto.VehicleId, cancellationToken);
    if (vehicle == null) throw new DomainValidationException("Araç bulunamadı!");

    // ⭐ IsAvailable yerine CanBeReservedNow (Rented → true, Maintenance/OutOfService → false)
    if (!vehicle.CanBeReservedNow)
      throw new DomainValidationException("Araç şu anda rezervasyon için uygun değil!");

    // 3. Müsaitlik kontrolü (Tarih çakışması)
    var conflictSpec = new ConflictingReservationsSpecification(dto.VehicleId, dto.PlannedStartDate, dto.PlannedEndDate);
    var conflicts = await _reservationRepo.GetAsync(conflictSpec, cancellationToken);
    if (conflicts.Any()) throw new DomainValidationException("Araç bu tarih aralığında müsait değil!");

    // 4. Extraları kontrol et
    var extraIds = dto.Extras.Select(e => e.ExtraId).ToList();
    var extras = await _extraRepo.GetByIdsAsync(extraIds, cancellationToken);

    // 5. Numara üret
    var reservationNumber = await _reservationNumberGenerator.GenerateAsync(cancellationToken);

    // 6. Reservation oluştur
    var reservation = new Reservation(
        reservationNumber: reservationNumber,
        customerId: dto.CustomerId,
        vehicleId: dto.VehicleId,
        pickupBranchId: dto.PickupBranchId,
        returnBranchId: dto.ReturnBranchId,
        plannedStartDate: dto.PlannedStartDate,
        plannedEndDate: dto.PlannedEndDate,
        dailyPrice: vehicle.DailyPrice,
        notes: dto.Notes,
        createdBy: userId
    );

    // 🚨 DÜZELTME: Aracı rezervasyona ata
    reservation.SetVehicle(vehicle);

    // 7. Extraları ekle ve stok düş
    foreach (var item in dto.Extras)
    {
      var extra = extras.FirstOrDefault(e => e.Id == item.ExtraId);
      if (extra == null)
        throw new DomainValidationException($"Ekstra ürün bulunamadı! (ID: {item.ExtraId})");

      if (!extra.IsInStock())
        throw new DomainValidationException($"{extra.Name} stokta yok!");

      var rentalExtra = new RentalExtra(
          rentalId: reservation.Id,
          extraId: extra.Id,
          unitPrice: extra.Price,
          quantity: item.Quantity,
          rentalDayCount: reservation.PlannedDays,
          priceType: extra.PriceType,
          createdBy: userId
      );

      reservation.AddExtra(rentalExtra);
      extra.DecreaseStock(item.Quantity);
    }

    // 8. Kaydet
    await _reservationRepo.AddAsync(reservation, cancellationToken);
    await _unit.SaveChangesAsync(cancellationToken);

    return reservation.Id;
  }

  // ==================== GET BY ID ====================
  public async Task<ReservationDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
  {
    var spec = new ReservationByIdWithDetailsSpecification(id);
    var reservation = await _reservationRepo.GetFirstAsync(spec, cancellationToken);

    if (reservation == null)
      throw new DomainValidationException("Rezervasyon bulunamadı!");

    return new ReservationDetailDto(
        Id: reservation.Id,
        ReservationNumber: reservation.ReservationNumber,
        CustomerId: reservation.CustomerId,
        CustomerFullName: GetCustomerFullName(reservation.Customer),
        CustomerPhone: reservation.Customer?.PhoneNumber ?? string.Empty,
        CustomerEmail: reservation.Customer?.Email ?? string.Empty,
        VehicleId: reservation.VehicleId,
        VehicleBrand: reservation.Vehicle?.Brand ?? string.Empty,
        VehicleModel: reservation.Vehicle?.Model ?? string.Empty,
        VehiclePlate: reservation.Vehicle?.Plate ?? string.Empty,
        PickupBranchName: reservation.PickupBranch?.Name ?? string.Empty,
        ReturnBranchName: reservation.ReturnBranch?.Name,
        PlannedStartDate: reservation.PlannedStartDate,
        PlannedEndDate: reservation.PlannedEndDate,
        PlannedDays: reservation.PlannedDays,
        ActualPickupAt: reservation.ActualPickupAt,
        ActualReturnAt: reservation.ActualReturnAt,
        DailyPrice: reservation.DailyPrice,
        TotalPrice: reservation.TotalPrice,
        BillablePrice: reservation.BillablePrice,
        DepositAmount: reservation.DepositAmount,
        RemainingAmount: reservation.RemainingAmount,

        // Ödeme: Depozito
        DepositPaid: reservation.DepositPaid,
        DepositPaidAt: reservation.DepositPaidAt,
        PaymentId: reservation.PaymentId,
        DepositLockDeadline: reservation.DepositLockDeadline,

        // Ödeme: Tam Ödeme
        FullPaymentPaid: reservation.FullPaymentPaid,
        FullPaymentPaidAt: reservation.FullPaymentPaidAt,
        FullPaymentId: reservation.FullPaymentId,

        // Durum
        Status: reservation.Status.ToString(),
        PaymentDeadline: reservation.PaymentDeadline,
        ConfirmedAt: reservation.ConfirmedAt,
        CompletedAt: reservation.CompletedAt,
        CancelledAt: reservation.CancelledAt,
        CancellationReason: reservation.CancellationReason,

        Extras: reservation.Extras.Select(e => new RentalExtraDto(
            ExtraId: e.ExtraId,
            Name: e.Extra?.Name ?? string.Empty,
            UnitPrice: e.UnitPrice,
            Quantity: e.Quantity,
            RentalDayCount: e.RentalDayCount,
            PriceType: e.PriceType.ToString(),
            TotalPrice: e.TotalPrice
        )).ToList(),
        ExtrasTotalPrice: reservation.ExtrasTotalPrice,
        Notes: reservation.Notes,
        AdminNotes: reservation.AdminNotes,
        CreatedAt: reservation.CreatedAt,
        IsDepositRefunded: reservation.IsDepositRefunded,
        RefundedAmount: reservation.RefundedAmount,
        ForfeitedAmount: reservation.ForfeitedAmount
    );
  }

  // ==================== GET LIST ====================
  public async Task<List<ReservationDto>> GetListAsync(ReservationSpecParams specParams, CancellationToken cancellationToken = default)
  {
    var spec = new GetAllReservationsSpecification(specParams);
    var reservations = await _reservationRepo.GetAsync(spec, cancellationToken);

    return reservations.Select(r => new ReservationDto(
        Id: r.Id,
        ReservationNumber: r.ReservationNumber,
        CustomerFullName: GetCustomerFullName(r.Customer),
        VehicleInfo: GetVehicleInfo(r.Vehicle),
        PlannedStartDate: r.PlannedStartDate,
        PlannedEndDate: r.PlannedEndDate,
        PlannedDays: r.PlannedDays,
        Status: r.Status.ToString(),
        TotalPrice: r.TotalPrice,
        DepositAmount: r.DepositAmount,
        DepositPaid: r.DepositPaid,
        CreatedAt: r.CreatedAt
    )).ToList();
  }

  // ==================== PAY DEPOSIT ====================
  public async Task PayDepositAsync(Guid id, PayDepositDto dto, Guid userId, CancellationToken cancellationToken = default)
  {
    var spec = new ReservationByIdSpecification(id);
    var reservation = await _reservationRepo.GetFirstAsync(spec, cancellationToken);

    if (reservation == null) throw new DomainValidationException("Rezervasyon bulunamadı!");

    // Domain metodu (24 saatlik DepositLockDeadline set eder)
    reservation.MarkDepositPaid(dto.PaymentId, userId);

    // ⭐ Stok düşürmek yerine aracı kiraya ver (mühürle)
    var vehicle = await _vehicleRepo.GetByIdAsync(reservation.VehicleId, cancellationToken);
    if (vehicle != null && vehicle.Status != VehicleStatus.Rented)
    {
      vehicle.Rent();
      _vehicleRepo.Update(vehicle);
    }

    _reservationRepo.Update(reservation);
    await _unit.SaveChangesAsync(cancellationToken);
  }

  // ==================== PRIVATE HELPERS ====================
  private static string GetCustomerFullName(Customer customer)
  {
    if (customer == null) return "Bilinmeyen Müşteri";
    return customer.Type == CustomerType.Individual
        ? $"{customer.FirstName} {customer.LastName}"
        : customer.CompanyName ?? "Kurumsal Müşteri";
  }

  private static string GetVehicleInfo(Vehicle vehicle)
  {
    if (vehicle == null) return "Araç Bilgisi Yok";
    return $"{vehicle.Brand} {vehicle.Model} - {vehicle.Plate}";
  }
}