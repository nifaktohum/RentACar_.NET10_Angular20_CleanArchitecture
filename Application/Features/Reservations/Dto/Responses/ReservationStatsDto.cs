namespace Application.Features.Reservations.Dto.Responses;

public sealed record ReservationStatsDto(
    int TotalCount,
    int PendingPaymentCount,
    int ConfirmedCount,
    int DepositOnlyCount,      // 🆕 Sadece kapora alınmış
    int FullPaidCount,         // 🆕 Tam ödemesi alınmış
    int ActiveCount,
    int CompletedCount,
    int ExpiredCount,
    int NoShowCount,
    int CancelledCount
);