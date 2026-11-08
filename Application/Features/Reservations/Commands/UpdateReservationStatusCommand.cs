using Application.Behaviors;
using Application.Features.Reservations.Dto.Responses;
using Application.Features.Reservations.Specifications;
using Domain.Entities.Customers;
using Domain.Entities.Reservations;
using Domain.Entities.Vehicles;
using Domain.Exceptions;
using Domain.Repositories;
using Domain.Repositories.Vehicles;
using FluentValidation;
using GenericRepository;
using MediatR;
using Microsoft.Extensions.Logging;
using TS.Result;

namespace Application.Features.Reservations.Commands;

[Permission("Reservations.Update")]
public sealed record UpdateReservationStatusCommand(
                        Guid ReservationId,
                        ReservationStatus NewStatus,
                        string? Reason,
                        string? AdminNotes,
                        string? PaymentId,                 // Depozito için
                        string? FullPaymentId,             // 🚨 Tam ödeme için eklendi
                        DateTimeOffset? ActualPickupAt,
                        DateTimeOffset? ActualReturnAt,
                        int? ReturnKilometer               // 🚨 İade km'si için eklendi
                    ) : IRequest<Result<ReservationDetailDto>>;

public sealed class UpdateReservationStatusCommandValidator : AbstractValidator<UpdateReservationStatusCommand>
{
  public UpdateReservationStatusCommandValidator()
  {
    RuleFor(x => x.ReservationId).NotEmpty().WithMessage("Rezervasyon ID boş olamaz!");
    RuleFor(x => x.NewStatus).IsInEnum().WithMessage("Geçersiz rezervasyon durumu!");

    RuleFor(x => x.PaymentId)
        .NotEmpty().When(x => x.NewStatus == ReservationStatus.Confirmed)
        .WithMessage("Rezervasyon onaylamak için depozito ödeme ID'si zorunludur!");

    RuleFor(x => x.FullPaymentId)
        .NotEmpty().When(x => x.NewStatus == ReservationStatus.Active)
        .WithMessage("Araç teslimi (Active) için tam ödeme ID'si zorunludur!");

    RuleFor(x => x.Reason)
        .NotEmpty().When(x => x.NewStatus == ReservationStatus.Cancelled)
        .WithMessage("İptal durumu için sebep belirtilmelidir!");

    RuleFor(x => x.ReturnKilometer)
        .NotEmpty().When(x => x.NewStatus == ReservationStatus.Completed)
        .WithMessage("Araç iadesi (Completed) için iade kilometresi zorunludur!");

    RuleFor(x => x.PaymentId).MaximumLength(100);
    RuleFor(x => x.FullPaymentId).MaximumLength(100);
    RuleFor(x => x.Reason).MaximumLength(500);
    RuleFor(x => x.AdminNotes).MaximumLength(1000);
  }
}

