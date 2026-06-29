using Application.Features.Login.Responses;
using Application.Services;
using Domain.Repositories;
using FluentValidation;
using GenericRepository;
using MediatR;
using TS.Result;

namespace Application.Features.Login.Commands;

public sealed record LoginCommand(
    string Email,
    string Password,
    bool RememberMe
) : IRequest<Result<LoginResponse>>;

public sealed class LoginCommandValidation : AbstractValidator<LoginCommand>
{
  public LoginCommandValidation()
  {
    // Email alanı için geçerlilik kurallarını tanımlıyoruz
    RuleFor(p => p.Email)
        // Alanın null olmamasını garanti altına alır
        .NotNull().WithMessage("E-posta adresi zorunludur.")
        // Alanın boş bırakılmasını veya sadece boşluklardan oluşmasını engeller
        .NotEmpty().WithMessage("E-posta adresi boş olamaz.")
        // Gelen metnin geçerli bir e-posta formatı (örn: ad@soyad.com) olup olmadığını kontrol eder
        .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.");

    // Password alanı için geçerlilik kurallarını tanımlıyoruz
    RuleFor(p => p.Password)
        // Şifre alanının boş geçilmesini engeller
        .NotEmpty().WithMessage("Şifre boş olamaz.")
        // Şifre alanının null olmamasını sağlar
        .NotNull().WithMessage("Şifre zorunludur.")
        // Şifrenin en az 6 karakter olmasını zorunlu kılar (İsteğe göre artırabilirsin kanka)
        .MinimumLength(6).WithMessage("Şifre en az 6 karakter olmalıdır.");
  }
}

public sealed class LoginCommandHandler(
                          IUserRepository _userRepo,
                          IPasswordHasher _passwordHasher,
                          IJwtProvider _jwtProvider,
                          IUnitOfWork _unit
) : IRequestHandler<LoginCommand, Result<LoginResponse>> // Dönüş tipini Result<LoginResponse> olarak güncelledik
{
  public async Task<Result<LoginResponse>> Handle(LoginCommand _req, CancellationToken _token)
  {
    var user = await _userRepo.GetByEmailWithRolesAsync(_req.Email, _token);
    if (user is null)
    {
      return Result<LoginResponse>.Failure("E-posta adresi veya şifre hatalı!");
    }

    // Stamp kontrolü
    if (string.IsNullOrEmpty(user.SecurityStamp))
    {
      user.UpdateSecurityStamp();
      await _unit.SaveChangesAsync(_token);
    }
    // Kullanıcının girdiği ham şifre ile veritabanındaki hash'lenmiş şifreyi karşılaştırıyoruz
    bool isPasswordVerified = _passwordHasher.VerifyPassword(_req.Password, user.PasswordHash);
    if (!isPasswordVerified)
    {
      return Result<LoginResponse>.Failure("E-posta veya şifre hatalı.");
    }

    var roles = await _userRepo.GetUserRoleNamesAsync(user.Id, _token);
    var permissions = await _userRepo.GetUserPermissionsAsync(user.Id, _token);
    var branchName = user.Branch?.Name ?? "Merkez";
    
    // 4. JwtProvider'a tüm listeleri paslayıp token'ı üretiyoruz
    string token = _jwtProvider.CreateToken(user, roles, branchName, permissions, _req.RememberMe);

    // Başarılı cevabı TS.Result.Success ile sarmalayarak, Angular'ın beklediği LoginResponse modeliyle geri dönüyoruz
    var loginResponse = new LoginResponse(
              Token: token,
              UserId: user.Id,
              FullName: user.FullName,
              Email: user.Email,
              Roles: roles,
              BranchName: branchName,
              Permissions: permissions
    );

    // Her şey yolundaysa token'ı response olarak fırlatıyoruz kanks!
    return Result<LoginResponse>.Succeed(loginResponse);
  }
}