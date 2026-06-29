using Application.Branches;
using Application.Features.Branches.Commands;
using Application.Features.Branches.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Modules;

public static class BranchModule
{
  public static void MapBranch(IEndpointRouteBuilder _builder)
  {
    var app = _builder.MapGroup("/branches").RequireRateLimiting("FixedWindowPolicy");

    // Tüm Şubeleri Listeleme (GET: /branches)
    app.MapGet("/", async (ISender mediator, CancellationToken token) =>
    {
      var response = await mediator.Send(new GetBranchesQuery(), token);
      return Results.Ok(response); // Angular'a 200 OK ve şube listesini fırlatır.
    });

    // Tek Bir Şubenin Detayını Getirme (GET: /branches/{id})
    app.MapGet("/{id:guid}", async (Guid id, ISender mediator, CancellationToken token) =>
    {
      var response = await mediator.Send(new GetBranchQuery(id), token);
      return Results.Ok(response); // Şube bulunursa 200 OK ile detayları döner.
    });

    // Yeni Şube Ekleme (POST: /branches)
    app.MapPost("/", async ([FromBody] CreateBranchCommand command, ISender mediator, CancellationToken token) =>
    {
      var branchId = await mediator.Send(command, token);
      // REST standartlarına uygun olarak 201 Created ve üretilen ID'yi dönüyoruz
      return Results.Created($"/branches/{branchId}", branchId);
    });

    // Şube Bilgilerini Güncelleme (PUT: /branches)
    app.MapPut("/", async ([FromBody] UpdateBranchCommand command, ISender mediator, CancellationToken token) =>
    {
      await mediator.Send(command, token);
      return Results.NoContent(); // 204 NoContent: "İşlem başarılı, geriye dönecek veri yok" (Unit karşılığı)
    });

    // Şubeyi Arşivleme / Silme (DELETE: /branches/{id})
    app.MapDelete("/{id:guid}", async (Guid id, ISender mediator, CancellationToken token) =>
    {
      await mediator.Send(new DeleteBranchCommand(id), token);
      return Results.NoContent(); // 204 NoContent: Senin SoftDelete metodun çalıştı, iş bitti.
    });
  }
}
