using Application.Behaviors;
using Application.Features.Reservations.Specifications;
using Domain.Entities.Vehicles; // VehicleStatus için
using Domain.Exceptions;
using Domain.Repositories;
using Domain.Repositories.Vehicles; // 🚨 EKLENDİ
using FluentValidation;
using GenericRepository;
using MediatR;
using Microsoft.Extensions.Configuration;
using TS.Result;

namespace Application.Features.Reservations.Commands;

[Permission("Reservations.Update")]
// * Aracın müşteriye fiilen teslim edilmesi
public sealed record ActivateReservationCommand(
                        Guid ReservationId,
                        DateTimeOffset ActualPickupAt
                    ) : IRequest<Result<bool>>;

public sealed class ActivateReservationCommandValidator : AbstractValidator<ActivateReservationCommand>
{
  public ActivateReservationCommandValidator()
  {
    RuleFor(x => x.ReservationId)
        .NotEmpty().WithMessage("Rezervasyon ID zorunludur!");

    RuleFor(x => x.ActualPickupAt)
        .NotEmpty().WithMessage("Teslim alma zamanı zorunludur!");
  }
}

public sealed class ActivateReservationCommandHandler(
    IReservationRepository _reservationRepository,
    IUserRepository _userRepo,
    IVehicleRepository _vehicleRepo, // 🚨 EKLENDİ
    IConfiguration _config,
    IUnitOfWork _unit
) : IRequestHandler<ActivateReservationCommand, Result<bool>>
{
  public async Task<Result<bool>> Handle(ActivateReservationCommand _req, CancellationToken _token)
  {
    // 1. Rezervasyonu bul
    var spec = new ReservationByIdSpecification(_req.ReservationId);
    var reservation = await _reservationRepository.GetFirstAsync(spec, _token);

    if (reservation == null) return Result<bool>.Failure("Rezervasyon bulunamadı!");

    // 2. User ID
    var userId = _userRepo.GetCurrentUserId();
    if (userId == Guid.Empty) userId = Guid.Parse(_config["SeedData:AdminUserId"]!);

    try
    {
      // 3. Domain metodunu çağır (Durum ve tam ödeme kontrolleri içeride)
      reservation.MarkAsActive(_req.ActualPickupAt, userId);

      // 4. Aracı bul ve fiziksel olarak kiraya ver (Mühürle)
      var vehicle = await _vehicleRepo.GetByIdAsync(reservation.VehicleId, _token);
      if (vehicle == null) return Result<bool>.Failure("Araç bulunamadı!");

      if (vehicle.Status == VehicleStatus.Available)
      {
        vehicle.Rent(); // Status = Rented (Araç artık fiziki olarak kirada, stok düştü)
        _vehicleRepo.Update(vehicle);
      }

      // 5. Kaydet (Tek Transaction: Rezervasyon aktifleşti + Araç fiziki olarak kiraya verildi)
      _reservationRepository.Update(reservation);
      await _unit.SaveChangesAsync(_token);

      return Result<bool>.Succeed(true);
    }
    catch (DomainValidationException _ex)
    {
      return Result<bool>.Failure(_ex.Message);
    }
    catch (Exception _ex)
    {
      return Result<bool>.Failure($"Beklenmeyen hata: {_ex.Message}");
    }
  }
}