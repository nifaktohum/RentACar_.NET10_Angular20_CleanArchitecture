using Application.Features.VehicleImages.Commands;
using Application.Features.VehicleImages.Dto;
using Application.Features.VehicleImages.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Vehicles;

[ApiExplorerSettings(GroupName = "v1-VehicleImages")]
[Authorize] // Admin yetkisi zorunlu!
public sealed class VehicleImagesController(
                    // IVehicleRepository _vehicleRepo,
                    // IVehicleImageRepository _vehicleImageRepo,
                    // IUnitOfWork _unit,
                    // IWebHostEnvironment _env
                    ) : BaseApiController
{

  // API/Controllers/V1/VehicleImagesController.cs
  [HttpPost("upload")]
  public async Task<IActionResult> Upload([FromForm] Guid vehicleId,
                                          [FromForm] List<IFormFile> files,
                                          [FromForm] bool isMain = false,
                                          CancellationToken cancellationToken = default)
  {
    var fileDtos = files.Select(f => new FileUploadDto(
        f.FileName,
        f.OpenReadStream(),
        f.ContentType,
        f.Length
    )).ToList();

    var command = new AddVehicleImageCommand(vehicleId, fileDtos, isMain);
    var result = await Mediator.Send(command, cancellationToken);

    return result.IsSuccessful ? Ok(result) : BadRequest(result.ErrorMessages);
  }

  [HttpGet("get-vehicle-images/{vehicleId}")]
  public async Task<IActionResult> GetVehicleImages(Guid vehicleId, CancellationToken token)
  {
    var query = new GetVehicleImagesQuery(vehicleId);
    var result = await Mediator.Send(query, token);

    return result.IsSuccessful ? Ok(result) : BadRequest(result.ErrorMessages);
  }

  [HttpDelete("delete/{id:guid}")]
  public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
  {
    var command = new DeleteVehicleImageCommand(id);
    var result = await Mediator.Send(command, cancellationToken);

    return result.IsSuccessful ? Ok(result) : BadRequest(result.ErrorMessages);
  }

  /// Araca ait bir resmi ana resim olarak ayarlar
  [HttpPatch("set-main")]
  public async Task<IActionResult> SetMainImage(SetMainVehicleImageCommand command, CancellationToken token = default)
  {
    var result = await Mediator.Send(command, token);

    return result.IsSuccessful ? Ok(result) : BadRequest(result.ErrorMessages);
  }

} 