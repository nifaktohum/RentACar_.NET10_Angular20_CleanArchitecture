using Application.Behaviors;
using Application.Services;
using Domain.Repositories;
using Domain.Entities.Users;
using FluentValidation;
using GenericRepository;
using MediatR;
using TS.Result;

namespace Application.Features.Users.Commands;

[Permission("Users.Update")]
public record class UpdateUserCommand(
                          Guid Id,
                          string FirstName,
                          string LastName,
                          string Email,
                          string PhoneNumber,
                          Guid BranchId,
                          Guid RoleId,
                          bool IsActive,
                          Guid CreatedBy
                    ) : IRequest<Result<UserDto>> {}

public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
  public UpdateUserCommandValidator()
  {
    RuleFor(p => p.Id).NotEmpty().WithMessage("Kullanıcı ID zorunludur.");
    RuleFor(p => p.FirstName).NotEmpty().WithMessage("Ad alanı zorunludur.");
    RuleFor(p => p.LastName).NotEmpty().WithMessage("Soyad alanı zorunludur.");
    RuleFor(p => p.Email).NotEmpty().WithMessage("E-posta zorunludur.").EmailAddress();
    RuleFor(p => p.PhoneNumber).NotEmpty().WithMessage("Telefon numarası zorunludur.");
  }
}

public sealed class UpdateUserCommandHandler(
                              IUserRepository _userRepo,
                              IUnitOfWork _unit
                    ): IRequestHandler<UpdateUserCommand, Result<UserDto>>
{
  public async Task<Result<UserDto>> Handle(UpdateUserCommand _req, CancellationToken _token)
  {
    // 1. Kullanıcıyı detaylı (Include'lu) getir
    var user = await _userRepo.GetUserWithDetailsByIdAsync(_req.Id, _token);
    if (user is null)
      return Result<UserDto>.Failure("Kullanıcı bulunamadı.");

    // 2. Email kontrolü (Kendini hariç tut!)
    if (user.Email != _req.Email)
    {
      var isEmailExists = await _userRepo.AnyAsync(
          u => u.Email == _req.Email && u.Id != _req.Id && !u.IsDeleted,
          _token
      );

      if (isEmailExists)
        return Result<UserDto>.Failure("Bu e-posta adresi zaten kullanımda.");
    }

    // 3. Domain operasyonlarını tetikle
    user.UpdateInfo(_req.FirstName, _req.LastName, _req.Email, _req.PhoneNumber);
    user.UpdateRoles(new List<Guid> { _req.RoleId });
    user.SetBranch(_req.BranchId);
    user.SetActiveStatus(_req.IsActive);

    // 4. Değişiklikleri kaydet
    await _unit.SaveChangesAsync(_token);

    // 5. Güncel kullanıcıyı getir
    var updatedUser = await _userRepo.GetUserWithDetailsByIdAsync(user.Id, _token);
    if (updatedUser is null)
      return Result<UserDto>.Failure("Kullanıcı güncellendi ancak getirilemedi.");

    // 6. DTO oluştur
    var userDto = new UserDto(
        updatedUser.Id,
        updatedUser.FirstName,
        updatedUser.LastName,
        updatedUser.FullName,
        updatedUser.Email,
        updatedUser.PhoneNumber,
        updatedUser.Branch?.Name ?? "Şube Tanımsız",
        updatedUser.BranchId ?? Guid.Empty,
        updatedUser.UserRoles?
            .Where(ur => ur.Role != null)
            .Select(ur => ur.Role.Name)
            .ToList() ?? new List<string>(),
        updatedUser.IsActive,
        updatedUser.CreatedAt
    );

    return Result<UserDto>.Succeed(userDto);
  }
}