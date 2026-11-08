using Application.Behaviors;
using Application.Features.Reservations.Specifications;
using Domain.Exceptions;
using Domain.Repositories;
using FluentValidation;
using GenericRepository;
using MediatR;
using Microsoft.Extensions.Configuration;
using TS.Result;

namespace Application.Features.Reservations.Commands;

[Permission("Reservations.Update")]
public sealed record PayFullPaymentCommand(
                          Guid ReservationId,
                          string FullPaymentId
                      ) : IRequest<Result<bool>>;

public sealed class PayFullPaymentCommandValidator : AbstractValidator<PayFullPaymentCommand>
{
  public PayFullPaymentCommandValidator()
  {
    RuleFor(x => x.ReservationId)
        .NotEmpty().WithMessage("Rezervasyon ID zorunludur!");

    RuleFor(x => x.FullPaymentId)
        .NotEmpty().WithMessage("Tam ödeme ID zorunludur!");
  }
}

public sealed class PayFullPaymentCommandHandler(
                        IReservationRepository _reservationRepo,
                        IUserRepository _userRepo,
                        IConfiguration _config,
                        IUnitOfWork _unit
                    ) : IRequestHandler<PayFullPaymentCommand, Result<bool>>
{
  public async Task<Result<bool>> Handle(PayFullPaymentCommand _req, CancellationToken _token)
  {
    try
    {
      // 1. Rezervasyonu bul
      var spec = new ReservationByIdSpecification(_req.ReservationId);
      var reservation = await _reservationRepo.GetFirstAsync(spec, _token);

      if (reservation == null)
        return Result<bool>.Failure("Rezervasyon bulunamadı!");

      // 2. User ID
      var userId = _userRepo.GetCurrentUserId();
      if (userId == Guid.Empty) userId = Guid.Parse(_config["SeedData:AdminUserId"]!);

      // 3. Domain metodunu çağır
      // İçeride: Status == Confirmed kontrolü, Depozito ödenmiş mi kontrolü, 
      // 24 saatlik süre dolmuş mu kontrolü ve FullPaymentPaid = true ataması var.
      reservation.MarkFullPaymentPaid(_req.FullPaymentId, userId);

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