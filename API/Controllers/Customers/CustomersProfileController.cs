using Application.Features.Customers.Profile.Commands;
using Application.Features.Customers.Profile.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Customers;

[ApiExplorerSettings(GroupName = "v1-Customers-Profile")]
[Authorize]
public sealed class CustomersProfileController : BaseApiController
{
  // ==================== 👤 GET PROFILE ====================

  [HttpGet("get-profile")]
  public async Task<IActionResult> GetProfile([FromQuery] GetCustomerProfileQuery query, CancellationToken token)
  {
    var result = await Mediator.Send(query, token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  // ==================== ✏️ UPDATE PROFILE ====================

  [HttpPut("update-profile")]
  public async Task<IActionResult> UpdateProfile([FromBody] UpdateCustomerProfileCommand command, CancellationToken token)
  {
    var result = await Mediator.Send(command, token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }
}
