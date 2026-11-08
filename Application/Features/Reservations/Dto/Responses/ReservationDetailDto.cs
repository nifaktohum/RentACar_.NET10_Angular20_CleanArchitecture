namespace Application.Features.Reservations.Dto.Responses;

public sealed record ReservationDetailDto(
    Guid Id,
    string ReservationNumber,

    // Müşteri
    Guid CustomerId,
    string CustomerFullName,
    string CustomerPhone,
    string CustomerEmail,

    // Araç
    Guid VehicleId,
    string VehicleBrand,
    string VehicleModel,
    string VehiclePlate,

    // Şubeler
    string PickupBranchName,
    string? ReturnBranchName,

    // Tarihler
    DateTimeOffset PlannedStartDate,
    DateTimeOffset PlannedEndDate,
    int PlannedDays,
    DateTimeOffset? ActualPickupAt,
    DateTimeOffset? ActualReturnAt,

    // Fiyatlandırma
    decimal DailyPrice,
    decimal TotalPrice,
    decimal? BillablePrice,
    decimal DepositAmount,
    decimal RemainingAmount,

    // Ödeme: Depozito
    bool DepositPaid,
    DateTimeOffset? DepositPaidAt,
    string? PaymentId,
    DateTimeOffset? DepositLockDeadline,

    // Ödeme: Tam Ödeme
    bool FullPaymentPaid,
    DateTimeOffset? FullPaymentPaidAt,
    string? FullPaymentId,

    // iade yöntemi 
    bool? IsDepositRefunded,       // İade yapıldı mı?
    decimal? RefundedAmount,       // İade edilen tutar
    decimal? ForfeitedAmount,      // Yanan tutar

    // Durum ve Süreler
    string Status,
    DateTimeOffset PaymentDeadline,
    DateTimeOffset? ConfirmedAt,
    DateTimeOffset? CompletedAt,
    DateTimeOffset? CancelledAt,
    string? CancellationReason,

    // Extralar
    List<RentalExtraDto> Extras,
    decimal ExtrasTotalPrice,

    // Notlar
    string? Notes,
    string? AdminNotes,

    DateTimeOffset CreatedAt
);