namespace Application.Services;

public interface ICustomerContext
{
  /// Giriş yapmış müşterinin ID'si. Yoksa Guid.Empty.
  Guid GetCustomerId();

  /// Giriş yapmış müşterinin email'i. Yoksa null.
  string? GetEmail();

  /// "Individual" / "Corporate". Yoksa null.
  string? GetCustomerType();
}

/*
  Neden IUserContext'e eklemedik? Çünkü müşteri ve personel aynı anda giriş yapmış olabilir (nadir ama mümkün). 
    Ayrıca karışıklığı önler. Token'daki claim'lerden hangisi geldiğine göre ilgili context dolar.
*/
