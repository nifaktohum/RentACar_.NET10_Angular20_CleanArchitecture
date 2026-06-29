using Application.Services;
using Domain.Repositories;
using FluentValidation;
using GenericRepository;
using MediatR;
using TS.Result;

namespace Application.Features.Login.Commands;

public sealed record SmsResetCodeCommand(string PhoneNumber) : IRequest<Result<string>>;

public sealed class SmsResetCodeCommandValidator : AbstractValidator<SmsResetCodeCommand>
{
  public SmsResetCodeCommandValidator()
  {
    RuleFor(p => p.PhoneNumber)
        .NotEmpty().WithMessage("Telefon numarası boş olamaz .")
        .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Lütfen geçerli bir uluslararası telefon numarası girin. (Örn: +90555...)");
  }
}

public sealed class SendSmsResetCodeHandler(
                          IUserRepository _userRepo,
                          ISmsService _smsService,
                          IUnitOfWork _unit
) : IRequestHandler<SmsResetCodeCommand, Result<string>>
{
  public async Task<Result<string>> Handle(SmsResetCodeCommand _req, CancellationToken _token)
  {
    // 1. Gelen telefon numarasına ait bir kullanıcı var mı kontrol et 
    var user = await _userRepo.FirstOrDefaultAsync(u => u.PhoneNumber == _req.PhoneNumber, _token);
    if (user is null) return Result<string>.Failure(404, "Bu telefon numarasına ait bir kullanıcı bulunamadı .");

    string randomOtpCode = Random.Shared.Next(100000, 999999).ToString();

    // 3. Entity içine tiresiz ham hali kaydediliyor (Burası doğru kanka)
    user.SmsResetCodeGenerate(randomOtpCode);

    // 🔥 GARANTİ YÖNTEM: Substring ile ilk 3 karakteri al, araya tire koy, kalan 3 karakteri al kanka!
    string formattedCode = $"{randomOtpCode.Substring(0, 3)}-{randomOtpCode.Substring(3, 3)}";

    // 4. Twilio servisimize içeriği basıyoruz
    string smsContent = $"RentACar şifre sıfırlama kodunuz: {formattedCode}. Bu kod 5 dakika geçerlidir.";

    // Kontrol için konsola da yazdıralım, backend loglarında jilet gibi görelim kanka:
    Console.WriteLine($"[TWILIO DEBUG] Gönderilen Formatlı Kod: {formattedCode}");

    bool isSmsSent = await _smsService.SendSmsAsync(user.PhoneNumber, smsContent, _token);

    if (!isSmsSent)
    {
      return Result<string>.Failure(500, "SMS sağlayıcısından kaynaklı bir hata oluştu, kod gönderilemedi.");
    }

    // 5. Değişiklikleri UnitOfWork ile veritabanına mühürle 
    _userRepo.Update(user);
    await _unit.SaveChangesAsync(_token);

    return Result<string>.Succeed("Şifre sıfırlama kodu telefonunuza başarıyla gönderildi.");
  }
}