using Application.Features.Reservations.Dto.Responses;
using Application.Features.Reservations.Specifications;
using Domain.Entities.Customers;
using Domain.Repositories;
using MediatR;
using TS.Result;

namespace Application.Features.Reservations.Queries;

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

    if (reservation == null) return Result<ReservationDetailDto>.Failure("Rezervasyon bulunamadı!");



    // 2. DTO'ya map et
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

    return Result<ReservationDetailDto>.Succeed(dto);
  }

  private static string GetCustomerFullName(Customer customer)
  {
    return customer.Type == CustomerType.Individual
                                ? $"{customer.FirstName} {customer.LastName}"
                                : customer.CompanyName ?? "Kurumsal Müşteri";
  }
}
