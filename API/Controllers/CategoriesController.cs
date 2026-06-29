using Application.Features.Categories.Commands;
using Application.Features.Categories.Queries;
using Application.Features.Users.Query;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiExplorerSettings(GroupName = "v1-Categories")]
[Authorize]
public sealed class CategoriesController(IMediator _mediator): BaseApiController
{
  [HttpPost("create")]
  public async Task<IActionResult> Create([FromBody] CreateCategoryCommand command, CancellationToken token)
  {
    var result = await _mediator.Send(command, token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  [HttpPut("update")]
  public async Task<IActionResult> Update([FromBody] UpdateCategoryCommand command, CancellationToken token)
  {
    var result = await _mediator.Send(command, token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  [HttpDelete("delete")]
  public async Task<IActionResult> Delete(DeleteCategoryCommand command, CancellationToken token)
  {
    var result = await _mediator.Send(command, token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  [HttpGet("get-all")]
  public async Task<IActionResult> GetAll(CancellationToken token)
  {
    var result = await _mediator.Send(new GetAllCategoriesQuery(), token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  [HttpGet("get-by-id/{id}")]
  public async Task<IActionResult> GetById(Guid id, CancellationToken token)
  {
    var category = new GetCategoryByIdQuery(id);
    var result = await _mediator.Send(category, token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }
}
