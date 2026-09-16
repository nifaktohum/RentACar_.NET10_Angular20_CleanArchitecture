using Application.Features.VehicleTypes.Commands;
using Application.Features.VehicleTypes.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Vehicles;

[ApiExplorerSettings(GroupName = "v1-VehicleTypes")]
[Authorize] // Admin yetkisi zorunlu!
public class VehicleTypesController : BaseApiController
{
  [HttpGet("get-all")]
  public async Task<IActionResult> GetList([FromQuery] GetAllVehicleTypesQuery query, CancellationToken token)
  {
    var result = await Mediator.Send(query, token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  [HttpGet("get-by-id/{id:guid}")]
  public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken token)
  {
    var query = new GetVehicleTypeByIdQuery(id);
    var result = await Mediator.Send(query, token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  [HttpPost("create")]
  public async Task<IActionResult> Create([FromBody] CreateVehicleTypeCommand command, CancellationToken token)
  {
    var result = await Mediator.Send(command, token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  [HttpPut("update")]
  public async Task<IActionResult> Update([FromBody] UpdateVehicleTypeCommand command, CancellationToken token)
  {
    var result = await Mediator.Send(command, token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  [HttpDelete("delete/{id:guid}")]
  public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken token)
  {
    var command = new DeleteVehicleTypeCommand(id);
    var result = await Mediator.Send(command, token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  [HttpPatch("toggle-status/{id:guid}")]
  public async Task<IActionResult> ToggleStatus([FromRoute] Guid id, CancellationToken token)
  {
    var query = new ToggleVehicleTypeStatusCommand(id);
    var result = await Mediator.Send(query, token);
    return StatusCode(result.StatusCode, result);
  }
}
