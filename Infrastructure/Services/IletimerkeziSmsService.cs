using System.Net.Http.Json;
using System.Text.Json;
using Application.Services;
using Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public class IletimerkeziSmsService(
    HttpClient _httpClient,
    IOptions<IletimerkeziSettings> _settings,
    ILogger<IletimerkeziSmsService> _logger
) : ISmsService
{
  public async Task<bool> SendSmsAsync(string toNumber, string message, CancellationToken cancellationToken = default)
  {
    try
    {
      var formattedNumber = FormatPhoneNumber(toNumber);

      // Numarayı kontrol et (İletimerkezi 10 haneli '5XXXXXXXXX' bekler)
      if (string.IsNullOrWhiteSpace(formattedNumber) || formattedNumber.Length != 10)
      {
        _logger.LogError("Geçersiz telefon numarası formatı. Ham: {ToNumber}, İşlenen: {FormattedNumber}", toNumber, formattedNumber);
        return false;
      }

      var payload = new
      {
        request = new
        {
          authentication = new
          {
            key = _settings.Value.ApiKey,
            hash = _settings.Value.ApiHash
          },
          order = new
          {
            sender = _settings.Value.Sender,
            iys = "0",  // ✅ Zorunlu alan. İşlemsel mesajlar için "0", ticari mesajlar için "1" kullanın
            // iysList = "BIREYSEL",  // Yalnızca iys="1" olduğunda gereklidir
            message = new
            {
              text = message,
              receipents = new  // ⚠️ Dikkat: yazım hatası "receipents", "recipients" değil!
              {
                number = new[] { formattedNumber }
              }
            }
          }
        }
      };
      var response = await _httpClient.PostAsJsonAsync(
          "https://api.iletimerkezi.com/v1/send-sms/json",
          payload,
          cancellationToken
      );

      var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

      // Yanıtı parse edip İletimerkezi iç durum kodunu kontrol ediyoruz
      using var doc = JsonDocument.Parse(responseContent);

      if (doc.RootElement.TryGetProperty("response", out var responseElem) &&
          responseElem.TryGetProperty("status", out var statusElem))
      {
        int apiCode = statusElem.GetProperty("code").GetInt32();
        string apiMessage = statusElem.GetProperty("message").GetString() ?? string.Empty;

        if (apiCode == 200)
        {
          _logger.LogInformation("İletimerkezi SMS başarıyla gönderildi. Alıcı: {ToNumber}", formattedNumber);
          return true;
        }

        _logger.LogError("İletimerkezi SMS gönderimi başarısız. API Code: {Code}, Message: {Message}", apiCode, apiMessage);
        return false;
      }

      _logger.LogError("İletimerkezi beklenmeyen yanıt formatı: {Response}", responseContent);
      return false;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "İletimerkezi SMS gönderimi sırasında beklenmeyen hata oluştu. Alıcı: {ToNumber}", toNumber);
      return false;
    }
  }

  public string FormatPhoneNumber(string rawPhone)
  {
    if (string.IsNullOrWhiteSpace(rawPhone)) return string.Empty;

    // Sadece rakamları ayıkla (+, -, boşluk, parantez v.b. temizle)
    string clean = new string(rawPhone.Where(char.IsDigit).ToArray());

    // Baştaki '0090', '90' veya '0' ifadelerini temizle
    if (clean.StartsWith("0090") && clean.Length == 14) clean = clean.Substring(4);
    else if (clean.StartsWith("90") && clean.Length == 12) clean = clean.Substring(2);
    else if (clean.StartsWith("0") && clean.Length == 11) clean = clean.Substring(1);

    // Türkiye GSM Numarası Kontrolü: Exactly 10 hane ve 5 ile başlamalı (Örn: 5321234567)
    if (clean.Length == 10 && clean.StartsWith("5"))
    {
      return clean;
    }

    return string.Empty; // Geçersiz GSM numarası
  }
}