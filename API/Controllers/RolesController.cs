using Application.Features.Roles.Commands;
using Application.Features.Roles.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

// 🔥 İşte Swagger'da Rol yönetimini ayrı bir sekme olarak gösterecek o sihirli dokunuş:
[ApiExplorerSettings(GroupName = "v1-Roles")]
[Authorize]
public sealed class RolesController : BaseApiController
{
  // === COMMANDS ===

  [HttpPost("create")]
  public async Task<IActionResult> Create(CreateRoleCommand command, CancellationToken _token)
  {
    var result = await Mediator.Send(command, _token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  [HttpPut("update")]
  public async Task<IActionResult> Update([FromBody] UpdateRoleCommand command, CancellationToken _token)
  {
    var result = await Mediator.Send(command, _token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  [HttpDelete("delete")]
  public async Task<IActionResult> Delete(DeleteRoleCommand command, CancellationToken _token)
  {
    var result = await Mediator.Send(command, _token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  // === QUERIES ===

  [HttpGet("get-all")]
  public async Task<IActionResult> GetAll(CancellationToken _token)
  {
    // Dışarıdan nesne beklemiyoruz, direkt içeride yaratıp tetikliyoruz kanka
    var result = await Mediator.Send(new GetAllRolesQuery(), _token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  [HttpGet("get-by-id/{id:guid}")]
  public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken _token)
  {
    // URL'den gelen id'yi elinle yeni query nesnesine bağlıyorsun kanks
    var result = await Mediator.Send(new GetRoleQuery(id), _token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }


}