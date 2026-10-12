using Application.Services;
using Domain.Entities.Customers;
using Domain.Repositories;
using FluentValidation;
using GenericRepository;
using MediatR;
using TS.Result;

namespace Application.Features.Auth.Customers.Register.Commands;

public sealed record RegisterCorporateCommand(
                        string Email,              // Kurumsal hesabın giriş ve iletişim e-posta adresi
                        string PhoneNumber,        // Şirketin iletişim/telefon numarası
                        string Password,           // Hesabın güvenliğini sağlayan şifre
                        string CompanyName,        // Şirketin resmi unvanı / firma adı
                        string TaxNumber,          // Fatura ve resmi işlemler için vergi kimlik numarası (VKN)
                        string TaxOffice,          // Şirketin bağlı olduğu vergi dairesi
                        string Address,            // Şirketin açık fatura ve tebligat adresi
                        string City,               // Şirketin bulunduğu şehir
                        string Country,            // Şirketin bulunduğu ülke
                        string? PostalCode = null  // Bölgeye ait posta kodu (Opsiyonel)
                    ) : IRequest<Result<Guid>>;

public sealed class RegisterCorporateCommandValidator : AbstractValidator<RegisterCorporateCommand>
{
  public RegisterCorporateCommandValidator()
  {
    // ==================== KURUMSAL ====================
    RuleFor(p => p.CompanyName)
        .NotEmpty().WithMessage("Şirket adı zorunludur.")
        .MaximumLength(200).WithMessage("Şirket adı en fazla 200 karakter olabilir.");

    RuleFor(p => p.TaxNumber)
        .NotEmpty().WithMessage("Vergi numarası zorunludur.")
        .MaximumLength(20).WithMessage("Vergi numarası en fazla 20 karakter olabilir.")
        .Matches(@"^[0-9]+$").WithMessage("Vergi numarası sadece rakamlardan oluşmalıdır.");

    RuleFor(p => p.TaxOffice)
        .NotEmpty().WithMessage("Vergi dairesi zorunludur.")
        .MaximumLength(100).WithMessage("Vergi dairesi en fazla 100 karakter olabilir.");

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
}

public sealed class RegisterCorporateCommandHandler(
                      ICustomerRepository _customerRepo,
                      IPasswordHasher _passwordHasher,
                      IUnitOfWork _unit
                  ) : IRequestHandler<RegisterCorporateCommand, Result<Guid>>
{
  public async Task<Result<Guid>> Handle(RegisterCorporateCommand _req, CancellationToken _token)
  {
    // 1. Email benzersiz mi?
    var isEmailExists = await _customerRepo.IsEmailExistsAsync(_req.Email, _token);
    if (isEmailExists) return Result<Guid>.Failure("Bu e-posta adresi zaten kullanımda!");

    // 2. Telefon benzersiz mi?
    var isPhoneExists = await _customerRepo.IsPhoneNumberExistsAsync(_req.PhoneNumber, _token);
    if (isPhoneExists) return Result<Guid>.Failure("Bu telefon numarası zaten kullanımda!");

    // 3. Vergi numarası benzersiz mi?
    var isTaxExists = await _customerRepo.IsTaxNumberExistsAsync(_req.TaxNumber, _token);
    if (isTaxExists) return Result<Guid>.Failure("Bu vergi numarası ile kayıtlı bir müşteri zaten var!");

    // 4. Şifreyi hash'le
    string passwordHash = _passwordHasher.HashPassword(_req.Password);

    // 5. Domain factory metodu ile Corporate Customer oluştur
    var customer = Customer.CreateCorporate(
        email: _req.Email,
        phoneNumber: _req.PhoneNumber,
        passwordHash: passwordHash,
        companyName: _req.CompanyName,
        taxNumber: _req.TaxNumber,
        taxOffice: _req.TaxOffice,
        address: _req.Address,
        city: _req.City,
        country: _req.Country,
        postalCode: _req.PostalCode,
        createdBy: Guid.Empty   // ⭐ Self-registration
    );

    // 6. Kaydet
    await _customerRepo.AddAsync(customer, _token);
    await _unit.SaveChangesAsync(_token);

    // 7. ID dön
    return Result<Guid>.Succeed(customer.Id);
  }
}