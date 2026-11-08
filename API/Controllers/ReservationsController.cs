using Application.Features.Reservations.Commands;
using Application.Features.Reservations.Queries;
using Application.Specifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TS.Result;

namespace API.Controllers;

[ApiExplorerSettings(GroupName = "v1-Reservations")]
[Authorize]
public sealed class ReservationsController : BaseApiController
{
  // ==================== COMMANDS ====================

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
    var updatedCommand = command with { ReservationId = id };
    var result = await Mediator.Send(updatedCommand, token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  // POST: Kalan tutarı (tam ödeme) öde (Confirmed -> FullPaymentPaid)
  [HttpPost("pay-full-payment/{id:guid}")]
  public async Task<IActionResult> PayFullPayment(Guid id, [FromBody] PayFullPaymentCommand command, CancellationToken token)
  {
    var updatedCommand = command with { ReservationId = id };
    var result = await Mediator.Send(updatedCommand, token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  // POST: Aracı teslim et (Confirmed -> Active)
  [HttpPost("activate/{id:guid}")]
  public async Task<IActionResult> Activate(Guid id, [FromBody] ActivateReservationCommand command, CancellationToken token)
  {
    if (id != command.ReservationId)
      return BadRequest(Result<bool>.Failure("URL'deki Rezervasyon ID'si ile gövdedeki ID eşleşmiyor!"));

    var result = await Mediator.Send(command, token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  // POST: Aracı iade et (Active -> Completed)
  [HttpPost("complete/{id:guid}")]
  public async Task<IActionResult> Complete(Guid id, [FromBody] CompleteReservationCommand command, CancellationToken token)
  {
    if (id != command.ReservationId)
      return BadRequest(Result<bool>.Failure("URL'deki Rezervasyon ID'si ile gövdedeki ID eşleşmiyor!"));

    var result = await Mediator.Send(command, token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  // POST: Rezervasyonu iptal et
  [HttpPost("cancel/{id:guid}")]
  public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelReservationCommand command, CancellationToken token)
  {
    if (id != command.ReservationId)
      return BadRequest(Result<bool>.Failure("URL'deki Rezervasyon ID'si ile gövdedeki ID eşleşmiyor!"));

    var result = await Mediator.Send(command, token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  // PUT: Rezervasyon durumunu güncelle (All-in-one)
  [HttpPut("update-status")]
  public async Task<IActionResult> UpdateStatus([FromBody] UpdateReservationStatusCommand command, CancellationToken token)
  {
    var result = await Mediator.Send(command, token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  // ==================== QUERIES ====================

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
  public async Task<IActionResult> GetList([FromQuery] GetReservationListQuery query, CancellationToken token)
  {
    var result = await Mediator.Send(query, token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }
}

/* Sistemin Doğrulanan Tüm İş Kuralları

      ✅ 15 dk checkout hold (PendingPayment → Expired otomatik)
      ✅ 24 saat depozito kilidi (DepositLockDeadline)
      ✅ Tam ödeme direkt yapılabiliyor (depozito atlanabiliyor)
      ✅ Tarih bazlı kilit (ConflictingReservationsSpecification)
      ✅ Fiziksel mühürleme (Activate'te Rented, Complete'te Available)
      ✅ Stok mantığı (AvailableStock computed)
      ✅ KM takibi (Complete'te güncelleniyor)
      ✅ Erken iade (PlannedEndDate fiili iadeye çekiliyor)
      ✅ Kademeli iade politikası:
              >48 saat → %100 iade
              24-48 saat → %50 iade
              <24 saat → %0 iade (yandı)
*/