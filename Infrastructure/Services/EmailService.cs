using System.Net;
using System.Net.Mail;
using Application.Services;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services;

public sealed class EmailService(IConfiguration _config ) : IEmailService
{
  public async Task SendEmailAsync(string to, string subject, string body)
  {
    // 1. appsettings.json içerisindeki ayarları şak diye okuyoruz kanka
    string smtpServer = _config["EmailSettings:SmtpServer"] ?? throw new InvalidOperationException("SmtpServer bulunamadı.");
    int port = int.TryParse(_config["EmailSettings:Port"], out var p) ? p : 587;
    string senderEmail = _config["EmailSettings:SenderEmail"] ?? throw new InvalidOperationException("SenderEmail bulunamadı.");
    string senderName = _config["EmailSettings:SenderName"] ?? "Rent A Car";
    // 🔥 DÜZELTME 1: Şifre boş gelebileceği için (smtp4dev'de boş) throw fırlatmayı kaldırıyoruz.
    string password = _config["EmailSettings:Password"] ?? "";

    // 2. Mail mesaj nesnemizi kurumsal standartta hazırlıyoruz kanka
    var mailMessage = new MailMessage
    {
      From = new MailAddress(senderEmail, senderName),
      Subject = subject,
      Body = body,
      IsBodyHtml = true // // Gönderdiğimiz <strong>{resetCode}</strong> HTML etiketleri çalışsın diye true yapıyoruz kanka
    };

    mailMessage.To.Add(to);

    // 3. Maili dış dünyaya fırlatmadan önce istemciyi pürüzsüzce yapılandırıyoruz kanka
    using var smtpClient = new SmtpClient(smtpServer, port);

    smtpClient.UseDefaultCredentials = false;

    // 🔥 DÜZELTME 2: Eğer şifre varsa kimlik bilgilerini ekle (Gerçek SMTP için), yoksa boş geç (smtp4dev için)
    if (!string.IsNullOrEmpty(password))
    {
      smtpClient.Credentials = new NetworkCredential(senderEmail, password);
    }

    // Böylece kodu production'a taşırken sürekli değiştirmek zorunda kalmazsın.
    smtpClient.EnableSsl = smtpServer != "localhost";

    // 🚀 Ve zurnanın hırt dediği yerde maili şak diye fırlatıyoruz kanka!
    await smtpClient.SendMailAsync(mailMessage);
  }

  public async Task<bool> SendVerificationCodeAsync(string to, string code, CancellationToken cancellationToken = default)
  {
    try
    {
      // 3-3 bölerek görsel okunabilirliği zirveye çıkarıyoruz (Örn: "482-915")
      string formattedCode = $"{code[..3]}-{code[3..]}";

      string subject = "Rent A Car - E-Posta Doğrulama Kodu";

      string body = $@"
            <div style='font-family: Arial, sans-serif; padding: 20px; color: #333; max-width: 600px; border: 1px solid #eee; border-radius: 8px;'>
                <h2 style='color: #2c3e50; text-align: center;'>Doğrulama Kodunuz</h2>
                <p style='text-align: center;'>Merhaba,</p>
                <p style='text-align: center;'>Rent A Car hesabınızı doğrulamak için aşağıdaki kodu kullanabilirsin:</p>
                
                <!-- Tek bakışta okunabilir büyük ve tireli tasarım -->
                <div style='background-color: #f8f9fa; border: 1px dashed #007bff; padding: 15px; font-size: 32px; font-weight: bold; letter-spacing: 4px; text-align: center; color: #007bff; margin: 20px 0; border-radius: 6px;'>
                    {formattedCode}
                </div>

                <p style='font-size: 13px; color: #777;'>Bu kod güvenliğiniz için 5 dakika boyunca geçerlidir.</p>
            </div>";

      await SendEmailAsync(to, subject, body);
      return true;
    }
    catch
    {
      return false;
    }
  }
}
