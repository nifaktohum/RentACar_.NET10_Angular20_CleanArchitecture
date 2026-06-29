using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Services;
using Domain.Repositories;
using FluentValidation;
using GenericRepository;
using MediatR;
using TS.Result;

namespace Application.Features.Login.Commands;

public sealed record SmsResetPasswordCommand(
    string PhoneNumber,
    string Code,
    string NewPassword
) : IRequest<Result<string>>;


public sealed class SmsResetPasswordCommandValidation : AbstractValidator<SmsResetPasswordCommand>
{
  public SmsResetPasswordCommandValidation()
  {
    // 1. Telefon Numarası Alanı Kuralları
    RuleFor(p => p.PhoneNumber)
        .NotEmpty().WithMessage("Telefon numarası boş olamaz kanka.")
        .NotNull().WithMessage("Telefon numarası zorunludur.");

    // 2. SMS Onay Kodu (Code) Alanı Kuralları
    RuleFor(p => p.Code)
        .NotEmpty().WithMessage("SMS onay kodu boş olamaz.")
        .NotNull().WithMessage("SMS onay kodu zorunludur.");

    // 3. Yeni Şifre (NewPassword) Alanı Kuralları
    RuleFor(p => p.NewPassword)
        .NotEmpty().WithMessage("Yeni şifre alanı boş olamaz.")
        .NotNull().WithMessage("Yeni şifre alanı zorunludur.")
        .MinimumLength(6).WithMessage("Yeni şifre en az 6 karakter olmalıdır kanka.");
  }
}


public sealed class SmsResetPasswordCommandHandler(
            IUserRepository _userRepo,
            IPasswordHasher _passwordHasher,
            IUnitOfWork _unit
) : IRequestHandler<SmsResetPasswordCommand, Result<string>>
{
  public async Task<Result<string>> Handle(SmsResetPasswordCommand _req, CancellationToken _token)
  {
    // 1. Kullanıcıyı telefon numarasıyla bul 
    var user = await _userRepo.FirstOrDefaultAsync(u => u.PhoneNumber == _req.PhoneNumber, _token);
    if (user is null) return Result<string>.Failure(404, "Kullanıcı bulunamadı .");

    // 2. Güvenlik Kontrolü: Veritabanında bir SMS kodu var mı ve gelen kodla eşleşiyor mu?
    if (string.IsNullOrEmpty(user.SmsResetCode) || user.SmsResetCode != _req.Code.Trim())
    {
      return Result<string>.Failure(400, "Girdiğiniz doğrulama kodu hatalı .");
    }

    // 3. Süre Kontrolü: 5 dakikalık süre dolmuş mu?
    if (user.SmsResetCodeExpires < DateTime.UtcNow)
    {
      return Result<string>.Failure(400, "Doğrulama kodunun süresi dolmuş. Lütfen tekrar kod isteyiniz.");
    }

    // 🔥 Eski şifre kontrolü - Burası çok elit bir detay olmuş !
    bool isSameWithOldPassword = _passwordHasher.VerifyPassword(_req.NewPassword, user.PasswordHash);
    if (isSameWithOldPassword)
    {
      return Result<string>.Failure(400, "Yeni şifreniz, eski şifrenizle aynı olamaz . Lütfen farklı bir şifre belirleyin.");
    }

    // 4. Şifre Güncelleme: Yeni gelen şifreyi güvenli bir şekilde hashleyip entity'e geçiyoruz
    string hashedPassword = _passwordHasher.HashPassword(_req.NewPassword);
    user.UpdatePassword(hashedPassword);

    // 5. Temizlik: Güvenlik için SMS kodlarını null yaparak boşa çıkarıyoruz
    user.ClearSmsResetCode();

    // 6. Değişiklikleri veritabanına mühürle 
    _userRepo.Update(user);
    await _unit.SaveChangesAsync(_token);

    return Result<string>.Succeed("Şifreniz SMS doğrulaması ile başarıyla güncellendi .");
  }
}