using Application.Features.VehicleTypes.Dto;
using Domain.Entities.Vehicles;
using Domain.Repositories.Vehicles;
using GenericRepository;
using MediatR;
using Microsoft.Extensions.Logging;
using TS.Result;

namespace Application.Features.VehicleTypes.Commands;

public sealed record CreateVehicleTypeCommand(
                        string Name,
                        string? Description,
                        string? Icon,
                        int DisplayOrder,
                        Guid CreatedBy
                    ) : IRequest<Result<CreateVehicleTypeDto>>;

public sealed class CreateVehicleTypeCommandHandler(
                          IVehicleTypeRepository _vehicleTypeRepo,
                          IUnitOfWork _unit,
                          ILogger<CreateVehicleTypeCommand> _logger
                    ) : IRequestHandler<CreateVehicleTypeCommand, Result<CreateVehicleTypeDto>>
{
  public async Task<Result<CreateVehicleTypeDto>> Handle(CreateVehicleTypeCommand _req, CancellationToken _token)
  {
    try
    {
      // 1. Aynı isimde type var mı kontrol et
      var isNameUnique = await _vehicleTypeRepo.IsNameUniqueAsync(_req.Name, null, _token);
      if (!isNameUnique)
      {
        _logger.LogWarning("⚠️ Araç tipi eklenemedi. '{Name}' ismi zaten kullanımda.", _req.Name);
        return Result<CreateVehicleTypeDto>.Failure(400, $"'{_req.Name}' ismiyle zaten bir araç tipi mevcut.");
      }

      // 2. Domain entity oluştur
      var vehicleType = new VehicleType(
          _req.Name,
          _req.Description,
          _req.Icon,
          _req.DisplayOrder,
          _req.CreatedBy
      );

      // 3. Repository'ye ekle & kaydet
      await _vehicleTypeRepo.AddAsync(vehicleType, _token);
      await _unit.SaveChangesAsync(_token);

      _logger.LogInformation("✅ Araç tipi başarıyla oluşturuldu. ID: {Id}, Name: {Name}", vehicleType.Id, vehicleType.Name);

      // 4. Response dön
      var response = new CreateVehicleTypeDto(
          vehicleType.Id,
          vehicleType.Name,
          vehicleType.Description,
          vehicleType.Icon,
          vehicleType.DisplayOrder,
          vehicleType.CreatedAt
      );

      return Result<CreateVehicleTypeDto>.Succeed(response);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "❌ Araç tipi oluşturulurken beklenmeyen bir hata oluştu: {Name}", _req.Name);
      return Result<CreateVehicleTypeDto>.Failure(500, $"Araç tipi oluşturulurken bir hata oluştu: {ex.Message}");
    }
    
  }
}