using Application.Features.Reservations.Specifications;
using Domain.Exceptions;
using Domain.Repositories;
using FluentValidation;
using GenericRepository;
using MediatR;
using Microsoft.Extensions.Configuration;
using TS.Result;

namespace Application.Features.Reservations.Commands;

public sealed record CompleteReservationCommand(
                          Guid ReservationId,
                          DateTimeOffset ActualReturnAt
                      ) : IRequest<Result<bool>>;

public sealed class CompleteReservationCommandValidator : AbstractValidator<CompleteReservationCommand>
{
  public CompleteReservationCommandValidator()
  {
    RuleFor(x => x.ReservationId)
        .NotEmpty().WithMessage("Rezervasyon ID zorunludur!");

    RuleFor(x => x.ActualReturnAt)
        .NotEmpty().WithMessage("İade zamanı zorunludur!");
  }
}

public sealed class CompleteReservationCommandHandler(
                        IReservationRepository _reservationRepo,
                        IUserRepository _userRepo,
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
      if (reservation == null) return Result<bool>.Failure("Rezervasyon bulunamadı!");

      var userId = _userRepo.GetCurrentUserId();
      if (userId == Guid.Empty) userId = Guid.Parse(_config["SeedData:AdminUserId"]!);

      // Domain metodu (Status, ActualPickupAt kontrolü, BillablePrice hesabı içeride)
      reservation.MarkAsCompleted(_req.ActualReturnAt, userId);

      _reservationRepo.Update(reservation);
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