public sealed class UpdateReservationStatusCommandHandler(
                        IReservationRepository _reservationRepo,
                        IUserRepository _userRepo,
                        IVehicleRepository _vehicleRepo,
                        IUnitOfWork _unit,
                        ILogger<UpdateReservationStatusCommandHandler> _logger
                    ) : IRequestHandler<UpdateReservationStatusCommand, Result<ReservationDetailDto>>
{
  public async Task<Result<ReservationDetailDto>> Handle(UpdateReservationStatusCommand _req, CancellationToken _token)
  {
    try
    {
      _logger.LogInformation("📝 Durum güncelleniyor. Id: {Id}, Yeni Durum: {Status}", _req.ReservationId, _req.NewStatus);

      var spec = new ReservationByIdWithDetailsSpecification(_req.ReservationId);
      var reservation = await _reservationRepo.GetFirstAsync(spec, _token);

      if (reservation is null) return Result<ReservationDetailDto>.Failure(404, "Rezervasyon bulunamadı!");
      if (reservation.Status == _req.NewStatus) return Result<ReservationDetailDto>.Failure(400, "Rezervasyon zaten bu durumda!");

      var userId = _userRepo.GetCurrentUserId();
      if (userId == Guid.Empty) userId = Guid.Parse("00000000-0000-0000-0000-000000000001");

      var oldStatus = reservation.Status;
      // var vehicle = await _vehicleRepo.GetByIdAsync(reservation.VehicleId, _token);
      var vehicle = reservation.Vehicle;
      if (vehicle == null) return Result<ReservationDetailDto>.Failure(404, "Rezervasyona bağlı araç bulunamadı!");

      var initialVehicleStatus = vehicle.Status;
      try
      {
        switch (_req.NewStatus)
        {
          // 1. PendingPayment → Confirmed (Sadece Depozito)
          case ReservationStatus.Confirmed:
            reservation.MarkDepositPaid(_req.PaymentId!, userId);
            // 🚨 DİKKAT: Burada vehicle.Rent() YOK! Araç henüz müşteriye teslim edilmedi.
            _logger.LogInformation("✅ Rezervasyon onaylandı (Depozito). PaymentId: {PaymentId}", _req.PaymentId);
            break;

          // 2. Confirmed → Active (Tam Ödeme + Araç Teslimi)
          case ReservationStatus.Active:
            if (string.IsNullOrWhiteSpace(_req.FullPaymentId))
              return Result<ReservationDetailDto>.Failure(400, "Tam ödeme ID'si zorunludur!");

            // 🚨 Eğer tam ödeme yapılmamışsa yap
            if (!reservation.FullPaymentPaid)
            {
              reservation.MarkFullPaymentPaid(_req.FullPaymentId, userId);
            }

            // Sonra aracı teslim et (FullPaymentPaid true olmadan çalışmaz)
            reservation.MarkAsActive(_req.ActualPickupAt ?? DateTimeOffset.UtcNow, userId);

            // 🚨 ŞİMDİ ARAÇ MÜHÜRLENİYOR (Fiziksel Teslimat)
            if (vehicle.Status == VehicleStatus.Available)
            {
              vehicle.Rent();
              _vehicleRepo.Update(vehicle);
            }
            _logger.LogInformation("🚗 Araç teslim edildi ve mühürlendi. Id: {Id}", reservation.Id);
            break;

          // 3. Active → Completed (Araç İadesi)
          case ReservationStatus.Completed:
            // İade km'si zorunlu
            reservation.MarkAsCompleted(_req.ActualReturnAt ?? DateTimeOffset.UtcNow, userId);

            // 🚨 ARAÇ İADE ALINIYOR (Girilen km ile stoğa döner)
            if (vehicle.Status == VehicleStatus.Rented)
            {
              vehicle.Return(_req.ReturnKilometer!.Value);
              _vehicleRepo.Update(vehicle);
            }
            _logger.LogInformation("🏁 Rezervasyon tamamlandı ve araç stoğa döndü. Id: {Id}", reservation.Id);
            break;

          // 4. İptal İşlemi
          case ReservationStatus.Cancelled:
            reservation.Cancel(_req.Reason!, userId);

            // Araç mühürlüyse (Depozito veya tam ödeme alınmışsa) serbest bırak
            if (oldStatus == ReservationStatus.Confirmed || oldStatus == ReservationStatus.Active)
            {
              if (vehicle.Status == VehicleStatus.Rented)
              {
                vehicle.Return(vehicle.CurrentKilometer);
                _vehicleRepo.Update(vehicle);
              }
            }

            // İade logu
            if (reservation.DepositPaid)
            {
              _logger.LogInformation("💰 İade Hesabı: İade={Refunded} TL, Yanan={Forfeited} TL",
                  reservation.RefundedAmount ?? 0, reservation.ForfeitedAmount ?? 0);
            }
            _logger.LogInformation("❌ Rezervasyon iptal edildi. Sebep: {Reason}", _req.Reason);
            break;

          // 5. Süre Aşımı
          case ReservationStatus.Expired:
            reservation.MarkAsExpired(userId);
            _logger.LogInformation("⏰ Rezervasyon süresi doldu. Id: {Id}", reservation.Id);
            break;

          case ReservationStatus.NoShow:
            return Result<ReservationDetailDto>.Failure(400, "NoShow durumu artık kullanılmıyor!");

          default:
            return Result<ReservationDetailDto>.Failure(400, $"'{_req.NewStatus}' durumuna geçiş desteklenmiyor!");
        }
      }
      catch (DomainValidationException ex)
      {
        _logger.LogWarning(ex, "⚠️ Domain validasyon hatası. Id: {Id}", _req.ReservationId);
        return Result<ReservationDetailDto>.Failure(400, ex.Message);
      }

      if (!string.IsNullOrWhiteSpace(_req.AdminNotes))
        reservation.SetAdminNotes(_req.AdminNotes, userId);

      _reservationRepo.Update(reservation);

      // Switch bittikten sonra:
      if (vehicle.Status != initialVehicleStatus)
        _vehicleRepo.Update(vehicle);

      await _unit.SaveChangesAsync(_token);

      _logger.LogInformation("✅ Durum güncellemesi başarılı. Id: {Id}, {Old} → {New}", reservation.Id, oldStatus, reservation.Status);

      return await BuildResponse(reservation, _token);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "❌ Durum güncellemesi hatası. Id: {Id}", _req.ReservationId);
      return Result<ReservationDetailDto>.Failure(500, $"Bir hata oluştu: {ex.Message}");
    }
  }

  // ==================== RESPONSE BUILDER ====================
  private static Task<Result<ReservationDetailDto>> BuildResponse(Reservation reservation, CancellationToken token)
  {
    var dto = new ReservationDetailDto(
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
        DepositLockDeadline: reservation.DepositLockDeadline,
        FullPaymentPaid: reservation.FullPaymentPaid,
        FullPaymentPaidAt: reservation.FullPaymentPaidAt,
        FullPaymentId: reservation.FullPaymentId,
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
        CreatedAt: reservation.CreatedAt,
        IsDepositRefunded: reservation.IsDepositRefunded,
        RefundedAmount: reservation.RefundedAmount,
        ForfeitedAmount: reservation.ForfeitedAmount
    );

    return Task.FromResult(Result<ReservationDetailDto>.Succeed(dto));
  }

  private static string GetCustomerFullName(Customer customer)
  {
    return customer.Type == CustomerType.Individual
        ? $"{customer.FirstName} {customer.LastName}"
        : customer.CompanyName ?? "Kurumsal Müşteri";
  }
}