using Application.Features.ProtectionPackages.Commands;
using Application.Features.ProtectionPackages._ProtectionBenefits.BenefitCommands;
using Application.Features.ProtectionPackages._ProtectionBenefits.BenefitQueries;
using Application.Features.ProtectionPackages.Queries;
using Microsoft.AspNetCore.Mvc;
using Application.Features.ProtectionPackages._ProtectionBenefits.Queries;

namespace API.Controllers;

[ApiExplorerSettings(GroupName = "v1-ProtectionPackages")]
// [Authorize]"
public class ProtectionPackagesController: BaseApiController
{

  // ========= PROTECTİON PACKAGES ======== //
  [HttpPost("create-packages")]
  public async Task<IActionResult> CreatePackeges([FromBody] CreateProtectionPackageCommand _command, CancellationToken _token)
  {
    var result = await Mediator.Send(_command, _token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  [HttpPut("update-packages")]
  public async Task<IActionResult> UpdatePackages([FromBody] UpdateProtectionPackageCommand _command, CancellationToken _token)
  {
    var result = await Mediator.Send(_command, _token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  [HttpGet("get-by-id-packages")]
  public async Task<IActionResult> GetByIdPackages([FromQuery] Guid id, CancellationToken _token)
  {
    var command = new GetProtectionPackageByIdQuery(id);
    var result = await Mediator.Send(command, _token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }
  [HttpGet("get-all-packages")]
  public async Task<IActionResult> GetAllPackages([FromQuery] bool? OnlyActive, [FromQuery] bool? OnlyRecommended, CancellationToken _token)
  {
    var command = new GetProtectionPackagesQuery(OnlyActive, OnlyRecommended);
    var result = await Mediator.Send(command, _token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  [HttpDelete("delete-packages/{id}")]
  public async Task<IActionResult> DeletePackages(Guid id, CancellationToken _token)
  {
    var command = new DeleteProtectionPackageCommand(id);
    var result = await Mediator.Send(command, _token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  [HttpGet("get-recommended")]
  public async Task<IActionResult> GetRecommended(CancellationToken _token)
  {
    var result = await Mediator.Send(new GetRecommendedPackagesQuery(), _token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }



  // ========= BENEFİTS ======== //
  [HttpGet("get-by-id-benefits")]
  public async Task<IActionResult> GetByIdBenefit([FromQuery] Guid id, CancellationToken _token)
  {
    var command = new GetProtectionBenefitsByIdQuery(id);
    var result = await Mediator.Send(command, _token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  [HttpGet("get-all-benefits")]
  public async Task<IActionResult> GetAllBenefit(CancellationToken _token)
  {
    var result = await Mediator.Send(new GetAllProtectionBenefitsQuery(), _token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  [HttpPost("create-benefits")]
  public async Task<IActionResult> CreateBenefit(CreateProtectionBenefitCommand _command, CancellationToken _token)
  {
    var result = await Mediator.Send(_command, _token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  [HttpPut("update-benefits")]
  public async Task<IActionResult> UpdateBenefit([FromBody] UpdateProtectionBenefitCommand _command, CancellationToken _token)
  {
    var result = await Mediator.Send(_command, _token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  [HttpDelete("delete-benefits")]
  public async Task<IActionResult> DeleteBenefit([FromBody] DeleteProtectionBenefitCommand _command, CancellationToken _token)
  {
    var result = await Mediator.Send(_command, _token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }


}
