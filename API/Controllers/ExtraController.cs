using Application.Features.Extras.Commands;
using Application.Features.Extras.Queries;
using Domain.Entities.Extras.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiExplorerSettings(GroupName = "v1-Extras")]
[Authorize]
public sealed class ExtraController : BaseApiController
{
  [HttpGet("extra-all")]
  public async Task<IActionResult> GetAll([FromQuery] GetAllExtrasQuery command, CancellationToken cancellationToken)
  {
    var result = await Mediator.Send(command, cancellationToken);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  [HttpGet("extra-by-id/{id:guid}")]
  public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
  {
    var command = new GetExtraByIdQuery(id);
    var result = await Mediator.Send(command, cancellationToken);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  [HttpPost("extra-create")]
  public async Task<IActionResult> Create([FromBody] CreateExtraCommand command, CancellationToken cancellationToken)
  {
    var result = await Mediator.Send(command, cancellationToken);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  [HttpPut("extra-update")]
  public async Task<IActionResult> Update([FromBody] UpdateExtraCommand command, CancellationToken cancellationToken)
  {
    var result = await Mediator.Send(command, cancellationToken);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }
  
  [HttpDelete("extra-delete/{id:guid}")]
  public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
  {
    var command = new DeleteExtraCommand(id);
    var result = await Mediator.Send(command, cancellationToken);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }
  [HttpPatch("toggle-status/{id:guid}")]
  public async Task<IActionResult> ToggleStatus(Guid id, CancellationToken cancellationToken)
  {
    var command = new ToggleExtraStatusCommand(id);
    var result = await Mediator.Send(command, cancellationToken);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  [HttpGet("extra-by-category/{category}")]
  public async Task<IActionResult> GetByCategory(int category, CancellationToken cancellationToken)
  {
    var command = new GetExtrasByCategoryQuery(category);
    var result = await Mediator.Send(command, cancellationToken);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  [HttpGet("extra-by-priceType/{priceType}")]
  public async Task<IActionResult> GetByPriceType( int priceType, CancellationToken cancellationToken)
  {
    var command = new GetExtrasByPriceTypeQuery(priceType);
    var result = await Mediator.Send(command, cancellationToken);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  [HttpGet("extra-in-stock")]
  public async Task<IActionResult> GetInStock([FromQuery] GetInStockExtrasQuery command, CancellationToken cancellationToken)
  {
    var result = await Mediator.Send(command, cancellationToken);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  [HttpGet("extra-recommended")]
  public async Task<IActionResult> GetRecommended([FromQuery] GetRecommendedExtrasQuery command, CancellationToken cancellationToken)
  {
    var result = await Mediator.Send(command, cancellationToken);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }




}

/* API Endpoints Özeti

Metot	  Endpoint	A                       çıklama	Yetki

GET	    /api/extra	                      Tüm ürünleri listele	Kullanıcı
GET	    /api/extra/{id}	                  ID'ye göre ürün getir	Kullanıcı
GET	    /api/extra/category/{category}	  Kategoriye göre listele	Kullanıcı
GET	    /api/extra/pricetype/{priceType}	Fiyat tipine göre listele	Kullanıcı
GET	    /api/extra/recommended	          Önerilenleri listele	Kullanıcı
GET	    /api/extra/in-stock	              Stoktakileri listele	Kullanıcı
POST	  /api/extra	                      Yeni ürün oluştur	Admin/Manager
PUT	    /api/extra/{id}	                  Ürün güncelle	Admin/Manager
DELETE	/api/extra/{id}	                  Ürün sil (soft)	Admin
PATCH	  /api/extra/{id}/toggle-status	    Aktif/Pasif değiştir	Admin/Manager

*/
