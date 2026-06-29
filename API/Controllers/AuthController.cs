using Application.Features.Auth.Register.Commands;
using Application.Features.Login.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TS.Result; // TS.Result paketinin nimetleri için

namespace API.Controllers;

[ApiExplorerSettings(GroupName = "v1-Auth")]
[AllowAnonymous]
public sealed class AuthController : BaseApiController // 👈 Constructor enjeksiyonunu tlı. :) temizlendi!
{
  // ==================> 🔥 KAYIT OLMA lı. :)) <================== //
  [HttpPost("register")]
  public async Task<IActionResult> Register([FromBody] RegisterCommand registerCommand, CancellationToken token)
  {
    var result = await Mediator.Send(registerCommand, token);

    if (result.IsSuccessful)
    {
      return Ok(new { id = result.Data, message = "Kullanıcı kaydı başarılı. :)" });
    }

    var statusCode = result.StatusCode == 0 ? 400 : result.StatusCode;
    return StatusCode(statusCode, new { error = result.ErrorMessages });
  }
  
  // ==================> GİRİŞ İŞLEMİ <================== //
  [HttpPost("login")]
  [EnableRateLimiting("LoginFixedWindowPolicy")]
  public async Task<IActionResult> Login([FromBody] LoginCommand loginCommand, CancellationToken token)
  {
    // Base'den gelen "Mediator" property'sini kullanıyoruz kanks
    var result = await Mediator.Send(loginCommand, token);

    if (result.IsSuccessful)
    {
      return Ok(result); // Direkt içerideki LoginResponse datası dönüyor
    }

    var statusCode = result.StatusCode == 0 ? 400 : result.StatusCode;
    return StatusCode(statusCode, new { error = result.ErrorMessages });
  }

  // ==================> E-POSTA İLE ŞİFRE SIFIRLAMA <================== //

  // 1. AŞAMA: KOD TALEP ETME
  [HttpPost("forgot-password")]
  [EnableRateLimiting("ForgotPasswordFixedWindowPolicy")]
  public async Task<IActionResult> ForgotPassword([FromBody] EmailResetCodeCommand forgotPasswordCommand, CancellationToken token)
  {
    var result = await Mediator.Send(forgotPasswordCommand, token);

    if (result.IsSuccessful)
    {
      return Ok(new { message = result.Data });
    }

    var statusCode = result.StatusCode == 0 ? 400 : result.StatusCode;
    return StatusCode(statusCode, new { error = result.ErrorMessages });
  }

  // 2. AŞAMA: YENİ ŞİFRE BELİRLEME
  [HttpPost("reset-password")]
  public async Task<IActionResult> ResetPassword([FromBody] EmailResetPasswordCommand resetPasswordCommand, CancellationToken token)
  {
    var result = await Mediator.Send(resetPasswordCommand, token);

    if (result.IsSuccessful)
    {
      return Ok(new { message = result.Data });
    }

    var statusCode = result.StatusCode == 0 ? 400 : result.StatusCode;
    return StatusCode(statusCode, new { error = result.ErrorMessages });
  }

  // ==================> SMS İLE ŞİFRE SIFIRLAMA <================== //

  [HttpPost("sms-reset-code")]
  public async Task<IActionResult> SendSmsResetCode([FromBody] SmsResetCodeCommand command, CancellationToken cancellationToken)
  {
    var result = await Mediator.Send(command, cancellationToken);

    // Burayı da yukardaki standart gibi sadece data dönecek şeklı. :)    if (result.IsSuccessful)
      // return Ok(new { message = result.Data });

    var statusCode = result.StatusCode == 0 ? 400 : result.StatusCode;
    return StatusCode(statusCode, new { error = result.ErrorMessages });
  }

  [HttpPost("sms-reset-password")]
  public async Task<IActionResult> ResetPasswordBySms([FromBody] SmsResetPasswordCommand command, CancellationToken cancellationToken)
  {
    var result = await Mediator.Send(command, cancellationToken);

    if (result.IsSuccessful)
      return Ok(new { message = result.Data });

    var statusCode = result.StatusCode == 0 ? 400 : result.StatusCode;
    return StatusCode(statusCode, new { error = result.ErrorMessages });
  }

  // ==================> GÜVENLİK VE ÇIKIŞ <================== //

  [HttpPost("logout-all-devices")]
  public async Task<IActionResult> LogoutAllDevices(CancellationToken cancellationToken)
  {
    var result = await Mediator.Send(new LogoutAllDevicesCommand(), cancellationToken);

    if (result.IsSuccessful)
      return Ok(result); // Tüm cihazlardan çıkışta objenin tamamını veya mesajı dönebilirsin

    var statusCode = result.StatusCode == 0 ? 400 : result.StatusCode;
    return StatusCode(statusCode, new { error = result.ErrorMessages });
  }
}