using Application.Behaviors;
using Application.Features.Reservations.Specifications;
using Domain.Entities.Vehicles; // VehicleStatus için
using Domain.Exceptions;
using Domain.Repositories;
using Domain.Repositories.Vehicles;
using FluentValidation;
using GenericRepository;
using MediatR;
using Microsoft.Extensions.Configuration;
using TS.Result;

namespace Application.Features.Reservations.Commands;

[Permission("Reservations.Update")]
public sealed record CompleteReservationCommand(
                          Guid ReservationId,
                          DateTimeOffset ActualReturnAt,
                          int ReturnKilometer // 🚨 EKLENDİ: İade kilometresi
                      ) : IRequest<Result<bool>>;

public sealed class CompleteReservationCommandValidator : AbstractValidator<CompleteReservationCommand>
{
  public CompleteReservationCommandValidator()
  {
    RuleFor(x => x.ReservationId)
        .NotEmpty().WithMessage("Rezervasyon ID zorunludur!");

    RuleFor(x => x.ActualReturnAt)
        .NotEmpty().WithMessage("İade zamanı zorunludur!");

    RuleFor(x => x.ReturnKilometer)
        .GreaterThanOrEqualTo(0).WithMessage("İade kilometresi geçersiz!");
  }
}

public sealed class CompleteReservationCommandHandler(
                        IReservationRepository _reservationRepo,
                        IUserRepository _userRepo,
                        IVehicleRepository _vehicleRepo,
                        IConfiguration _config,
                        IUnitOfWork _unit
                    ) : IRequestHandler<CompleteReservationCommand, Result<bool>>
{
  public async Task<Result<bool>> Handle(CompleteReservationCommand _req, CancellationToken _token)
  {
    try
    {
      // ⭐ Complete için Extras lazım (BillablePrice hesabı)
      var spec = new ReservationByIdWithDetailsSpecification(_req.ReservationId);
      var reservation = await _reservationRepo.GetFirstAsync(spec, _token);

      if (reservation == null)
        return Result<bool>.Failure("Rezervasyon bulunamadı!");

      var userId = _userRepo.GetCurrentUserId();
      if (userId == Guid.Empty)
        userId = Guid.Parse(_config["SeedData:AdminUserId"]!);

      // 1. Domain metodu (Status, ActualPickupAt kontrolü, BillablePrice hesabı içeride)
      reservation.MarkAsCompleted(_req.ActualReturnAt, userId);

      // 2. Aracı bul
      var vehicle = await _vehicleRepo.GetByIdAsync(reservation.VehicleId, _token);
      if (vehicle == null)
        return Result<bool>.Failure("Araç bulunamadı!");

      // 3. ✅ ARAÇ İADE AL (Stok artırmak yerine aracı stoğa döndür)
      if (vehicle.Status == VehicleStatus.Rented)
      {
        // Status = Available olur, stok otomatik artar, km güncellenir
        vehicle.Return(_req.ReturnKilometer);
        _vehicleRepo.Update(vehicle);
      }

      _reservationRepo.Update(reservation);

      // 4. TEK TRANSACTION: Hem rezervasyon tamamlandı hem araç stoğa döndü
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