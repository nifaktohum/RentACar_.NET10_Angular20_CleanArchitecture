using Application.Features.Profiles.Queries;
using Application.Features.Users.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiExplorerSettings(GroupName = "v1-Profiles")] // Swagger'da jilet gibi ayrı bir sekmede görünsün kanks
[Authorize] // Sadece giriş yapmış (Token'lı) kullanıcılar bu kapıdan geçebilir.
public class ProfileController : BaseApiController
{
  [HttpPut("update-profile")]
  public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileCommand command, CancellationToken _token)
  {
    // Komutu direkt MediatR üzerinden Handler'a fırlatıyoruz kanka
    var result = await Mediator.Send(command, _token);

    // TS.Result yapına göre başarılıysa 200 OK, hatalıysa 400 BadRequest dönüyoruz
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }

  [HttpGet("get-profile")] // URL: api/Profile/get-profile
  public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
  {
    // İçeriye parametre falan göndermiyoruz, MediatR arkada hallediyor kanks
    var result = await Mediator.Send(new GetUserProfileQuery(), cancellationToken);
    return result.IsSuccessful ? Ok(result) : BadRequest(result);
  }
}
