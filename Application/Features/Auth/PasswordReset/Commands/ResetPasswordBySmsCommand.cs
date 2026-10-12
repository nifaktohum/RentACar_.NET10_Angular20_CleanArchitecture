using Application.Services;
using Domain.Repositories;
using FluentValidation;
using GenericRepository;
using MediatR;
using TS.Result;

namespace Application.Features.Auth.PasswordReset.Commands;

public sealed record CustomerResetPasswordBySmsCommand(
                          string PhoneNumber,
                          string Code,
                          string NewPassword
                      ) : IRequest<Result<string>>;

public sealed class CustomerResetPasswordBySmsCommandValidator : AbstractValidator<CustomerResetPasswordBySmsCommand>
{
  public CustomerResetPasswordBySmsCommandValidator()
  {
    RuleFor(p => p.PhoneNumber)
        .NotEmpty().WithMessage("Telefon numarası zorunludur.");

    RuleFor(p => p.Code)
        .NotEmpty().WithMessage("Doğrulama kodu zorunludur.")
        .Length(6).WithMessage("Doğrulama kodu 6 haneli olmalıdır.")
        .Matches(@"^[0-9]+$").WithMessage("Doğrulama kodu sadece rakamlardan oluşmalıdır.");

    RuleFor(p => p.NewPassword)
        .NotEmpty().WithMessage("Yeni şifre zorunludur.")
        .MinimumLength(6).WithMessage("Şifre en az 6 karakter olmalıdır.")
        .Matches(@"[A-Z]").WithMessage("Şifre en az bir büyük harf içermelidir.")
        .Matches(@"[a-z]").WithMessage("Şifre en az bir küçük harf içermelidir.")
        .Matches(@"[0-9]").WithMessage("Şifre en az bir rakam içermelidir.");
  }
}

public sealed class CustomerResetPasswordBySmsCommandHandler(
                        ICustomerRepository _customerRepo,
                        IPasswordHasher _passwordHasher,
                        IUnitOfWork _unit
                    ) : IRequestHandler<CustomerResetPasswordBySmsCommand, Result<string>>
{
  public async Task<Result<string>> Handle(CustomerResetPasswordBySmsCommand _req, CancellationToken _token)
  {
    // 1. Telefon ile müşteriyi bul
    var customer = await _customerRepo.GetByPhoneNumberAsync(_req.PhoneNumber, _token);
    if (customer is null) return Result<string>.Failure("Geçersiz veya süresi dolmuş kod!");   // ⭐ Güvenlik

    // 2. ⭐ Domain metodu ile kodu doğrula (doğrulama başarılıysa kod temizlenir)
    bool isCodeValid = customer.VerifySmsResetCode(_req.Code);
    if (!isCodeValid) return Result<string>.Failure("Doğrulama kodu hatalı veya süresi dolmuş!");

    // 3. ⭐ Eski şifre kontrolü
    // bool isSameWithOldPassword = _passwordHasher.VerifyPassword(_req.NewPassword, customer.PasswordHash);
    // if (isSameWithOldPassword) return Result<string>.Failure("Yeni şifreniz, eski şifrenizle aynı olamaz!");

    // 4. Yeni şifreyi hash'le
    string newPasswordHash = _passwordHasher.HashPassword(_req.NewPassword);

    // 5. Domain metodu ile güncelle
    customer.UpdatePassword(newPasswordHash, Guid.Empty);

    // 6. ⭐ Güvenlik: Tüm oturumları sonlandır
    customer.ClearRefreshToken();
    customer.UpdateSecurityStamp();
    _customerRepo.Update(customer);
    
    await _unit.SaveChangesAsync(_token);

    return Result<string>.Succeed("Şifreniz SMS doğrulaması ile başarıyla güncellendi.");
  }
}