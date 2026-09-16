using Application.Features.VehicleModels.Queries;
using Application.Features.Vehicles.Commands;
using Application.Features.Vehicles.Queries;
using Application.Specifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Vehicles;

[ApiExplorerSettings(GroupName = "v1-Vehicles")]
[Authorize] // Admin yetkisi zorunlu!
public sealed class VehiclesController : BaseApiController
{

  // GET: Tüm araçları getir (Admin)
  [HttpGet("get-all")]
  [AllowAnonymous] // "Bu kapıdan geçecek kişilere kimlik sorma, direkt içeri al"
  public async Task<IActionResult> GetAll([FromQuery] GetAllVehiclesQuery commands, CancellationToken token)
  {
    // var query = new GetAllVehiclesQuery(pagingParams);
    var result = await Mediator.Send(commands, token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  // GET: ID'ye göre araç getir (DÜZELTİLDİ)
  // [HttpGet("get-by-id/{id:guid}")]
  // public async Task<IActionResult> GetById(Guid id, CancellationToken token)
  // {
  //   var query = new GetVehicleByIdQuery(id);
  //   var result = await Mediator.Send(query, token);
  //   return result.IsSuccessful ? Ok(result) : BadRequest(result);
  // }

  // GET: Araç tipine göre listeleme
  [HttpGet("by-type/{id:guid}")]
  [AllowAnonymous]
  public async Task<IActionResult> GetByType(Guid id, [FromQuery] VehicleSpecParams? specParams, CancellationToken token)
  {
    var query = new GetVehicleByTypeQuery(id, specParams);
    var result = await Mediator.Send(query, token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }


  // Tüm araç tiplerini ve altındaki modelleri hiyerarşik olarak getirir
  [HttpGet("types-with-models")]
  [AllowAnonymous]
  public async Task<IActionResult> GetTypesWithModels([FromQuery] GetVehicleTypeWithModelsQuery query, CancellationToken _token)
  {
    var result = await Mediator.Send(query, _token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  // POST: Yeni araç oluşturma
  [HttpPost("create")]
  public async Task<IActionResult> Create([FromForm] CreateVehicleCommand command, CancellationToken token)
  {
    var result = await Mediator.Send(command, token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  // PUT: Araç güncelleme
  [HttpPut("update")]
  public async Task<IActionResult> Update([FromBody] UpdateVehicleCommand command, CancellationToken token)
  {
    var result = await Mediator.Send(command, token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  // DELETE: Araç silme
  [HttpDelete("delete/{id:guid}")]
  public async Task<IActionResult> Delete(Guid id, CancellationToken token)
  {
    var command = new DeleteVehicleCommand(id);
    var result = await Mediator.Send(command, token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  [HttpGet("available-by-brand")]
  [AllowAnonymous]
  public async Task<IActionResult> GetAvailableByBrand([FromQuery] GetAvailableVehiclesByBrandQuery query, CancellationToken token)
  {
    var result = await Mediator.Send(query, token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }


  [HttpGet("by-brand")]
  [AllowAnonymous]
  public async Task<IActionResult> GetByBrand([FromQuery] GetVehiclesByBrandQuery query, CancellationToken token)
  {
    var result = await Mediator.Send(query, token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  // PATCH: Stok güncelleme
  [HttpPatch("update-stock")]
  public async Task<IActionResult> UpdateStock([FromBody] UpdateVehicleStockCommand command, CancellationToken cancellationToken)
  {
    var result = await Mediator.Send(command, cancellationToken);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  // PATCH: Durum değiştirme (Aktif/Pasif)
  [HttpPatch("toggle-status/{id:guid}")]
  public async Task<IActionResult> ToggleStatus(Guid id, CancellationToken cancellationToken)
  {
    var command = new ToggleVehicleStatusCommand(id);
    var result = await Mediator.Send(command, cancellationToken);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }


  // ==================== YENİ SPECIFICATION ENDPOINT'LER ====================

  // GET: Müsait araçlar
  [HttpGet("available")]
  [AllowAnonymous]
  public async Task<IActionResult> GetAvailable([FromQuery] GetAvailableVehiclesQuery query, CancellationToken token)
  {
    var result = await Mediator.Send(query, token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  // GET: Model'e göre musait araçlar
  [HttpGet("available-by-model")]
  [AllowAnonymous]
  public async Task<IActionResult> GetAvailableByModel([FromQuery] GetAvailableVehiclesByModelQuery query, CancellationToken token)
  {
    var result = await Mediator.Send(query, token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  // GET: Tüm Araçları Model'e göre sırala
  [HttpGet("by-model")]
  [AllowAnonymous]
  public async Task<IActionResult> GetByModel([FromQuery] GetVehiclesByModelQuery query, CancellationToken token)
  {
    var result = await Mediator.Send(query, token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  // GET: Musait Araçları Fiyat aralıgına göre sırala
  [HttpGet("available-by-price-range")]
  [AllowAnonymous]
  public async Task<IActionResult> GetAvailableByPriceRange([FromQuery] GetAvailableVehiclesByPriceRangeQuery query, CancellationToken token)
  {
    var result = await Mediator.Send(query, token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  // GET: Tüm Araçları Fiyat aralıgına göre sırala
  [HttpGet("by-price-range")]
  [AllowAnonymous]
  public async Task<IActionResult> GetByPriceRange([FromQuery] GetVehiclesByPriceRangeQuery query, CancellationToken token)
  {
    var result = await Mediator.Send(query, token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  // 1️⃣1️⃣ GET: Filtreleme + Sayfalama
  // [HttpGet("filter")]
  // [AllowAnonymous]
  // public async Task<IActionResult> GetFiltered(
  //   [FromQuery] string? brand = null,
  //   [FromQuery] string? model = null,
  //   [FromQuery] Guid? vehicleTypeId = null,
  //   [FromQuery] decimal? minPrice = null,
  //   [FromQuery] decimal? maxPrice = null,
  //   [FromQuery] bool? isAvailable = null,
  //   [FromQuery] string? fuelType = null,
  //   [FromQuery] string? transmission = null,
  //   [FromQuery] int? minYear = null,
  //   [FromQuery] int? maxYear = null,
  //   [FromQuery] int? minSeatCount = null,
  //   [FromQuery] string? sortBy = null,
  //   [FromQuery] bool descending = false,
  //   [FromQuery] int pageNumber = 1,
  //   [FromQuery] int pageSize = 10,
  //   [FromQuery] bool includeDeleted = false,
  //   CancellationToken token = default)
  // {
  //   var query = new GetFilteredVehiclesQuery(
  //       brand, model, vehicleTypeId, minPrice, maxPrice,
  //       isAvailable, fuelType, transmission, minYear, maxYear,
  //       minSeatCount, sortBy, descending, pageNumber, pageSize,
  //       includeDeleted);

  //   var result = await Mediator.Send(query, token);
  //   return result.IsSuccessful ? Ok(result) : BadRequest(result);
  // }

  // 1️⃣2️⃣ GET: DTO olarak araç listesi (Müşteri için)
  // [HttpGet("list-dto")]
  // [AllowAnonymous]
  // public async Task<IActionResult> GetListDto(
  //       [FromQuery] PagingParams pagingParams,
  //       [FromQuery] bool onlyAvailable = true,
  //       [FromQuery] bool includeDeleted = false,
  //       CancellationToken token = default)
  // {
  //   var query = new GetVehicleListDtoQuery(pagingParams, onlyAvailable, includeDeleted);
  //   var result = await Mediator.Send(query, token);
  //   return result.IsSuccessful ? Ok(result) : BadRequest(result);
  // }

  // 1️⃣3️⃣ GET: Admin DTO listesi
  // [HttpGet("admin-list")]
  // [Authorize(Roles = "Admin")]
  // public async Task<IActionResult> GetAdminList(
  //     [FromQuery] bool showDeleted = false,
  //     CancellationToken token = default)
  // {
  //   var query = new GetVehicleAdminListQuery(showDeleted);
  //   var result = await Mediator.Send(query, token);
  //   return result.IsSuccessful ? Ok(result) : BadRequest(result);
  // }

  // 1️⃣4️⃣ GET: Araç detayı (DTO)
  [HttpGet("get-by-id/{id:guid}")]
  [AllowAnonymous]
  public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken token)
  {
    var query = new GetVehicleByIdQuery(id);
    var result = await Mediator.Send(query, token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  // 1️⃣5️⃣ GET: Araç özeti
  // [HttpGet("summary")]
  // [AllowAnonymous]
  // public async Task<IActionResult> GetSummary(
  //     [FromQuery] bool onlyAvailable = true,
  //     CancellationToken token = default)
  // {
  //   var query = new GetVehicleSummaryQuery(onlyAvailable);
  //   var result = await Mediator.Send(query, token);
  //   return result.IsSuccessful ? Ok(result) : BadRequest(result);
  // }

  // 1️⃣6️⃣ GET: Stok bilgisi
  // [HttpGet("stock/{id:guid}")]
  // [Authorize(Roles = "Admin")]
  // public async Task<IActionResult> GetStock(Guid id, CancellationToken token)
  // {
  //   var query = new GetVehicleStockQuery(id);
  //   var result = await Mediator.Send(query, token);
  //   return result.IsSuccessful ? Ok(result) : BadRequest(result);
  // }

  [HttpGet("by-stock-list")]
  public async Task<IActionResult> GetByStock([FromQuery] GetVehiclesByStockList query, CancellationToken token)
  {
    var result = await Mediator.Send(query, token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  // 1️⃣7️⃣ GET: Silinmiş araçlar (Admin)
  // [HttpGet("deleted")]
  // [Authorize(Roles = "Admin")]
  // public async Task<IActionResult> GetDeleted(CancellationToken token)
  // {
  //   var query = new GetDeletedVehiclesQuery();
  //   var result = await Mediator.Send(query, token);
  //   return result.IsSuccessful ? Ok(result) : BadRequest(result);
  // }

  // 1️⃣8️⃣ GET: Plaka kontrolü
  [HttpGet("by-plate/{plate}")]
  public async Task<IActionResult> GetByPlate([FromRoute] string plate, CancellationToken token)
  {
    var query = new GetVehicleByPlateQuery(plate);
    var result = await Mediator.Send(query, token);

    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  [HttpGet("by-fuel-type")]
  public async Task<IActionResult> GetByFuelType([FromQuery] string? fuelType, [FromQuery] VehicleSpecParams? specParams, CancellationToken cancellationToken)
  {
    var query = new GetVehiclesByFuelTypeQuery(fuelType, specParams);
    var result = await Mediator.Send(query, cancellationToken);
    return StatusCode(result.StatusCode, result);
  }

  [HttpGet("by-transmission")]
  public async Task<IActionResult> GetByTransmission([FromQuery] string? transmission, [FromQuery] VehicleSpecParams? specParams, CancellationToken cancellationToken)
  {
    var query = new GetVehiclesByTransmissionQuery(transmission, specParams);
    var result = await Mediator.Send(query, cancellationToken);
    return StatusCode(result.StatusCode, result);
  }

  [HttpGet("by-featured")]
  [AllowAnonymous]
  public async Task<IActionResult> GetByFeatured([FromQuery] GetFeaturedVehiclesQuery query, CancellationToken cancellationToken)
  {

    var result = await Mediator.Send(query, cancellationToken);
    return StatusCode(result.StatusCode, result);
  }

  [HttpGet("by-latest")]
  [AllowAnonymous]
  public async Task<IActionResult> GetByLatest([FromQuery] GetLatestVehiclesQuery query, CancellationToken cancellationToken)
  {
    var result = await Mediator.Send(query, cancellationToken);
    return StatusCode(result.StatusCode, result);
  }

  [HttpGet("by-recommended")]
  [AllowAnonymous]
  public async Task<IActionResult> GetByRecommended([FromQuery] GetRecommendedVehiclesQuery query, CancellationToken cancellationToken)
  {
    var result = await Mediator.Send(query, cancellationToken);
    return StatusCode(result.StatusCode, result);
  }

  [HttpGet("admin-list")]
  public async Task<IActionResult> GetAdminList([FromQuery] GetAdminVehicleListQuery query ,CancellationToken cancellationToken)
  {
    var result = await Mediator.Send(query, cancellationToken);
    return StatusCode(result.StatusCode, result);
  }

  [HttpGet("statistics")]
  public async Task<IActionResult> GetStatistics(CancellationToken cancellationToken)
  {
    var query = new GetVehicleStatisticsQuery();
    var result = await Mediator.Send(query, cancellationToken);
    return StatusCode(result.StatusCode, result);
  }

  [HttpGet("for-report")]
  public async Task<IActionResult> GetForReport([FromQuery] GetVehiclesForReportQuery query, CancellationToken cancellationToken)
  {
    var result = await Mediator.Send(query, cancellationToken);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  [HttpGet("by-year")] // Yıla göre filtreleme
  public async Task<IActionResult> GetByYear([FromQuery] int? year, CancellationToken cancellationToken)
  {
    var query = new GetVehiclesByYearQuery(year);
    var result = await Mediator.Send(query, cancellationToken);
    return StatusCode(result.StatusCode, result);
  }

  [HttpGet("by-color")]
  public async Task<IActionResult> GetByColor([FromQuery] string? color, CancellationToken cancellationToken)
  {
    var query = new GetVehiclesByColorQuery(color);
    var result = await Mediator.Send(query, cancellationToken);
    return StatusCode(result.StatusCode, result);
  }

  [HttpGet("by-seatcount")]
  public async Task<IActionResult> GetBySeatCount([FromQuery] int? seatCount, CancellationToken cancellationToken)
  {
    var query = new GetVehiclesBySeatCountQuery(seatCount);
    var result = await Mediator.Send(query, cancellationToken);
    return StatusCode(result.StatusCode, result);
  }

  // ==================== DİSTİNCT SPECIFICATION ENDPOINT'LER ====================

  [HttpGet("model-distinct")]
  [AllowAnonymous]
  public async Task<IActionResult> GetDistinctModels(CancellationToken cancellationToken)
  {
    var result = await Mediator.Send(new GetDistinctModelsQuery(), cancellationToken);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  // GET: Distinct Araç Marka listesi
  [HttpGet("brand-distinct")]
  [AllowAnonymous]
  public async Task<IActionResult> GetDistinctBrands(CancellationToken cancellationToken)
  {
    var result = await Mediator.Send(new GetDistinctBrandsQuery(), cancellationToken);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  // GET: Distinct Araç Yakıt türleri listesi
  [HttpGet("fuel-type-distinct")]
  [AllowAnonymous]
  public async Task<IActionResult> GetDistinctFuelTypes(CancellationToken cancellationToken)
  {
    var result = await Mediator.Send(new GetDistinctFuelTypesQuery(), cancellationToken);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  [HttpGet("transmission-distinct")]
  [AllowAnonymous]
  public async Task<IActionResult> GetDistinctTransmissions(CancellationToken cancellationToken)
  {
    var result = await Mediator.Send(new GetDistinctTransmissionsQuery(), cancellationToken);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  [HttpGet("color-distinct")]
  [AllowAnonymous]
  public async Task<IActionResult> GetDistinctColors(CancellationToken cancellationToken)
  {
    var result = await Mediator.Send(new GetDistinctColorsQuery(), cancellationToken);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  [HttpGet("year-distinct")]
  [AllowAnonymous]
  public async Task<IActionResult> GetDistinctYears(CancellationToken cancellationToken)
  {
    var result = await Mediator.Send(new GetDistinctYearsQuery(), cancellationToken);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  [HttpGet("type-distinct")]
  [AllowAnonymous]
  public async Task<IActionResult> GetDistinctVehicleTypes(CancellationToken cancellationToken)
  {
    var result = await Mediator.Send(new GetDistinctVehicleTypesQuery(), cancellationToken);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  [HttpGet("seat-count-distinct")]
  [AllowAnonymous]
  public async Task<IActionResult> GetDistinctSeatCounts(CancellationToken cancellationToken)
  {
    var result = await Mediator.Send(new GetDistinctSeatCountsQuery(), cancellationToken);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  [HttpGet("door-count-distinct")]
  [AllowAnonymous]
  public async Task<IActionResult> GetDistinctDoorCounts(CancellationToken cancellationToken)
  {
    var result = await Mediator.Send(new GetDistinctDoorCountsQuery(), cancellationToken);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }
}
