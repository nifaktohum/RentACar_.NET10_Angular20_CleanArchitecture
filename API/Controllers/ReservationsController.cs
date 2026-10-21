using Application.Features.Reservations.Commands;
using Application.Features.Reservations.Queries;
using Application.Specifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TS.Result;

namespace API.Controllers;

[ApiExplorerSettings(GroupName = "v1-Reservations")]
[Authorize] // Rezervasyon işlemleri için giriş zorunlu
public sealed class ReservationsController : BaseApiController
{
  // POST: Yeni rezervasyon oluştur
  [HttpPost("create")]
  public async Task<IActionResult> Create([FromBody] CreateReservationCommand command, CancellationToken token)
  {
    var result = await Mediator.Send(command, token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  // POST: Kapora ödemesi yap (PendingPayment -> Confirmed)
  [HttpPost("pay-deposit/{id:guid}")]
  public async Task<IActionResult> PayDeposit(Guid id, [FromBody] PayDepositCommand command, CancellationToken token)
  {
    // URL'den gelen id ile Command'daki ReservationId'yi eşitleyelim
    var updatedCommand = command with { ReservationId = id };

    var result = await Mediator.Send(updatedCommand, token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  // POST: Aracı teslim et (Active)
  [HttpPost("activate/{id:guid}")]
  public async Task<IActionResult> Activate(Guid id, [FromBody] ActivateReservationCommand command, CancellationToken token)
  {
    if (id != command.ReservationId) return BadRequest(Result<bool>.Failure("Rezervasyon ID'leri eşleşmiyor!"));

    var result = await Mediator.Send(command, token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  // POST: Aracı iade et (Active -> Completed)
  [HttpPost("complete/{id:guid}")]
  public async Task<IActionResult> Complete(Guid id, [FromBody] CompleteReservationCommand command, CancellationToken token)
  {
    var updatedCommand = command with { ReservationId = id };
    var result = await Mediator.Send(updatedCommand, token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  // POST: Rezervasyonu iptal et
  [HttpPost("cancel/{id:guid}")]
  public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelReservationCommand command, CancellationToken token)
  {
    var updatedCommand = command with { ReservationId = id };
    var result = await Mediator.Send(updatedCommand, token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  // GET: Rezervasyon detayı
  [HttpGet("get-by-id/{id:guid}")]
  public async Task<IActionResult> GetById(Guid id, CancellationToken token)
  {
    var query = new GetReservationByIdQuery(id);
    var result = await Mediator.Send(query, token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }
  // GET: Rezervasyon listesi (filtreleme + sayfalama)
  [HttpGet("get-all")]
  public async Task<IActionResult> GetList([FromQuery] GetReservationListQuery query  , CancellationToken token)
  {
    var result = await Mediator.Send(query, token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }
}
