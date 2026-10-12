using Application.Services;
using Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;


namespace Infrastructure.Services;

public class SmsTwilioService(
                IOptions<TwilioSettings> _options,
                ILogger<SmsTwilioService> _logger
            ) : ISmsService

{
  public async Task<bool> SendSmsAsync(string toNumber, string message, CancellationToken cancellationToken = default)
  {
    
    // // appsettings.json dosyasından ayarları güvenle çekiyoruz 

    // ==> Opstions/TwilioSettings options kullandık gerek kalmadı..
    // string _accountSid = _config["Twilio:AccountSid"] ?? throw new ArgumentNullException("Twilio AccountSid eksik!");
    // string _authToken = _config["Twilio:AuthToken"] ?? throw new ArgumentNullException("Twilio AuthToken eksik!");
    // string _fromNumber = _config["Twilio:FromPhoneNumber"] ?? throw new ArgumentNullException("Twilio FromPhoneNumber eksik!");

    // // Twilio istemcisini bu bilgilerle initialize ediyoruz
    TwilioClient.Init(_options.Value.AccountSid, _options.Value.AuthToken);

    try
    {
      // // Gerçek SMS fırlatma anı !
      var messageResource = await MessageResource.CreateAsync(
          body: message,
          from: new PhoneNumber(_options.Value.FromPhoneNumber),
          to: new PhoneNumber(toNumber)
      );


      bool isSuccess = messageResource.Status != MessageResource.StatusEnum.Failed
                        && messageResource.Status != MessageResource.StatusEnum.Undelivered;

      if (!isSuccess)
      {
        _logger.LogWarning($"SMS gönderimi tamamlanamadı. Alıcı: {toNumber}, Status: {messageResource.Status}, Hata Kodu: {messageResource.ErrorCode}");
      }

      return isSuccess;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, $"Twilio SMS gönderimi sırasında bir hata oluştu. Alıcı: {toNumber}");
      return false;
    }
  }
}
