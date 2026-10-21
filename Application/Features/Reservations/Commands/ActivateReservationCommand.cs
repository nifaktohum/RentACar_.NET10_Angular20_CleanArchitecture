using Application.Features.Reservations.Specifications;
using Domain.Exceptions;
using Domain.Repositories;
using FluentValidation;
using GenericRepository;
using MediatR;
using Microsoft.Extensions.Configuration;
using TS.Result;

namespace Application.Features.Reservations.Commands;

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
    IConfiguration _config,
    IUnitOfWork _unit
) : IRequestHandler<ActivateReservationCommand, Result<bool>>
{
  public async Task<Result<bool>> Handle(ActivateReservationCommand _req, CancellationToken _token)
  {
    // 1. Rezervasyonu bul (Araç bilgileriyle birlikte lazım olacak)
    var spec = new ReservationByIdSpecification(_req.ReservationId);
    var reservation = await _reservationRepository.GetFirstAsync(spec, _token);

    if (reservation == null) return Result<bool>.Failure("Rezervasyon bulunamadı!");

    // 2. User ID (Senin standartlarına göre)
    var userId = _userRepo.GetCurrentUserId();
    if (userId == Guid.Empty) userId = Guid.Parse(_config["SeedData:AdminUserId"]!);

    try
    {
      // 3. Domain metodunu çağır (Teslim alma zamanı kontrolleri içeride)
      reservation.MarkAsActive(_req.ActualPickupAt, userId);

      // 4. Kaydet
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
