using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
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
}
