using Application.Branches;
using Application.Features.Branches.Commands;
using Application.Features.Branches.Dto;
using Application.Features.Branches.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiExplorerSettings(GroupName = "v1-Branches")]
[Authorize]
public sealed class BranchesController : BaseApiController
{
  // --- OKUMA (QUERY) ENDPOINT'LERİ ---

  // Tüm Şubeleri Listeleme (GET: api/branches)
  [HttpGet]
  [ProducesResponseType(typeof(List<BranchDto>), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetAll(CancellationToken token)
  {
    var response = await Mediator.Send(new GetBranchesQuery(), token);
    return Ok(response); // Angular'a 200 OK ve listeyi döner
  }

  // Tek Bir Şubenin Detayını Getirme (GET: api/branches/{id})
  [HttpGet("{id:guid}")]
  [ProducesResponseType(typeof(BranchDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken token)
  {
    var response = await Mediator.Send(new GetBranchQuery(id), token);
    return Ok(response);
  }

  // --- YAZMA (COMMAND) ENDPOINT'LERİ ---

  // Yeni Şube Ekleme (POST: api/branches)
  [HttpPost]
  [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
  public async Task<IActionResult> Create([FromBody] CreateBranchCommand command, CancellationToken token)
  {
    var branchId = await Mediator.Send(command, token);
    // REST standartlarına uygun olarak 21 Created ve kaynağın adresini dönüyoruz
    return CreatedAtAction(nameof(GetById), new { id = branchId }, branchId);
  }

  // Şube Bilgilerini Güncelleme (PUT: api/branches)
  [HttpPut("{id}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> Update([FromBody] UpdateBranchCommand command, CancellationToken token)
  {
    await Mediator.Send(command, token);
    return NoContent(); // 204 NoContent: İşlem başarılı, dönecek gövde yok (Unit)
  }

  // Şubeyi Arşivleme / Silme (DELETE: api/branches/{id})
  [HttpDelete("{id:guid}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken token)
  {
    await Mediator.Send(new DeleteBranchCommand(id), token);
    return NoContent(); // 204 NoContent: Senin SoftDelete metodun başarıyla çalıştı
  }
}
