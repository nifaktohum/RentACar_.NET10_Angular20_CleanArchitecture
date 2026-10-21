using Application.Features.Reservations.Specifications;
using Domain.Exceptions;
using Domain.Repositories;
using FluentValidation;
using GenericRepository;
using MediatR;
using Microsoft.Extensions.Configuration;
using TS.Result;

namespace Application.Features.Reservations.Commands;

public sealed record CancelReservationCommand(
                          Guid ReservationId,
                          string Reason
                      ) : IRequest<Result<bool>>;

public sealed class CancelReservationCommandValidator : AbstractValidator<CancelReservationCommand>
{
  public CancelReservationCommandValidator()
  {
    RuleFor(x => x.ReservationId)
        .NotEmpty().WithMessage("Rezervasyon ID zorunludur!");

    RuleFor(x => x.Reason)
        .NotEmpty().WithMessage("İptal sebebi zorunludur!")
        .MaximumLength(500).WithMessage("İptal sebebi en fazla 500 karakter olabilir!");
  }
}

public sealed class CancelReservationCommandHandler(
                        IReservationRepository _reservationRepo,
                        IUserRepository _userRepo,
                        IConfiguration _config,
                        IUnitOfWork _unit
                    ) : IRequestHandler<CancelReservationCommand, Result<bool>>
{
  public async Task<Result<bool>> Handle(CancelReservationCommand _req, CancellationToken _token)
  {
    try
    {
      // 1. Rezervasyonu bul (Extras'a gerek yok)
      var spec = new ReservationByIdSpecification(_req.ReservationId);
      var reservation = await _reservationRepo.GetFirstAsync(spec, _token);

      if (reservation == null)return Result<bool>.Failure("Rezervasyon bulunamadı!");

      // 2. User ID
      var userId = _userRepo.GetCurrentUserId();
      if (userId == Guid.Empty) userId = Guid.Parse(_config["SeedData:AdminUserId"]!);

      // 3. Domain metodu (Status kontrolü, iptal sebebi validasyonu içeride)
      reservation.Cancel(_req.Reason, userId);

      // 4. Kaydet
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