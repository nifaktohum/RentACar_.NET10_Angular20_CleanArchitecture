using Application.Features.Customers.Management.Commands;
using Application.Features.Customers.Management.Queries;
using Application.Specifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Customers;

[ApiExplorerSettings(GroupName = "v1-Customers-Management")]
[Authorize] // Token gerekli
public sealed class CustomersManagementController : BaseApiController
{
  // ==================== 📋 GET LIST ====================

  [HttpGet("get-all")]
  public async Task<IActionResult> GetAll([FromQuery] CustomerSpecParams specParams, CancellationToken token)
  {
    var query = new GetCustomerListQuery(specParams);
    var result = await Mediator.Send(query, token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  // ==================== 🔍 GET BY ID ====================

  [HttpGet("get-by-id/{id:guid}")]
  public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken token)
  {
    var query = new GetCustomerByIdQuery(id);
    var result = await Mediator.Send(query, token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  // ==================== 🚫 BLACKLIST ====================

  [HttpPost("blacklist-added")]
  public async Task<IActionResult> Blacklist([FromBody] BlacklistCustomerCommand command, CancellationToken token)
  {
    var result = await Mediator.Send(command, token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  [HttpDelete("blacklist-removed/{id:guid}")]
  public async Task<IActionResult> RemoveFromBlacklist([FromRoute] Guid id, CancellationToken token)
  {
    var command = new RemoveFromBlacklistCommand(id);
    var result = await Mediator.Send(command, token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  // ==================== 📊 FINDEKS SCORE ====================

  [HttpPut("findeks-added")]
  public async Task<IActionResult> UpdateFindeks([FromBody] UpdateFindeksScoreCommand command, CancellationToken token)
  {
    var result = await Mediator.Send(command, token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }
}
