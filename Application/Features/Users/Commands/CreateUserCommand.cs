using Application.Behaviors;
using Application.Services;
using Domain.Repositories;
using Domain.Users;
using GenericRepository;
using MediatR;
using TS.Result;

namespace Application.Features.Users.Commands;

[Permission("Users.Create")]
public record CreateUserCommand(
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string Password,
    Guid BranchId,
    Guid RoleId,
    Guid CreatedBy
) : IRequest<Result<UserDto>>;


public sealed class CreateUserCommandHanler(
                          IUserRepository _userRepo,
                          IUnitOfWork _unit,
                          IPasswordHasher _passwordHasher
                    ) : IRequestHandler<CreateUserCommand, Result<UserDto>>
{
  public async Task<Result<UserDto>> Handle(CreateUserCommand _req, CancellationToken _token)
  {
    // 1. Email kontrolü (Artık repository'deki jilet metodunu kullanabilirsin)
    var isEmailUnique = await _userRepo.IsEmailUniqueAsync(_req.Email, _token);
    if (!isEmailUnique)
    {
      return Result<UserDto>.Failure("Bu email adresi sistemde zaten kayıtlı!");
    }

    // 2. Şifre hash'leme
    string passwordHash = _passwordHasher.HashPassword(_req.Password);

    // 3. Domain Entity oluştur (BranchId ve RoleId ZORUNLU!)
    var user = new User(
        _req.FirstName,
        _req.LastName,
        _req.Email,
        _req.PhoneNumber,
        passwordHash,
        _req.BranchId,
        _req.RoleId,
        _req.CreatedBy
    );
    // 4. Veritabanına ekle
    await _userRepo.AddAsync(user, _token);
    await _unit.SaveChangesAsync(_token);

    // 🚀 ÇÖZÜM: Kullanıcıyı "Include" edilmiş şekilde tekrar çekiyoruz.
    // Bu metodun içinde Branch ve UserRoles.Role kısımlarını .Include() ile çekmelisin.
    var resBranchRoles = await _userRepo.GetUserWithDetailsByIdAsync(user.Id , _token);
    
    user.AddRole(_req.RoleId);

    var branchName = resBranchRoles!.Branch != null ? resBranchRoles.Branch.Name : "Branch Nesnesi NULL";
    if (resBranchRoles.Branch != null && string.IsNullOrEmpty(resBranchRoles.Branch.Name)) branchName = "Name Alanı BOŞ";
    var roleNames = resBranchRoles?.UserRoles?
    .Where(ur => ur.Role != null)
    .Select(ur => ur.Role.Name)
    .ToList() ?? new List<string>();


    var userDto = new UserDto(
        user.Id,
        user.FirstName,
        user.LastName,
        user.FullName,
        user.Email,
        user.PhoneNumber,
        branchName, // 👈 Burayı yukarıdaki değişkenden al
        user.BranchId ?? Guid.Empty,
        roleNames!,
        user.IsActive,
        user.CreatedAt
    );

    return Result<UserDto>.Succeed(userDto);
  }
}