using Application.Behaviors;
using Application.Common.Models;
using Application.Features.Reservations.Dto.Responses;
using Application.Features.Reservations.Specifications;
using Application.Specifications;
using Domain.Entities.Customers;
using Domain.Entities.Vehicles;
using Domain.Repositories;
using MediatR;
using TS.Result;

namespace Application.Features.Reservations.Queries;

[Permission("Reservations.Read")]
public sealed record GetReservationListQuery : ReservationSpecParams, IRequest<Result<PagedResponse<ReservationDto>>>;

public sealed class GetReservationListQueryHandler(
                        IReservationRepository _reservationRepo
                    ) : IRequestHandler<GetReservationListQuery, Result<PagedResponse<ReservationDto>>>
{
  public async Task<Result<PagedResponse<ReservationDto>>> Handle(GetReservationListQuery _req, CancellationToken _token)
  {
    // 1. Specification oluştur
    var spec = new GetAllReservationsSpecification(_req);

    // 2. Repository'den getir (Filtrelenmiş, sıralanmış ve sayfalanmış liste)
    var reservations = await _reservationRepo.GetAsync(spec, _token);

    // 3. Toplam sayıyı al (Sayfalama yapmadan, sadece filtreye uyanları sayar)
    var totalCount = await _reservationRepo.CountAsync(spec, _token);

    // 4. DTO'ya map et
    var result = reservations.Select(r => new ReservationDto(
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

    var response = new PagedResponse<ReservationDto>(
        result,
        totalCount,
        _req.PageNumber,
        _req.PageSize
    );

    return Result<PagedResponse<ReservationDto>>.Succeed(response);
  }

  // 🚨 Null Korumalı Metotlar
  private static string GetCustomerFullName(Customer? customer)
  {
    if (customer == null) return "Bilinmeyen Müşteri";

    return customer.Type == CustomerType.Individual
        ? $"{customer.FirstName} {customer.LastName}"
        : customer.CompanyName ?? "Kurumsal Müşteri";
  }

  private static string GetVehicleInfo(Vehicle? vehicle)
  {
    if (vehicle == null) return "Araç Bilgisi Yok";

    return $"{vehicle.Brand} {vehicle.Model} - {vehicle.Plate}";
  }
}