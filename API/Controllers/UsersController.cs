
using Application.Features.Users.Commands;
using Application.Features.Users.Query;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiExplorerSettings(GroupName = "v1-Users")]
[Authorize] // Admin yetkisi zorunlu!
public sealed class UsersController : BaseApiController
{
  // === COMMANDS ===

  [HttpPost("create")]
  public async Task<IActionResult> Create([FromBody] CreateUserCommand command, CancellationToken token)
  {
    var result = await Mediator.Send(command, token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  [HttpPut("update")]
  public async Task<IActionResult> Update([FromBody] UpdateUserCommand command, CancellationToken token)
  {
    var result = await Mediator.Send(command, token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  [HttpDelete("delete")]
  public async Task<IActionResult> Delete([FromBody] DeleteUserCommand command, CancellationToken token)
  {
    var result = await Mediator.Send(command, token);
    return result.IsSuccessful ? Ok(new { message = "Kullanıcı başarıyla silindi" })
                               : BadRequest(result);
  }

  // === QUERIES ===

  [HttpGet("get-all")]
  public async Task<IActionResult> GetAll(CancellationToken token)
  {
    var result = await Mediator.Send(new GetUserListQuery(), token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  [HttpGet("get-by-id/{id}")]
  public async Task<IActionResult> GetById(Guid id, CancellationToken token)
  {
    var query = new GetUserByIdQuery(id);
    var result = await Mediator.Send(query, token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

}