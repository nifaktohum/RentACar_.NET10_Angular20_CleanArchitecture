using Application.Features.ProtectionPackages._BenefitCategories.Commands;
using Application.Features.ProtectionPackages._BenefitCategories.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiExplorerSettings(GroupName = "v1-BenefitCategories")]
[Authorize]
public class BenefitCategoriesController: BaseApiController
{
  [HttpGet("benefit-category-get-all")]
  public async Task<IActionResult> GetAll(CancellationToken _token)
  {
    var result = await Mediator.Send(new GetAllBenefitCategoriesQuery(), _token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  [HttpGet("benefit-category-get-id/{id}")]
  public async Task<IActionResult> GetById(Guid id, CancellationToken _token)
  {
    var result = await Mediator.Send(new GetBenefitCategoryByIdQuery(id), _token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  [HttpPost("benefit-category-create")]
  public async Task<IActionResult> Create([FromBody] CreateBenefitCategoryCommand _command, CancellationToken _token)
  {
    var result = await Mediator.Send(_command, _token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  [HttpPut("benefit-category-update")]
  public async Task<IActionResult> Update([FromBody] UpdateBenefitCategoryCommand _command, CancellationToken _token)
  {
    var result = await Mediator.Send(_command, _token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  [HttpDelete("benefit-category-delete/{id}")]
  public async Task<IActionResult> Delete(Guid id, CancellationToken _token)
  {
    var result = await Mediator.Send(new DeleteBenefitCategoryCommand(id), _token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }
}
