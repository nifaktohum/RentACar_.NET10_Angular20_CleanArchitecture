using Application.Behaviors;
using Application.Features.Reservations.Specifications;
using Domain.Entities.Reservations;
using Domain.Entities.Vehicles;
using Domain.Exceptions;
using Domain.Repositories;
using Domain.Repositories.Vehicles;
using FluentValidation;
using GenericRepository;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TS.Result;

namespace Application.Features.Reservations.Commands;

[Permission("Reservations.Cancel")]
public sealed record CancelReservationCommand(
                          Guid ReservationId,
                          string Reason
                      ) : IRequest<Result<bool>>;

public sealed class CancelReservationCommandValidator : AbstractValidator<CancelReservationCommand>
{
  public CancelReservationCommandValidator()
  {
    RuleFor(x => x.ReservationId).NotEmpty().WithMessage("Rezervasyon ID zorunludur!");
    RuleFor(x => x.Reason).NotEmpty().WithMessage("İptal sebebi zorunludur!")
        .MaximumLength(500).WithMessage("İptal sebebi en fazla 500 karakter olabilir!");
  }
}

public sealed class CancelReservationCommandHandler(
                        IReservationRepository _reservationRepo,
                        IUserRepository _userRepo,
                        IVehicleRepository _vehicleRepo,
                        IConfiguration _config,
                        IUnitOfWork _unit,
                        ILogger<CancelReservationCommandHandler> _logger
                    ) : IRequestHandler<CancelReservationCommand, Result<bool>>
{
  public async Task<Result<bool>> Handle(CancelReservationCommand _req, CancellationToken _token)
  {
    try
    {
      // 1. Rezervasyonu bul
      var spec = new ReservationByIdSpecification(_req.ReservationId);
      var reservation = await _reservationRepo.GetFirstAsync(spec, _token);

      if (reservation == null) return Result<bool>.Failure("Rezervasyon bulunamadı!");

      // 2. User ID
      var userId = _userRepo.GetCurrentUserId();
      if (userId == Guid.Empty) userId = Guid.Parse(_config["SeedData:AdminUserId"]!);

      // 3. İptal öncesi durum
      var oldStatus = reservation.Status;

      _logger.LogInformation(
          "❌ Rezervasyon iptal ediliyor. Id: {Id}, Eski Durum: {OldStatus}, Sebep: {Reason}",
          reservation.Id, oldStatus, _req.Reason);

      // 4. Domain metodu (İçeride iade hesabı da yapılıyor)
      reservation.Cancel(_req.Reason, userId);

      // 5. 🚨 İADE BİLGİLERİNİ LOGLA
      if (reservation.DepositPaid)
      {
        _logger.LogInformation(
            "💰 İade Hesabı: Depozito={Deposit} TL, İade Edilen={Refunded} TL, Yanan={Forfeited} TL, Durum={RefundStatus}",
            reservation.DepositAmount,
            reservation.RefundedAmount ?? 0,
            reservation.ForfeitedAmount ?? 0,
            reservation.IsDepositRefunded == true
                ? (reservation.RefundedAmount == reservation.DepositAmount ? "Tam İade" : "Kısmi İade")
                : "Yandı");
      }

      // 6. Stok İadesi / Araç Serbest Bırakma
      if (oldStatus == ReservationStatus.Confirmed || oldStatus == ReservationStatus.Active)
      {
        var vehicle = await _vehicleRepo.GetByIdAsync(reservation.VehicleId, _token);

        if (vehicle != null && vehicle.Status == VehicleStatus.Rented)
        {
          vehicle.Return(vehicle.CurrentKilometer);
          _vehicleRepo.Update(vehicle);

          _logger.LogInformation(
              "📦 Araç serbest bırakıldı ve stoğa döndü. VehicleId: {VehicleId}",
              reservation.VehicleId);
        }
      }

      // 7. Kaydet (Tek Transaction)
      _reservationRepo.Update(reservation);
      await _unit.SaveChangesAsync(_token);

      _logger.LogInformation("✅ Rezervasyon iptali başarılı. Id: {Id}", reservation.Id);

      return Result<bool>.Succeed(true);
    }
    catch (DomainValidationException _ex)
    {
      _logger.LogWarning(_ex, "⚠️ Domain validasyon hatası. Id: {Id}", _req.ReservationId);
      return Result<bool>.Failure(_ex.Message);
    }
    catch (Exception _ex)
    {
      _logger.LogError(_ex, "❌ Rezervasyon iptali hatası. Id: {Id}", _req.ReservationId);
      return Result<bool>.Failure($"Beklenmeyen hata: {_ex.Message}");
    }
  }
}

