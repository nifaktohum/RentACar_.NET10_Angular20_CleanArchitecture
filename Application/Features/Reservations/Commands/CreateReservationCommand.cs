using Application.Behaviors;
using Application.Features.Reservations.Dto.Requests;
using Application.Features.Reservations.Specifications;
using Application.Services;
using Domain.Entities.Extras;
using Domain.Entities.Reservations;
using Domain.Exceptions;
using Domain.Repositories;
using Domain.Repositories.Extras;
using Domain.Repositories.Vehicles;
using FluentValidation;
using GenericRepository;
using MediatR;
using Microsoft.Extensions.Configuration;
using TS.Result;

namespace Application.Features.Reservations.Commands;

[Permission("Reservations.Create")]
public sealed record CreateReservationCommand(
                        Guid CustomerId,
                        Guid VehicleId,
                        Guid PickupBranchId,
                        Guid? ReturnBranchId,
                        DateTimeOffset PlannedStartDate,
                        DateTimeOffset PlannedEndDate,
                        string? Notes,
                        List<CreateReservationExtraDto> Extras
                    ) : IRequest<Result<Guid>>;

public sealed class CreateReservationCommandValidator : AbstractValidator<CreateReservationCommand>
{
  public CreateReservationCommandValidator()
  {
    RuleFor(x => x.CustomerId).NotEmpty().WithMessage("Müşteri ID zorunludur!");
    RuleFor(x => x.VehicleId).NotEmpty().WithMessage("Araç ID zorunludur!");
    RuleFor(x => x.PickupBranchId).NotEmpty().WithMessage("Alış şubesi zorunludur!");
    RuleFor(x => x.PlannedStartDate).LessThan(x => x.PlannedEndDate).WithMessage("Başlangıç tarihi bitiş tarihinden önce olmalıdır!");
    RuleForEach(x => x.Extras).ChildRules(extra =>
    {
      extra.RuleFor(e => e.Quantity).GreaterThan(0).WithMessage("Extra miktarı 0'dan büyük olmalıdır!");
    });
  }
}

public sealed class CreateReservationCommandHandler(
                          ICustomerRepository _customerRepo,
                          IVehicleRepository _vehicleRepo,
                          IReservationRepository _reservationRepo,
                          IExtraRepository _extraRepo,
                          IReservationNumberGenerator _reservationNumberGenerator,
                          IUserRepository _userRepo,
                          IConfiguration _config,
                          IUnitOfWork _unit
                    ) : IRequestHandler<CreateReservationCommand, Result<Guid>>
{
  public async Task<Result<Guid>> Handle(CreateReservationCommand _req, CancellationToken _token)
  {
    try
    {
      // 1. Müşteri Kontrolü
      var customer = await _customerRepo.GetByExpressionAsync(c => c.Id == _req.CustomerId, _token);
      if (customer == null) throw new DomainValidationException("Müşteri bulunamadı!");
      if (!customer.CanRent()) throw new DomainValidationException("Müşteri araç kiralamak için uygun değil!");

      // 2. Araç Kontrolü
      var vehicle = await _vehicleRepo.GetVehicleWithDetailsAsync(_req.VehicleId, _token);
      if (vehicle == null) throw new DomainValidationException("Araç bulunamadı!");

      // 🚨 DÜZELTME: Anlık durum değil, operasyonel uygunluk kontrolü
      if (!vehicle.CanBeReservedNow)
        throw new DomainValidationException("Araç şu anda rezervasyon için uygun değil!");

      // 3. Tarih Çakışma Kontrolü (Spec içerisinde 3 katmanlı kilit kontrolü var)
      var conflictSpec = new ConflictingReservationsSpecification(_req.VehicleId, _req.PlannedStartDate, _req.PlannedEndDate);
      var conflicts = await _reservationRepo.GetAsync(conflictSpec, _token);
      if (conflicts.Any()) throw new DomainValidationException("Araç bu tarih aralığında müsait değil!");

      // 4. Extraları Kontrol Et
      var extraIds = _req.Extras.Select(e => e.ExtraId).ToList();
      var extras = await _extraRepo.GetByIdsAsync(extraIds, _token);

      // 5. User ID
      var userId = _userRepo.GetCurrentUserId();
      if (userId == Guid.Empty) userId = Guid.Parse(_config["SeedData:AdminUserId"]!);

      // 6. Numara Üret
      var reservationNumber = await _reservationNumberGenerator.GenerateAsync(_token);

      // 7. Reservation Oluştur
      var reservation = new Reservation(
          reservationNumber: reservationNumber,
          customerId: _req.CustomerId,
          vehicleId: _req.VehicleId,
          pickupBranchId: _req.PickupBranchId,
          returnBranchId: _req.ReturnBranchId,
          plannedStartDate: _req.PlannedStartDate,
          plannedEndDate: _req.PlannedEndDate,
          dailyPrice: vehicle.DailyPrice,
          notes: _req.Notes,
          createdBy: userId
      );

      // Aracı rezervasyona ata (Null referans hatalarını önler)
      reservation.SetVehicle(vehicle);

      // 8. Extraları Ekle & Stok Düş
      foreach (var item in _req.Extras)
      {
        var extra = extras.FirstOrDefault(e => e.Id == item.ExtraId);
        if (extra == null) throw new DomainValidationException($"Ekstra ürün bulunamadı! (ID: {item.ExtraId})");

        if (!extra.IsInStock()) throw new DomainValidationException($"{extra.Name} stokta yok!");

        var rentalExtra = new RentalExtra(
            rentalId: reservation.Id,
            extraId: extra.Id,
            unitPrice: extra.Price,
            quantity: item.Quantity,
            rentalDayCount: reservation.PlannedDays,
            priceType: extra.PriceType,
            createdBy: userId
        );

        reservation.AddExtra(rentalExtra);
        extra.DecreaseStock(item.Quantity);
      }

      // 9. Kaydet
      await _reservationRepo.AddAsync(reservation, _token);
      await _unit.SaveChangesAsync(_token);

      return Result<Guid>.Succeed(reservation.Id);
    }
    catch (DomainValidationException ex)
    {
      // Domain hatalarını yakalayıp 400 dönelim
      return Result<Guid>.Failure(ex.Message);
    }
    catch (Exception ex)
    {
      return Result<Guid>.Failure($"Rezervasyon oluşturulurken hata oluştu: {ex.Message}");
    }
  }
}