using Application.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
// [ApiExplorerSettings(GroupName = "v1-rest")] // Sadece REST dokümanında görünür
public abstract class BaseApiController : ControllerBase
{
  private ISender? _mediator;

  // Miras alan sınıflar constructor içinde mediator geçmek zorunda kalmasın diye
  // HttpContext üzerinden (IoC Container) ihtiyaç duyulduğu an (Lazy) çekiyoruz.
  protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();
}

/*
  // Standart araç operasyonları kontrolcün
  [Tags("🚗 Araç Yönetimi (REST)")]
  public class CarsController : BaseApiController { ... }

  // Dinamik araç raporları kontrolcün
  [Tags("📊 Dinamik Raporlama (OData)")]
  public class CarReportsController : BaseODataController { ... }

*/