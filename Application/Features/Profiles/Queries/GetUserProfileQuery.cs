using Application.Features.Profiles.Dto;
using Application.Services;
using Domain.Repositories;
using MediatR;
using TS.Result;

namespace Application.Features.Profiles.Queries;

public sealed record GetUserProfileQuery() : IRequest<Result<GetUserProfileDto>>;


public sealed class GetUserProfileQueryHandler(
    IUserRepository _userRepo, // 👈 Senin canavar repo
    IUserContext _userContext         // 👈 Token'dan ID'yi söken büyücü
) : IRequestHandler<GetUserProfileQuery, Result<GetUserProfileDto>>
{
  public async Task<Result<GetUserProfileDto>> Handle(GetUserProfileQuery _req, CancellationToken _token)
  {
    // 1. Token'dan giriş yapan adamın ID'sini güvenle alıyoruz kanks
    Guid userId = _userContext.GetUserId();

    // 2. Repository üzerinden kullanıcıyı çekiyoruz
    var user = await _userRepo.FirstOrDefaultAsync(u => u.Id == userId, _token);
    if (user is null) return Result<GetUserProfileDto>.Failure("Kullanıcı profili bulunamadı!");

    var response = new GetUserProfileDto(
        user.Id,
        user.FirstName,
        user.LastName,
        user.FullName, 
        user.Email,
        user.PhoneNumber
    );

    return Result<GetUserProfileDto>.Succeed(response);
  }
}