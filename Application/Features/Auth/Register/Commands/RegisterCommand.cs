using Application.Services;
using Domain.Repositories;
using Domain.Entities.Users;
using FluentValidation;
using GenericRepository;
using MediatR;
using TS.Result;

namespace Application.Features.Auth.Register.Commands;

public sealed record RegisterCommand(
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string Password,
    Guid BranchId,
    Guid RoleId
) : IRequest<Result<Guid>>;

public sealed class RegisterCommandValidation : AbstractValidator<RegisterCommand>
{
  public RegisterCommandValidation()
  {
    RuleFor(p => p.FirstName)
        .NotEmpty().WithMessage("Ad alanı boş olamaz.")
        .NotNull().WithMessage("Ad alanı zorunludur.")
        .MaximumLength(50).WithMessage("Ad alanı en fazla 50 karakter olabilir.");

    RuleFor(p => p.LastName)
        .NotEmpty().WithMessage("Soyad alanı boş olamaz.")
        .NotNull().WithMessage("Soyad alanı zorunludur.")
        .MaximumLength(50).WithMessage("Soyad alanı en fazla 50 karakter olabilir.");

    RuleFor(p => p.Email)
        .NotEmpty().WithMessage("E-posta adresi boş olamaz.")
        .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.");

    RuleFor(p => p.PhoneNumber)
        .NotEmpty().WithMessage("Telefon numarası boş olamaz.");

    RuleFor(p => p.Password)
        .NotEmpty().WithMessage("Şifre boş olamaz.")
        .NotNull().WithMessage("Şifre zorunludur.")
        .MinimumLength(6).WithMessage("Şifre en az 6 karakter olmalıdır.")
        // 🔥 İşte istediğin o zırhlı kural:
        .Matches(@"[A-Z]").WithMessage("Şifre en az bir büyük harf içermelidir!")
        .Matches(@"[a-z]").WithMessage("Şifre en az bir küçük harf içermelidir!")
        .Matches(@"[0-9]").WithMessage("Şifre en az bir rakam içermelidir!");
  }
}

public sealed class RegisterCommandHandler(
                          IUserRepository _userRepo,
                          IRoleRepository _roleRepo, // 👈 Rolü bulabilmek için ekledik
                          IPasswordHasher _passwordHasher,
                          IUnitOfWork _unit
) : IRequestHandler<RegisterCommand, Result<Guid>>
{
  public async Task<Result<Guid>> Handle(RegisterCommand _req, CancellationToken _token)
  {
    // 1. E-posta adresi sistemde zaten var mı? (Giriş süzgeci)
    var isEmailExists = await _userRepo.AnyAsync(u => u.Email == _req.Email, _token);
    if (isEmailExists)
      return Result<Guid>.Failure("Bu e-posta adresi kullanımda!");

    // 2. Şifreyi güvenli bir şekilde hash'le
    string passwordHash = _passwordHasher.HashPassword(_req.Password);

    // 3. Yeni User nesnesini senin o canavar domain constructor'ı ile yarat
    // CreatedBy alanına sistem kaydı olduğu için Guid.Empty veya newUser'ın kendi ID'sini geçebilirsin.
    var newUser = new User(
        _req.FirstName,
        _req.LastName,
        _req.Email,
        _req.PhoneNumber,
        passwordHash,
        _req.BranchId,
        _req.RoleId,
        Guid.Empty
    );

    // 4. 🔥 ROL EVLİLİĞİ: Veritabanından "Customer" rolünü çekiyoruz
    var customerRole = await _roleRepo.FirstOrDefaultAsync(r => r.Name == "Customer", _token);

    if (customerRole is null)
      return Result<Guid>.Failure("Sistemde 'Customer' rolü bulunamadı!");

    // Senin User entity'sinin içindeki o akıllı domain metoduyla rolü bağlıyoruz
    newUser.AddRole(customerRole.Id);

    // 5. Veritabanına ekle ve Unit of Work ile kilidi vur!
    await _userRepo.AddAsync(newUser, _token);
    await _unit.SaveChangesAsync(_token);


    return Result<Guid>.Succeed(newUser.Id);

  }
}
