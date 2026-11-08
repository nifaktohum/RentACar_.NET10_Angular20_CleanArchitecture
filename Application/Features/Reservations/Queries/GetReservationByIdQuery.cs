using Application.Behaviors;
using Application.Features.Reservations.Dto.Responses;
using Application.Features.Reservations.Specifications;
using Domain.Entities.Customers;
using Domain.Repositories;
using MediatR;
using TS.Result;

namespace Application.Features.Reservations.Queries;

[Permission("Reservations.Read")]
public sealed record GetReservationByIdQuery(Guid Id) : IRequest<Result<ReservationDetailDto>>;

public sealed class GetReservationByIdQueryHandler(
                        IReservationRepository _reservationRepo
                    ) : IRequestHandler<GetReservationByIdQuery, Result<ReservationDetailDto>>
{
  public async Task<Result<ReservationDetailDto>> Handle(GetReservationByIdQuery _req, CancellationToken _token)
  {
    // 1. Rezervasyonu detaylarıyla bul
    var spec = new ReservationByIdWithDetailsSpecification(_req.Id);
    var reservation = await _reservationRepo.GetFirstAsync(spec, _token);

    if (reservation == null)
      return Result<ReservationDetailDto>.Failure("Rezervasyon bulunamadı!");

    // 2. DTO'ya map et
    var dto = new ReservationDetailDto(
        Id: reservation.Id,
        ReservationNumber: reservation.ReservationNumber,
        CustomerId: reservation.CustomerId,
        CustomerFullName: GetCustomerFullName(reservation.Customer),
        CustomerPhone: reservation.Customer.PhoneNumber,
        CustomerEmail: reservation.Customer.Email,

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

        // ---------- Ödeme: Depozito (Katman 2) ----------
        DepositPaid: reservation.DepositPaid,
        DepositPaidAt: reservation.DepositPaidAt,
        PaymentId: reservation.PaymentId,
        DepositLockDeadline: reservation.DepositLockDeadline,

        // ---------- Ödeme: Tam Ödeme (Katman 3) ----------
        FullPaymentPaid: reservation.FullPaymentPaid,
        FullPaymentPaidAt: reservation.FullPaymentPaidAt,
        FullPaymentId: reservation.FullPaymentId,

        // ---------- Durum ve Süreler ----------
        Status: reservation.Status.ToString(),
        PaymentDeadline: reservation.PaymentDeadline,
        ConfirmedAt: reservation.ConfirmedAt,
        CompletedAt: reservation.CompletedAt,
        CancelledAt: reservation.CancelledAt,
        CancellationReason: reservation.CancellationReason,

        // ---------- Extralar ----------
        Extras: reservation.Extras.Select(e => new RentalExtraDto(
            ExtraId: e.ExtraId,
            Name: e.Extra?.Name ?? string.Empty, // 🚨 Null koruması
            UnitPrice: e.UnitPrice,
            Quantity: e.Quantity,
            RentalDayCount: e.RentalDayCount,
            PriceType: e.PriceType.ToString(),
            TotalPrice: e.TotalPrice
        )).ToList(),
        ExtrasTotalPrice: reservation.ExtrasTotalPrice,

        // ---------- Notlar ----------
        Notes: reservation.Notes,
        AdminNotes: reservation.AdminNotes,

        CreatedAt: reservation.CreatedAt,
        IsDepositRefunded: reservation.IsDepositRefunded,
        RefundedAmount: reservation.RefundedAmount,
        ForfeitedAmount: reservation.ForfeitedAmount
    );

    return Result<ReservationDetailDto>.Succeed(dto);
  }

  private static string GetCustomerFullName(Customer customer)
  {
    if (customer == null) return "Bilinmeyen Müşteri";

    return customer.Type == CustomerType.Individual
        ? $"{customer.FirstName} {customer.LastName}"
        : customer.CompanyName ?? "Kurumsal Müşteri";
  }
}