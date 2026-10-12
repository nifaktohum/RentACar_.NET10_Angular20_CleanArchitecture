using Application.Services;
using Domain.Entities.Customers;
using Domain.Repositories;
using FluentValidation;
using GenericRepository;
using MediatR;
using TS.Result;

namespace Application.Features.Customers.Profile.Commands;

public sealed record UpdateCustomerProfileCommand(
                        // Ortak
                        string PhoneNumber,
                        string Address,
                        string City,
                        string Country,
                        string? PostalCode,

                        // Bireysel (Individual için)
                        string? FirstName,
                        string? LastName,
                        string? IdentityNumber,
                        DateOnly? BirthDate,
                        string? Gender,

                        // Kurumsal (Corporate için)
                        string? CompanyName,
                        string? TaxNumber,
                        string? TaxOffice,

                        // Ehliyet
                        string? DriverLicenseNumber,
                        DateOnly? DriverLicenseIssueDate,
                        string? DriverLicenseClass
                    ) : IRequest<Result<string>>;

public sealed class UpdateCustomerProfileCommandValidator : AbstractValidator<UpdateCustomerProfileCommand>
{
  public UpdateCustomerProfileCommandValidator()
  {
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

    // Bireysel alanları (opsiyonel — ama doldurulursa format kontrolü)
    RuleFor(p => p.IdentityNumber)
        .Length(11).WithMessage("T.C. Kimlik No 11 haneli olmalıdır.")
        .Matches(@"^[0-9]+$").WithMessage("T.C. Kimlik No sadece rakamlardan oluşmalıdır.")
        .When(p => !string.IsNullOrEmpty(p.IdentityNumber));

    RuleFor(p => p.BirthDate)
        .LessThan(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Doğum tarihi bugünden ileri bir tarih olamaz.")
        .When(p => p.BirthDate.HasValue);

    // Ehliyet alanları (opsiyonel — ama doldurulursa format kontrolü)
    RuleFor(p => p.DriverLicenseIssueDate)
        .LessThan(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)))
            .WithMessage("Ehliyet veriliş tarihi bugünden ileri bir tarih olamaz.")
        .When(p => p.DriverLicenseIssueDate.HasValue);
  }
}

public sealed class UpdateCustomerProfileCommandHandler(
                        ICustomerRepository _customerRepo,
                        ICustomerContext _customerContext,
                        IUnitOfWork _unit
                    ) : IRequestHandler<UpdateCustomerProfileCommand, Result<string>>
{
  public async Task<Result<string>> Handle(UpdateCustomerProfileCommand _req, CancellationToken _token)
  {
    // 1. Giriş yapmış müşteriyi bul
    var customerId = _customerContext.GetCustomerId();
    if (customerId == Guid.Empty) return Result<string>.Failure("Giriş yapmış bir müşteri bulunamadı!");

    var customer = await _customerRepo.GetByExpressionAsync(c => c.Id == customerId, _token);
    if (customer is null) return Result<string>.Failure("Müşteri bulunamadı!");

    // 2. ⭐ T.C. Kimlik No değişiyorsa, benzersizlik kontrolü
    if (customer.Type == CustomerType.Individual
                            && !string.IsNullOrEmpty(_req.IdentityNumber)
                            && _req.IdentityNumber != customer.IdentityNumber)
    {
      var isIdentityExists = await _customerRepo.IsIdentityNumberExistsAsync(_req.IdentityNumber, _token);
      if (isIdentityExists)
        return Result<string>.Failure("Bu T.C. Kimlik No ile kayıtlı bir müşteri zaten var!");
    }

    // 3. ⭐ Telefon numarası değişiyorsa, benzersizlik kontrolü
    if (_req.PhoneNumber != customer.PhoneNumber)
    {
      var isPhoneExists = await _customerRepo.IsPhoneNumberExistsAsync(_req.PhoneNumber, _token);
      if (isPhoneExists)
        return Result<string>.Failure("Bu telefon numarası ile kayıtlı bir müşteri zaten var!");
    }

    // 4. ⭐ Vergi numarası değişiyorsa (Corporate), benzersizlik kontrolü
    if (customer.Type == CustomerType.Corporate &&
        !string.IsNullOrEmpty(_req.TaxNumber) &&
        _req.TaxNumber != customer.TaxNumber)
    {
      var isTaxExists = await _customerRepo.IsTaxNumberExistsAsync(_req.TaxNumber, _token);
      if (isTaxExists)
        return Result<string>.Failure("Bu vergi numarası ile kayıtlı bir müşteri zaten var!");
    }

    // 5. ⭐ Telefon değiştiyse, IsPhoneVerified'ı sıfırla (yeniden doğrulama gerek)
    bool phoneChanged = _req.PhoneNumber != customer.PhoneNumber;

    // 6. Domain metodunu çağır
    customer.UpdateProfile(
        firstName: _req.FirstName,
        lastName: _req.LastName,
        identityNumber: _req.IdentityNumber,
        birthDate: _req.BirthDate,
        gender: _req.Gender,
        companyName: _req.CompanyName,
        taxNumber: _req.TaxNumber,
        taxOffice: _req.TaxOffice,
        phoneNumber: _req.PhoneNumber,
        address: _req.Address,
        city: _req.City,
        country: _req.Country,
        postalCode: _req.PostalCode,
        driverLicenseNumber: _req.DriverLicenseNumber,
        driverLicenseIssueDate: _req.DriverLicenseIssueDate,
        driverLicenseClass: _req.DriverLicenseClass,
        updatedBy: customerId   // ⭐ Self-service, kendi ID'si
    );

    // 7. ⭐ Telefon değiştiyse doğrulamayı sıfırla
    if (phoneChanged)
    {
      customer.ResetPhoneVerification();  // ⚠️ Entity'ye eklenecek
    }

    _customerRepo.Update(customer);

    await _unit.SaveChangesAsync(_token);

    return Result<string>.Succeed("Profiliniz başarıyla güncellendi.");
  }
}

