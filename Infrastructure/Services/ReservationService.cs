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
  public async Task ActivateAsync(Guid id, ActivateReservationDto dto, Guid userId, CancellationToken cancellationToken = default)
  {
    var spec = new ReservationByIdSpecification(id);
    var reservation = await _reservationRepo.GetFirstAsync(spec, cancellationToken);

    if (reservation == null) throw new DomainValidationException("Rezervasyon bulunamadı!");

    // Domain metodunu çağır
    reservation.MarkAsActive(dto.ActualPickupAt, userId);

    _reservationRepo.Update(reservation);
    await _unit.SaveChangesAsync(cancellationToken);
  }

  public async Task CancelAsync(Guid id, CancelReservationDto dto, Guid userId, CancellationToken cancellationToken = default)
  {
    var spec = new ReservationByIdSpecification(id);
    var reservation = await _reservationRepo.GetFirstAsync(spec, cancellationToken);

    if (reservation == null) throw new DomainValidationException("Rezervasyon bulunamadı!");

    // Domain metodunu çağır
    reservation.Cancel(dto.Reason, userId);

    _reservationRepo.Update(reservation);
    await _unit.SaveChangesAsync(cancellationToken);
  }

  public async Task CompleteAsync(Guid id, CompleteReservationDto dto, Guid userId, CancellationToken cancellationToken = default)
  {
    // Extraları da getirmemiz lazım (BillablePrice hesabı için)
    var spec = new ReservationByIdWithDetailsSpecification(id);
    var reservation = await _reservationRepo.GetFirstAsync(spec, cancellationToken);

    if (reservation == null) throw new DomainValidationException("Rezervasyon bulunamadı!");

    // Domain metodunu çağır
    reservation.MarkAsCompleted(dto.ActualReturnAt, userId);

    _reservationRepo.Update(reservation);
    await _unit.SaveChangesAsync(cancellationToken);
  }

  public async Task<Guid> CreateAsync(CreateReservationDto dto, Guid userId, CancellationToken cancellationToken = default)
  {
    // 1. Müşteri kontrolü
    var customer = await _customerRepo.GetByExpressionAsync(c => c.Id == dto.CustomerId, cancellationToken);
    if (customer == null) throw new DomainValidationException("Müşteri bulunamadı!");
    if (!customer.CanRent()) throw new DomainValidationException("Müşteri araç kiralamak için uygun değil! (Kara liste, ehliyet, yaş vb.)");

    // 2. Araç kontrolü
    var vehicle = await _vehicleRepo.GetVehicleWithDetailsAsync(dto.VehicleId, cancellationToken);
    if (vehicle == null) throw new DomainValidationException("Araç bulunamadı!");
    if (!vehicle.IsAvailable) throw new DomainValidationException("Araç şu anda müsait değil!");

    // 3. Müsaitlik kontrolü (Tarih çakışması)
    var conflictSpec = new ConflictingReservationsSpecification(dto.VehicleId, dto.PlannedStartDate, dto.PlannedEndDate);
    var conflicts = await _reservationRepo.GetAsync(conflictSpec, cancellationToken);
    if (conflicts.Any()) throw new DomainValidationException("Araç bu tarih aralığında müsait değil!");

    // 4. Extraları kontrol et (fiyatları ve stokları)
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
        dailyPrice: vehicle.DailyPrice, // Snapshot
        notes: dto.Notes,
        createdBy: userId
    );

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

      // Stok düş
      extra.DecreaseStock(item.Quantity);
    }

    // 8. Kaydet (Reservation + RentalExtras + Extra Stokları)
    await _reservationRepo.AddAsync(reservation, cancellationToken);
    await _unit.SaveChangesAsync(cancellationToken);

    return reservation.Id;
  }

  public async Task<ReservationDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
  {
    var spec = new ReservationByIdWithDetailsSpecification(id);
    var reservation = await _reservationRepo.GetFirstAsync(spec, cancellationToken);

    if (reservation == null)
      throw new DomainValidationException("Rezervasyon bulunamadı!");

    // ⭐ Map: Entity → DTO
    return new ReservationDetailDto(
        Id: reservation.Id,
        ReservationNumber: reservation.ReservationNumber,
        CustomerId: reservation.CustomerId,
        CustomerFullName: GetCustomerFullName(reservation.Customer),
        CustomerPhone: reservation.Customer.PhoneNumber,
        CustomerEmail: reservation.Customer.Email,
        VehicleId: reservation.VehicleId,
        VehicleBrand: reservation.Vehicle.Brand,
        VehicleModel: reservation.Vehicle.Model,
        VehiclePlate: reservation.Vehicle.Plate,
        PickupBranchName: reservation.PickupBranch.Name,
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
        DepositPaid: reservation.DepositPaid,
        DepositPaidAt: reservation.DepositPaidAt,
        PaymentId: reservation.PaymentId,
        Status: reservation.Status.ToString(),
        PaymentDeadline: reservation.PaymentDeadline,
        PickupDeadline: reservation.PickupDeadline,
        ConfirmedAt: reservation.ConfirmedAt,
        CompletedAt: reservation.CompletedAt,
        CancelledAt: reservation.CancelledAt,
        CancellationReason: reservation.CancellationReason,
        Extras: reservation.Extras.Select(e => new RentalExtraDto(
            ExtraId: e.ExtraId,
            Name: e.Extra.Name,
            UnitPrice: e.UnitPrice,
            Quantity: e.Quantity,
            RentalDayCount: e.RentalDayCount,
            PriceType: e.PriceType.ToString(),
            TotalPrice: e.TotalPrice
        )).ToList(),
        ExtrasTotalPrice: reservation.ExtrasTotalPrice,
        Notes: reservation.Notes,
        AdminNotes: reservation.AdminNotes,
        CreatedAt: reservation.CreatedAt
    );
  }

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

  public async Task PayDepositAsync(Guid id, PayDepositDto dto, Guid userId, CancellationToken cancellationToken = default)
  {
    var spec = new ReservationByIdSpecification(id);
    var reservation = await _reservationRepo.GetFirstAsync(spec, cancellationToken);

    if (reservation == null) throw new DomainValidationException("Rezervasyon bulunamadı!");

    // Domain metodunu çağır (İçinde iş kuralları var)
    reservation.MarkDepositPaid(dto.PaymentId, userId);

    _reservationRepo.Update(reservation);
    await _unit.SaveChangesAsync(cancellationToken);
  }

  // ==================== PRIVATE HELPERS ====================
  private static string GetCustomerFullName(Customer customer)
  {
    return customer.Type == CustomerType.Individual
        ? $"{customer.FirstName} {customer.LastName}"
        : customer.CompanyName ?? "Kurumsal Müşteri";
  }

  private static string GetVehicleInfo(Vehicle vehicle)
  {
    return $"{vehicle.Brand} {vehicle.Model} - {vehicle.Plate}";
  }
}
