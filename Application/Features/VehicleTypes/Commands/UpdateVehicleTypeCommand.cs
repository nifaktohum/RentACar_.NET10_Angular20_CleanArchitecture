using Application.Features.VehicleTypes.Dto;
using Domain.Repositories;
using Domain.Repositories.Vehicles;
using GenericRepository;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TS.Result;

namespace Application.Features.VehicleTypes.Commands;

public sealed record UpdateVehicleTypeCommand(
                        Guid Id,
                        string Name,
                        string? Description,
                        string? Icon,
                        int DisplayOrder
                    ) : IRequest<Result<UpdateVehicleTypeDto>>;
public sealed class UpdateVehicleTypeCommandHandler(
                        IVehicleTypeRepository _vehicleTypeRepo,
                        IUserRepository _userRepo,
                        IUnitOfWork _unit,
                        ILogger<UpdateVehicleTypeCommandHandler> _logger
                    ) : IRequestHandler<UpdateVehicleTypeCommand, Result<UpdateVehicleTypeDto>>
{
  public async Task<Result<UpdateVehicleTypeDto>> Handle(UpdateVehicleTypeCommand _req, CancellationToken _token)
  {
    try
    {
      _logger.LogInformation("📝 ID: {Id} olan araç tipi güncelleniyor...", _req.Id);

      // 1. Kayıt var mı kontrol et
      var vehicleType = await _vehicleTypeRepo
                                    .GetAll()
                                    .FirstOrDefaultAsync(vt => vt.Id == _req.Id, _token);

      if (vehicleType is null)
      {
        _logger.LogWarning("⚠️ Güncellenecek araç tipi bulunamadı. ID: {Id}", _req.Id);
        return Result<UpdateVehicleTypeDto>.Failure(404, "Güncellenmek istenen araç tipi bulunamadı.");
      }

      // 2. İsim benzersizlik kontrolü (Kendi ID'si hariç)
      var isNameUnique = await _vehicleTypeRepo.IsNameUniqueAsync(_req.Name, _req.Id, _token);
      if (!isNameUnique)
      {
        _logger.LogWarning("⚠️ Araç tipi güncellenemedi. '{Name}' ismi başka bir kayıtta kullanılıyor.", _req.Name);
        return Result<UpdateVehicleTypeDto>.Failure(400, $"'{_req.Name}' ismiyle başka bir araç tipi zaten mevcut.");
      }

      // 3. Oturum açan kullanıcı ve zaman bilgilerini al
      var userId = _userRepo.GetCurrentUserId(); ;

      vehicleType.UpdateDetails(
          _req.Name,
          _req.Description,
          _req.Icon,
          _req.DisplayOrder
      );

      vehicleType.UpdateMetadata(userId);

      _vehicleTypeRepo.Update(vehicleType);
      await _unit.SaveChangesAsync(_token);

      var userName = await _userRepo.GetUserNamesByIdsAsync([userId], _token);

      string UpdatedByName(Guid userId) => userName.GetValueOrDefault(userId, "Bilinmiyor");

      // 5. DTO Nesnesini Oluştur ve Dön
      var responseDto = new UpdateVehicleTypeDto(
          vehicleType.Id,
          vehicleType.Name,
          vehicleType.Description,
          vehicleType.Icon,
          vehicleType.DisplayOrder,
          vehicleType.UpdatedAt,
          vehicleType.UpdatedBy,
          vehicleType.UpdatedBy.HasValue ? UpdatedByName(vehicleType.UpdatedBy.Value) : null
      );


      _logger.LogInformation("✅ ID: {Id} olan araç tipi başarıyla güncellendi.", _req.Id);
      return Result<UpdateVehicleTypeDto>.Succeed(responseDto);
    }
    catch (Exception _ex)
    {
      _logger.LogError(_ex, "❌ ID: {Id} olan araç tipi güncellenirken hata oluştu", _req.Id);
      return Result<UpdateVehicleTypeDto>.Failure(500, $"Araç tipi güncellenirken hata oluştu: {_ex.Message}");
    }
  }
}