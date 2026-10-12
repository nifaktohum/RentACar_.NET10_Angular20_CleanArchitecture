using System.Security.Claims;
using Application.Services;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services;

public sealed class CustomerContext(IHttpContextAccessor _http) : ICustomerContext
{
  private ClaimsPrincipal? User => _http.HttpContext?.User;

  /// İstekte bulunan kullanıcının Müşteri (Customer) olup olmadığını ve kimliğinin doğrulanıp doğrulanmadığını kontrol eder.
  private bool IsValidCustomer =>
        User?.Identity?.IsAuthenticated == true &&
        string.Equals(User.FindFirst("UserType")?.Value, "Customer", StringComparison.OrdinalIgnoreCase);

  public Guid GetCustomerId()
  {

    if (!IsValidCustomer || User is null) return Guid.Empty;

    // 1. Önce "Id" claim'ine, bulamazsa standart NameIdentifier ("http://schemas.../nameidentifier") veya "sub" claim'ine bak
    var customerIdClaim = User.FindFirst("Id")
                          ?? User.FindFirst(ClaimTypes.NameIdentifier)
                          ?? User.FindFirst("sub");

    if (customerIdClaim is null || string.IsNullOrWhiteSpace(customerIdClaim.Value)) return Guid.Empty;
    if (Guid.TryParse(customerIdClaim.Value, out Guid id)) return id;

    throw new ArgumentException($"'{customerIdClaim.Value}' geçerli bir Guid değil.", nameof(customerIdClaim));
  }

  public string? GetCustomerType()
  {
    if (!IsValidCustomer) return null;

    return User?.FindFirst("CustomerType")?.Value;
  }

  public string? GetEmail()
  {
    if (!IsValidCustomer) return null;

    // "Email" veya standart ClaimTypes.Email ("http://schemas.../emailaddress") üzerinden oku
    return User?.FindFirst("Email")?.Value
           ?? User?.FindFirst(ClaimTypes.Email)?.Value;
  }
}
