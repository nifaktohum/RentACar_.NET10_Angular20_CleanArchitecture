using Application.Services;
using Domain.Repositories;
using FluentValidation;
using GenericRepository;
using MediatR;
using TS.Result;

namespace Application.Features.Users.Commands;

public sealed record UpdateUserProfileCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber
) : IRequest<Result<string>>;

public sealed class UpdateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommand>
{
  public UpdateUserProfileCommandValidator()
  {
    RuleFor(p => p.Id).NotEmpty().WithMessage("Kullanıcı ID bilgisi boş olamaz.");
    RuleFor(p => p.FirstName).NotEmpty().WithMessage("Ad alanı boş olamaz.").MinimumLength(2).WithMessage("Ad en az 2 karakter olmalıdır.");
    RuleFor(p => p.LastName).NotEmpty().WithMessage("Soyad alanı boş olamaz.");
    RuleFor(p => p.Email).NotEmpty().WithMessage("E-posta adresi boş olamaz.").EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.");
    RuleFor(p => p.PhoneNumber).NotEmpty().WithMessage("Telefon numarası boş olamaz.");
  }
}

public sealed class UpdateUserProfileCommandHandler(
                            IUserRepository  _userRepo, 
                            IUnitOfWork _unit,         
                            IUserContext _userContext        
                    ) : IRequestHandler<UpdateUserProfileCommand, Result<string>>
{
  public async Task<Result<string>> Handle(UpdateUserProfileCommand _req, CancellationToken _token)
  {
    // 1. ID'yi yine senin yazdığın IUserContext üzerinden jilet gibi çekiyoruz
    Guid userId = _userContext.GetUserId();

    // 2. Kullanıcıyı repository üzerinden veritabanından sorguluyoruz kanks
    var user = await _userRepo.FirstOrDefaultAsync(u => u.Id == userId, _token);
    if (user is null) return Result<string>.Failure("Kullanıcı bulunamadı!");

    // 3. Eğer e-posta adresi değiştiyse, yeni e-postanın başkası tarafından alınıp alınmadığı kontrolü
    if (user.Email != _req.Email)
    {
      // Repository'inde e-postaya göre kontrol eden bir metot olduğunu varsayıyorum (Örn: AnyAsync veya GetByEmailAsync)
      bool isEmailExist = await _userRepo.AnyAsync(u => u.Email == _req.Email, _token);
      if (isEmailExist)
      {
        return Result<string>.Failure("Bu e-posta adresi başka bir kullanıcı tarafından kullanımda");
      }
    }

    user.UpdateInfo(_req.FirstName, _req.LastName, _req.Email, _req.PhoneNumber);

    // 5. Repository üzerinde update işaretlemesi yapıyoruz (Ef Core kullanıyorsan bu satır state'i Modified yapar)
    _userRepo.Update(user);

    // 6. Değişiklikleri UnitOfWork vasıtasıyla veritabanına tek bir transaction olarak yazıyoruz kanks
    await _unit.SaveChangesAsync(_token);

    return Result<string>.Succeed("Profil bilgilerin kendi repository altyapınla başarıyla güncellendi!");
  }
}