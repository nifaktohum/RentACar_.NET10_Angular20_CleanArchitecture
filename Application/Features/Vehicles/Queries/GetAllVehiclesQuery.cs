using Application.Features.Vehicles.Dto;
using Application.Features.Vehicles.Specifications;
using Domain.Repositories.Vehicles;
using Application.Specifications;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TS.Result;
using Application.Common.Models;

namespace Application.Features.Vehicles.Queries;

public sealed record GetAllVehiclesQuery : VehicleSpecParams, IRequest<Result<PagedResponse<VehicleListDto>>>;

public sealed class GetAllVehiclesQueryHandler(
    IVehicleRepository _vehicleRepo,
    ILogger<GetAllVehiclesQueryHandler> _logger
) : IRequestHandler<GetAllVehiclesQuery, Result<PagedResponse<VehicleListDto>>>
{
 public async Task<Result<PagedResponse<VehicleListDto>>> Handle(GetAllVehiclesQuery _req, CancellationToken _token)
{
    try
    {
        _logger.LogInformation("🔍 Tüm araçlar getiriliyor...");

        var spec = new GetAllVehiclesSpecification(_req);

        // 1) Count: Include'suz, sadece filtre
        var countQuery = spec.Criteria != null
            ? _vehicleRepo.Where(spec.Criteria)
            : _vehicleRepo.GetAll();
        var totalCount = await countQuery.CountAsync(_token);

        // 2) Data: spec üzerinden (Include + OrderBy + Skip/Take spec içinde olmalı!)
        var vehicles = await _vehicleRepo.GetAsync(spec, _token);

        // 3) DTO map
        var result = vehicles.Select(v => new VehicleListDto(
            v.Id,
            v.Brand,
            v.Model,
            v.Description ?? "",
            v.Year,
            v.Plate,
            v.Color,
            v.VinNumber,
            v.FuelType,
            v.Transmission,
            v.TractionType,
            v.SeatCount,
            v.DoorCount,
            v.DailyPrice,
            v.CurrentKilometer,
            v.Status.ToString(),
            v.IsAvailable,
            v.VehicleModel?.VehicleType?.Name ?? "Bilinmiyor",
            v.GetMainImageUrl(),
            v.IsActive,
            v.CreatedAt
        )).ToList();

        _logger.LogInformation("✅ {Count} araç getirildi. Toplam: {TotalCount}", result.Count, totalCount);

        var response = new PagedResponse<VehicleListDto>(
            result,
            totalCount,
            _req.PageNumber,
            _req.PageSize
        );

        return Result<PagedResponse<VehicleListDto>>.Succeed(response);
    }
    catch (Exception _ex)
    {
        _logger.LogError(_ex, "❌ Araçlar getirilirken hata oluştu");
        return Result<PagedResponse<VehicleListDto>>.Failure(500, $"Araçlar getirilirken hata oluştu: {_ex.Message}");
    }
}
}