using Application.Features.Auth.Customers.Login.Commands;
using Application.Features.Auth.Customers.Register.Commands;
using Application.Features.Auth.PasswordReset.Commands;
using Application.Features.Auth.Verification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers.Customers;

[ApiExplorerSettings(GroupName = "v1-Customers-Auth")]
public sealed class CustomersAuthController : BaseApiController
{
  // ==================== 📝 REGISTER ====================

  [HttpPost("register-individual")]
  [AllowAnonymous]
  public async Task<IActionResult> RegisterIndividual([FromBody] RegisterIndividualCommand command, CancellationToken token)
  {
    var result = await Mediator.Send(command, token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  [HttpPost("register-corporate")]
  [AllowAnonymous]
  public async Task<IActionResult> RegisterCorporate([FromBody] RegisterCorporateCommand command, CancellationToken token)
  {
    var result = await Mediator.Send(command, token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  // ==================== 🔐 LOGIN ====================

  [HttpPost("customer-login")]
  [AllowAnonymous]
  [EnableRateLimiting("LoginFixedWindowPolicy")]
  public async Task<IActionResult> Login([FromBody] CustomerLoginCommand command, CancellationToken token)
  {
    var result = await Mediator.Send(command, token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  // ==================== 🔄 REFRESH TOKEN ====================

  [HttpPost("refresh-token")]
  [AllowAnonymous]
  public async Task<IActionResult> RefreshToken([FromBody] CustomerRefreshTokenCommand command, CancellationToken token)
  {
    var result = await Mediator.Send(command, token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  // ==================== 🚪 LOGOUT ====================
  // ⚠️ Logout için [Authorize] gerekli — ama controller [AllowAnonymous]
  // Bu yüzden metot seviyesinde [Authorize] kullanıyoruz

  [HttpPost("customer-logout")]
  [Authorize]
  public async Task<IActionResult> Logout(CancellationToken token)
  {
    var result = await Mediator.Send(new CustomerLogoutCommand(), token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  [HttpPost("logout-all-devices")]
  [Authorize]
  public async Task<IActionResult> LogoutAllDevices(CancellationToken token)
  {
    var result = await Mediator.Send(new CustomerLogoutAllDevicesCommand(), token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  // ==================== 📧 EMAIL VERIFICATION ====================

  [HttpPost("send-email-verification-code")] // Emaile dogrularma kodu gönder. 
  [Authorize]
  public async Task<IActionResult> SendEmailVerificationCode(CancellationToken token)
  {
    var result = await Mediator.Send(new SendEmailVerificationCodeCommand(), token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  [HttpPost("verify-email")] // Emil ile gelen kodu gönder 
  [Authorize]
  public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailCommand command, CancellationToken token)
  {
    var result = await Mediator.Send(command, token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  // ==================== 📱 SMS VERIFICATION ====================

  [HttpPost("send-sms-verification-code")] //iletişimMerkezini kullanıyoruz sadece SMS göderiyor. Code göndermiyor. giden Code'du DBeaver'den elde ediyoruz. 
  [Authorize]
  public async Task<IActionResult> SendSmsVerificationCode(CancellationToken token)
  {
    var result = await Mediator.Send(new SendSmsVerificationCodeCommand(), token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  [HttpPost("verify-phone")]
  [Authorize]
  public async Task<IActionResult> VerifyPhone([FromBody] VerifyPhoneCommand command, CancellationToken token)
  {
    var result = await Mediator.Send(command, token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  // ==================== 🔑 PASSWORD RESET (EMAIL) ====================

  [HttpPost("forgot-password-email-code")]
  [EnableRateLimiting("ForgotPasswordFixedWindowPolicy")]
  [AllowAnonymous]
  public async Task<IActionResult> ForgotPasswordByEmail([FromBody] SendEmailPasswordResetCodeCommand command, CancellationToken token)
  {
    var result = await Mediator.Send(command, token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  [HttpPost("reset-password-email")]
  [AllowAnonymous]
  public async Task<IActionResult> ResetPasswordByEmail([FromBody] ResetPasswordByEmailCommand command, CancellationToken token)
  {
    var result = await Mediator.Send(command, token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  // ==================== 🔑 PASSWORD RESET (SMS) ====================

  [HttpPost("forgot-password-sms-code")]
  [AllowAnonymous]
  public async Task<IActionResult> ForgotPasswordBySms([FromBody] SendSmsPasswordResetCodeCommand command, CancellationToken token)
  {
    var result = await Mediator.Send(command, token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  [HttpPost("reset-password-sms")]
  [AllowAnonymous]
  public async Task<IActionResult> ResetPasswordBySms([FromBody] CustomerResetPasswordBySmsCommand command, CancellationToken token)
  {
    var result = await Mediator.Send(command, token);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

}
