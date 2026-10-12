using Application.Services;
using Domain.Entities.Customers;
using Domain.Repositories;
using FluentValidation;
using GenericRepository;
using MediatR;
using TS.Result;

namespace Application.Features.Auth.Customers.Register.Commands;

public sealed record RegisterIndividualCommand(
                        string Email,          // Kullanıcının oturum açma ve iletişim için kullanacağı e-posta adresi
                        string PhoneNumber,    // İletişim ve doğrulama işlemleri için telefon numarası
                        string Password,       // Hesabın güvenliğini sağlayan şifre
                        string FirstName,      // Müşterinin adı
                        string LastName,       // Müşterinin soyadı
                        string IdentityNumber, // Kimlik doğrulama ve resmi işlemler için T.C. Kimlik / Pasaport no
                        DateOnly BirthDate,    // Doğum tarihi (YYYY-MM-DD formatında)
                        string Address,        // Fatura ve sözleşmeler için açık adres bilgisi
                        string City,           // Adresin bulunduğu şehir
                        string Country,        // Adresin bulunduğu ülke
                        string? Gender = null,     // Cinsiyet bilgisi (Opsiyonel - örn: "Male", "Female")
                        string? PostalCode = null  // Bölgeye ait posta kodu (Opsiyonel)
                    ) : IRequest<Result<Guid>>;
public sealed class RegisterIndividualCommandValidator : AbstractValidator<RegisterIndividualCommand>
{
  public RegisterIndividualCommandValidator()
  {
    // ==================== KİMLİK ====================
    RuleFor(p => p.FirstName)
        .NotEmpty().WithMessage("Ad zorunludur.")
        .MaximumLength(100).WithMessage("Ad en fazla 100 karakter olabilir.");

    RuleFor(p => p.LastName)
        .NotEmpty().WithMessage("Soyad zorunludur.")
        .MaximumLength(100).WithMessage("Soyad en fazla 100 karakter olabilir.");

    RuleFor(p => p.IdentityNumber)
        .NotEmpty().WithMessage("T.C. Kimlik No zorunludur.")
        .Length(11).WithMessage("T.C. Kimlik No 11 haneli olmalıdır.")
        .Matches(@"^[0-9]+$").WithMessage("T.C. Kimlik No sadece rakamlardan oluşmalıdır.");

    RuleFor(p => p.BirthDate)
        .NotEmpty().WithMessage("Doğum tarihi zorunludur.")
        .LessThan(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Doğum tarihi bugünden ileri bir tarih olamaz.")
        .Must(BeAtLeast18YearsOld)
            .WithMessage("Araç kiralayabilmek için en az 18 yaşında olmalısınız.");

    // ==================== İLETİŞİM ====================
    RuleFor(p => p.Email)
        .NotEmpty().WithMessage("E-posta adresi zorunludur.")
        .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.")
        .MaximumLength(200).WithMessage("E-posta adresi en fazla 200 karakter olabilir.");

    RuleFor(p => p.PhoneNumber)
        .NotEmpty().WithMessage("Telefon numarası zorunludur.")
        .MaximumLength(20).WithMessage("Telefon numarası en fazla 20 karakter olabilir.");

    RuleFor(p => p.Address)
        .NotEmpty().WithMessage("Adres zorunludur.")
        .MaximumLength(500).WithMessage("Adres en fazla 500 karakter olabilir.");

    RuleFor(p => p.City)
        .NotEmpty().WithMessage("Şehir zorunludur.")
        .MaximumLength(100).WithMessage("Şehir en fazla 100 karakter olabilir.");

    RuleFor(p => p.Country)
        .NotEmpty().WithMessage("Ülke zorunludur.")
        .MaximumLength(100).WithMessage("Ülke en fazla 100 karakter olabilir.");

    // ==================== ŞİFRE ====================
    RuleFor(p => p.Password)
        .NotEmpty().WithMessage("Şifre zorunludur.")
        .MinimumLength(6).WithMessage("Şifre en az 6 karakter olmalıdır.")
        .Matches(@"[A-Z]").WithMessage("Şifre en az bir büyük harf içermelidir.")
        .Matches(@"[a-z]").WithMessage("Şifre en az bir küçük harf içermelidir.")
        .Matches(@"[0-9]").WithMessage("Şifre en az bir rakam içermelidir.");
  }
  /// 18 yaş kontrolü (Customer entity'sindeki GetAge ile aynı mantık)
  private static bool BeAtLeast18YearsOld(DateOnly birthDate)
  {
    var today = DateOnly.FromDateTime(DateTime.UtcNow);
    var age = today.Year - birthDate.Year;
    if (birthDate > today.AddYears(-age)) age--;
    return age >= 18;
  }
}

public sealed class RegisterIndividualCommandHandler(
                        ICustomerRepository _customerRepo,
                        IPasswordHasher _passwordHasher,
                        IUnitOfWork _unit
                    ) : IRequestHandler<RegisterIndividualCommand, Result<Guid>>
{
  public async Task<Result<Guid>> Handle(RegisterIndividualCommand _req, CancellationToken _token)
  {
    // 1. Email benzersiz mi?
    var isEmailExists = await _customerRepo.IsEmailExistsAsync(_req.Email, _token);
    if (isEmailExists) return Result<Guid>.Failure("Bu e-posta adresi zaten kullanımda!");

    // 2. Telefon benzersiz mi?
    var isPhoneExists = await _customerRepo.IsPhoneNumberExistsAsync(_req.PhoneNumber, _token);
    if (isPhoneExists) return Result<Guid>.Failure("Bu telefon numarası zaten kullanımda!");

    // 3. T.C. Kimlik No benzersiz mi?
    var isIdentityExists = await _customerRepo.IsIdentityNumberExistsAsync(_req.IdentityNumber, _token);
    if (isIdentityExists) return Result<Guid>.Failure("Bu T.C. Kimlik No ile kayıtlı bir müşteri zaten var!");

    // 4. Şifreyi hash'le
    string passwordHash = _passwordHasher.HashPassword(_req.Password);

    // 5. Domain factory metodu ile Customer oluştur
    var customer = Customer.CreateIndividual(
        email: _req.Email,
        phoneNumber: _req.PhoneNumber,
        passwordHash: passwordHash,
        firstName: _req.FirstName,
        lastName: _req.LastName,
        identityNumber: _req.IdentityNumber,
        birthDate: _req.BirthDate,
        address: _req.Address,
        city: _req.City,
        country: _req.Country,
        gender: _req.Gender,
        postalCode: _req.PostalCode,
        createdBy: Guid.Empty   // ⭐ Self-registration → sistem kaydı
    );


    // 6. Kaydet
    await _customerRepo.AddAsync(customer, _token);
    await _unit.SaveChangesAsync(_token);

    // 7. ID dön
    return Result<Guid>.Succeed(customer.Id);
  }
}

