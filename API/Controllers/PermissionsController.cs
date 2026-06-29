using Application.Features.Permissions.Commands;
using Application.Features.Permissions.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiExplorerSettings(GroupName = "v1-Permissions")]
[Authorize]
public sealed class PermissionsController: BaseApiController
{
  [HttpGet("get-all-permissions")]
  public async Task<IActionResult> GetAllPermissions(CancellationToken _token)
  {
    var response = await Mediator.Send(new GetAllPermissionsQuery(), _token);
    return Ok(response);
  }

  // 🔥 İşte aradığımız o nokta atışı endpoint kanks!
  [HttpGet("get-by-role/{roleId:guid}")]
  public async Task<IActionResult> GetByRole([FromRoute] Guid roleId, CancellationToken _token)
  {
    if (roleId == Guid.Empty) return BadRequest("Geçerli bir Rol ID giriniz.");

    var response = await Mediator.Send(new GetPermissionsByRoleIdQuery(roleId), _token);

    return response.IsSuccessful ? Ok(response) : BadRequest(response);
  }

  [HttpPut("deactivate")]
  public async Task<IActionResult> Deactivate([FromBody] DeactivatePermissionCommand command, CancellationToken cancellationToken)
  {
    if (command.RoleId == Guid.Empty || command.PermissionId == Guid.Empty)
      return BadRequest("Geçerli ID'ler giriniz.");

    var response = await Mediator.Send(command, cancellationToken);

    if (!response.IsSuccess)
      return BadRequest(response);

    return Ok(response);
  }

  [HttpPut("activate")]
  public async Task<IActionResult> Activate([FromBody] ActivatePermissionCommand command, CancellationToken cancellationToken)
  {
    if (command.RoleId == Guid.Empty || command.PermissionId == Guid.Empty)
      return BadRequest("Geçerli ID'ler giriniz.");

    var response = await Mediator.Send(command, cancellationToken);

    if (!response.IsSuccess)
      return BadRequest(response);

    return Ok(response);
  }
}
