using Application.Behaviors;
using Domain.Entities.Reservations;
using Domain.Repositories;
using Domain.Repositories.Vehicles;
using FluentValidation;
using GenericRepository;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TS.Result;

namespace Application.Features.Vehicles.Commands;

[Permission("Vehicles.Delete")]
public sealed record DeleteVehicleCommand(Guid Id) : IRequest<Result<Unit>>;

public sealed class DeleteVehicleCommandValidator : AbstractValidator<DeleteVehicleCommand>
{
  public DeleteVehicleCommandValidator()
  {
    RuleFor(x => x.Id)
        .NotEmpty().WithMessage("Araç ID boş olamaz!");
  }
}

public sealed class DeleteVehicleCommandHandler(
    IVehicleRepository _vehicleRepo,
    IUserRepository _userRepo,
    IConfiguration _config,
    IUnitOfWork _unit,
    ILogger<DeleteVehicleCommandHandler> _logger
) : IRequestHandler<DeleteVehicleCommand, Result<Unit>>
{
  public async Task<Result<Unit>> Handle(DeleteVehicleCommand _req, CancellationToken _token)
  {
    try
    {
      _logger.LogInformation("🗑️ Araç silme başladı. VehicleId: {VehicleId}", _req.Id);

      // 1. Vehicle var mı kontrol et (Rezervasyonlarıyla birlikte çekiyoruz)
      // Not: Eğer Repository'de özel bir metodun varsa (örn: GetWithReservationsAsync) onu kullan.
      // var vehicle = await _vehicleRepo.FirstOrDefaultAsync(v => v.Id == _req.Id, cancellationToken: _token);

      // EĞER RESERVASYONLARI DAHİL ETMEK İÇİN ÖZEL BİR METOT GEREKİYORSA:
      var vehicle = await _vehicleRepo.GetVehicleWithDetailsAsync(_req.Id, _token); 

      if (vehicle is null)
      {
        _logger.LogWarning("❌ Araç bulunamadı. VehicleId: {VehicleId}", _req.Id);
        return Result<Unit>.Failure(404, "Araç bulunamadı!");
      }

      // 2. İŞ KURALLARI (Domain Kontrolleri)
      // Araç kiradaysa veya bakımdaysa silinemez
      if (vehicle.Status != Domain.Entities.Vehicles.VehicleStatus.Available)
      {
        _logger.LogWarning("⚠️ Araç silinemez, çünkü müsait değil (Durum: {Status}). VehicleId: {VehicleId}", vehicle.Status, _req.Id);
        return Result<Unit>.Failure(400, $"Araç silinemez! Şu anki durumu: {vehicle.Status}. Sadece 'Müsait' (Available) durumdaki araçlar silinebilir.");
      }

      // TODO: Eğer aracın iptal edilmemiş/tamamlanmamış aktif bir rezervasyonu varsa silinmesini engelle.
      // (Repository'nizde Reservation tablosuna erişim varsa buraya bir kontrol eklenmeli)
      if (vehicle.Reservations.Any(r => r.Status == ReservationStatus.Confirmed || r.Status == ReservationStatus.Active))
      {
          return Result<Unit>.Failure(400, "Bu araca ait aktif veya onaylanmış bir rezervasyon bulunduğu için silinemez!");
      }

      // 3. Soft Delete İşlemi
      var userId = _userRepo.GetCurrentUserId();
      if (userId == Guid.Empty)
        userId = Guid.Parse(_config["SeedData:AdminUserId"]!);

      // ❌ _vehicleRepo.Delete(vehicle); -> BU SATIRI SİLDİK! (Hard Delete yapıyordu)

      vehicle.SoftDelete(userId); // ✅ Doğrusu bu. BaseEntity'den geliyor.

      // 4. Save
      // _vehicleRepo.Update(vehicle); // SoftDelete zaten state'i değiştiriyor, UnitOfWork halledecektir.
      await _unit.SaveChangesAsync(_token);

      _logger.LogInformation("✅ Araç başarıyla silindi (Soft Delete). VehicleId: {VehicleId}", _req.Id);

      return Result<Unit>.Succeed(Unit.Value);
    }
    catch (Exception _ex)
    {
      _logger.LogError(_ex, "❌ Araç silme hatası. VehicleId: {VehicleId}", _req.Id);
      return Result<Unit>.Failure(500, $"Araç silinirken bir hata oluştu: {_ex.Message}");
    }
  }
}