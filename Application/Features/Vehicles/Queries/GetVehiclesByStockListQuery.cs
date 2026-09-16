using Application.Features.VehicleModels.Dto;
using Application.Features.Vehicles.Specifications;
using Domain.Repositories.Vehicles;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TS.Result;

namespace Application.Features.Vehicles.Queries;

public sealed record GetVehiclesByStockList(int? MinStock, int? MaxStock, string? Sort) : IRequest<Result<List<VehicleModelStockDto>>>;


public sealed class GetVehiclesByStockQueryHandler(
                        IVehicleModelRepository _vehicleModelRepo,
                        ILogger<GetVehiclesByStockQueryHandler> _logger
                    ) : IRequestHandler<GetVehiclesByStockList, Result<List<VehicleModelStockDto>>>
{
  public async Task<Result<List<VehicleModelStockDto>>> Handle(GetVehiclesByStockList _req, CancellationToken _token)
  {
    try
    {
      // 1. Türkçe Bilgi Logu (Başlangıç)
      _logger.LogInformation("Admin paneli stok raporu sorgulanıyor. Min Stok: {MinStock}, Max Stok: {MaxStock}",
          _req.MinStock?.ToString() ?? "Yok",
          _req.MaxStock?.ToString() ?? "Yok");

      // 2. Specification oluştur (VehicleModel için)
      var spec = new VehicleModelByStockSpecification(_req.MinStock, _req.MaxStock, _req.Sort);

      // 3. Query'yi hazırla ve çalıştır
      // Eğer criteria varsa filtrele, yoksa tüm modelleri al
      var query = spec.Criteria != null
          ? _vehicleModelRepo.Where(spec.Criteria)
          : _vehicleModelRepo.GetAll();

      var que = await query
           .OrderBy(vm => vm.Brand)
           .ThenBy(vm => vm.Name)
           .ToListAsync(_token);

      // Specification içinde belirlenen sıralama kuralını sorguya uyguluyoruz
      if (spec.OrderBy != null)
      {
        query = query.OrderBy(spec.OrderBy);
      }
      else if (spec.OrderByDescending != null)
      {
        query = query.OrderByDescending(spec.OrderByDescending);
      }

      // 4. Log - Başarılı
      _logger.LogInformation(
                "{Count} adet araç modeli stok kriterlerine göre başarıyla getirildi.",
                que.Count
            );

      // Entity -> DTO Dönüşümü
      var vehicleDtos = query.Select(vm => new VehicleModelStockDto(
                vm.Id,
                vm.Name,
                vm.Brand,
                vm.Description,
                vm.Stock,
                vm.AvailableStock,
                vm.IsInStock
            )).ToList();

      return Result<List<VehicleModelStockDto>>.Succeed(vehicleDtos);
    }
    catch (Exception ex)
    {
      // 3. Türkçe Hata Logu
      _logger.LogError(ex, "Stok bilgisine göre araçlar getirilirken sistemde bir hata oluştu! Min: {Min}, Max: {Max}",
          _req.MinStock?.ToString() ?? "Yok",
          _req.MaxStock?.ToString() ?? "Yok");

      return Result<List<VehicleModelStockDto>>.Failure("Araç stok bilgileri getirilirken bir hata oluştu.");
    }
  }
}
