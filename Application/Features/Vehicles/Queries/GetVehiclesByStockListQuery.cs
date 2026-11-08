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
      _logger.LogInformation("🔍 Admin paneli stok raporu sorgulanıyor. Min Stok: {MinStock}, Max Stok: {MaxStock}",
          _req.MinStock?.ToString() ?? "Yok",
          _req.MaxStock?.ToString() ?? "Yok");

      var spec = new VehicleModelByStockSpecification(_req.MinStock, _req.MaxStock, _req.Sort);

      var query = spec.Criteria != null
          ? _vehicleModelRepo.Where(spec.Criteria)
          : _vehicleModelRepo.GetAll();

      // 🆕 KRİTİK: Include yapmazsak Stock ve AvailableStock her zaman 0 döner!
      query = query.Include(vm => vm.Vehicles);

      // Sıralama
      if (spec.OrderBy != null)
        query = query.OrderBy(spec.OrderBy);
      else if (spec.OrderByDescending != null)
        query = query.OrderByDescending(spec.OrderByDescending);
      else
        query = query.OrderBy(vm => vm.Brand).ThenBy(vm => vm.Name); // Varsayılan Sıralama

      // Veriyi Çek
      var vehicleModels = await query.ToListAsync(_token);

      _logger.LogInformation("✅ {Count} adet araç modeli stok kriterlerine göre başarıyla getirildi.", vehicleModels.Count);

      // Entity -> DTO Dönüşümü
      var vehicleDtos = vehicleModels.Select(vm => new VehicleModelStockDto(
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
      _logger.LogError(ex, "❌ Stok bilgisine göre araçlar getirilirken sistemde bir hata oluştu! Min: {Min}, Max: {Max}",
          _req.MinStock?.ToString() ?? "Yok",
          _req.MaxStock?.ToString() ?? "Yok");

      return Result<List<VehicleModelStockDto>>.Failure("Araç stok bilgileri getirilirken bir hata oluştu.");
    }
  }
}