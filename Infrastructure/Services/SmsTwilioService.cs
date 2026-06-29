using Application.Services;
using Microsoft.Extensions.Configuration;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Infrastructure.Services;

public class SmsTwilioService(IConfiguration _config) : ISmsService
{
  public async Task<bool> SendSmsAsync(string toNumber, string message, CancellationToken cancellationToken = default)
  {
    // // appsettings.json dosyasından ayarları güvenle çekiyoruz 
    string _accountSid = _config["Twilio:AccountSid"] ?? throw new ArgumentNullException("Twilio AccountSid eksik!");
    string _authToken = _config["Twilio:AuthToken"] ?? throw new ArgumentNullException("Twilio AuthToken eksik!");
    string _fromNumber = _config["Twilio:FromPhoneNumber"] ?? throw new ArgumentNullException("Twilio FromPhoneNumber eksik!");

    // // Twilio istemcisini bu bilgilerle initialize ediyoruz
    TwilioClient.Init(_accountSid, _authToken);

    try
    {
      // // Gerçek SMS fırlatma anı !
      var messageResource = await MessageResource.CreateAsync(
          body: message,
          from: new PhoneNumber(_fromNumber),
          to: new PhoneNumber(toNumber)
      );

      // // Eğer mesajın statüsü failed değilse her şey yolunda demektir 
      return messageResource.Status != MessageResource.StatusEnum.Failed;
    }
    catch (Exception ex)
    {
      // // İleride buraya logger ekleyip hatayı basabilirsin 
      Console.WriteLine($"Twilio SMS Hatası: {ex.Message}");
      return false;
    }
  }
}
