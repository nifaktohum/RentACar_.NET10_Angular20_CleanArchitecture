using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace API.Exceptions;
// merkezi hata yönetimi yapısı
public class ExceptionHandler : IExceptionHandler
{
  public async ValueTask<bool> TryHandleAsync(
      HttpContext httpContext,
      Exception exception,
      CancellationToken cancellationToken)
  {
    int statusCode = (int)HttpStatusCode.InternalServerError;
    string message = "Sistemsel bir hata oluştu. Lütfen daha sonra tekrar deneyiniz.";

    if (exception is FluentValidation.ValidationException validationException)
    {
      statusCode = (int)HttpStatusCode.BadRequest;
      message = string.Join(", ", validationException.Errors.Select(e => e.ErrorMessage));
    }

    // // 🔥 Değişiklik 1: ProblemDetails nesnesine Type ve Status'u tam verelim
    var problemDetails = new ProblemDetails
    {
      Status = statusCode,
      Title = statusCode == 400 ? "Doğrulama Hatası" : "Sunucu Hatası",
      Detail = message,
      Instance = httpContext.Request.Path,
      Type = statusCode == 400
                ? "https://tools.ietf.org/html/rfc7231#section-6.5.1"  // Eğer hata 400 ise bunu ver
                : "https://tools.ietf.org/html/rfc7231#section-6.6.1" // Hata 400 değilse (yani 500 ise) bunu ver
    };

    // // 🔥 Değişiklik 2: Response StatusCode'u en başta set ediyoruz kanka
    httpContext.Response.StatusCode = statusCode;
    httpContext.Response.ContentType = "application/problem+json"; // RFC standardı hata tipi

    // // 🔥 Değişiklik 3: WriteAsync yerine WriteAsJsonAsync kullanarak nesneyi doğrudan akışa yazıyoruz kanka
    await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

    return true;
  }
}
