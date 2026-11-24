using Application.Features.Reservations.Dto.Responses;
using Domain.Entities.Reservations;
using Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TS.Result;

namespace Application.Features.Reservations.Queries;

public sealed record GetReservationStatsQuery : IRequest<Result<ReservationStatsDto>>;

public sealed class GetReservationStatsQueryHandler(
    IReservationRepository _reservationRepo
) : IRequestHandler<GetReservationStatsQuery, Result<ReservationStatsDto>>
{
  public async Task<Result<ReservationStatsDto>> Handle(
      GetReservationStatsQuery _req,
      CancellationToken _token)
  {
    try
    {
      // 1. Tüm statülerin sayısı
      var statusCounts = await _reservationRepo
          .GetAll()
          .Where(r => !r.IsDeleted)
          .GroupBy(r => r.Status)
          .Select(g => new { Status = g.Key, Count = g.Count() })
          .ToListAsync(_token);

      // 2. Confirmed olanları kendi içinde böl (Kapora vs Tam Ödeme)
      var confirmedBreakdown = await _reservationRepo
          .GetAll()
          .Where(r => !r.IsDeleted && r.Status == ReservationStatus.Confirmed)
          .Select(r => new
          {
            IsFullPaid = r.FullPaymentPaid,
            IsDepositOnly = r.DepositPaid && !r.FullPaymentPaid
          })
          .GroupBy(x => new { x.IsFullPaid, x.IsDepositOnly })
          .Select(g => new
          {
            g.Key.IsFullPaid,
            g.Key.IsDepositOnly,
            Count = g.Count()
          })
          .ToListAsync(_token);

      int fullPaidCount = confirmedBreakdown.Where(x => x.IsFullPaid).Sum(x => x.Count);
      int depositOnlyCount = confirmedBreakdown.Where(x => x.IsDepositOnly).Sum(x => x.Count);

      var counts = statusCounts.ToDictionary(x => x.Status, x => x.Count);

      int GetCount(ReservationStatus status) => counts.GetValueOrDefault(status, 0);

      var total = statusCounts.Sum(x => x.Count);

      var result = new ReservationStatsDto(
          TotalCount: total,
          PendingPaymentCount: GetCount(ReservationStatus.PendingPayment),
          ConfirmedCount: GetCount(ReservationStatus.Confirmed),
          DepositOnlyCount: depositOnlyCount,      // 🆕
          FullPaidCount: fullPaidCount,            // 🆕
          ActiveCount: GetCount(ReservationStatus.Active),
          CompletedCount: GetCount(ReservationStatus.Completed),
          ExpiredCount: GetCount(ReservationStatus.Expired),
          NoShowCount: GetCount(ReservationStatus.NoShow),
          CancelledCount: GetCount(ReservationStatus.Cancelled)
      );

      return Result<ReservationStatsDto>.Succeed(result);
    }
    catch (Exception _ex)
    {
      return Result<ReservationStatsDto>.Failure(
          500,
          $"İstatistikler alınırken hata oluştu: {_ex.Message}");
    }
  }
}