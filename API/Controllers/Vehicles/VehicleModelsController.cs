using Application.Features.VehicleModels.Commands;
using Application.Features.VehicleModels.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Vehicles;

[ApiExplorerSettings(GroupName = "v1-VehicleModels")]
[Authorize] // Admin yetkisi zorunlu!
public class VehicleModelsController : BaseApiController
{
  [HttpGet("get-all")]
  public async Task<IActionResult> GetList([FromQuery] GetAllVehicleModelsQuery query)
  {
    var result = await Mediator.Send(query);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  [HttpGet("get-by-id/{id}")]
  public async Task<IActionResult> GetById(Guid id, CancellationToken token)
  {
    var command = new GetVehicleModelByIdQuery(id);
    var result = await Mediator.Send(command, token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }


  [HttpPost("create")]
  public async Task<IActionResult> Create([FromBody] CreateVehicleModelCommand command)
  {
    var result = await Mediator.Send(command);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  [HttpPut("update")]
  public async Task<IActionResult> Update([FromBody] UpdateVehicleModelCommand command)
  {
    var result = await Mediator.Send(command);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  [HttpDelete("delete/{id:guid}")]
  public async Task<IActionResult> Delete(Guid id)
  {
    var command = new DeleteVehicleModelCommand(id);
    var result = await Mediator.Send(command);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }
}
