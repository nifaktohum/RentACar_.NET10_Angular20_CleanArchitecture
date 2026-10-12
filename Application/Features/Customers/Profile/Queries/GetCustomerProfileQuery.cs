using Application.Features.Customers.Profile.Dto;
using Application.Services;
using Domain.Repositories;
using MediatR;
using TS.Result;

namespace Application.Features.Customers.Profile.Queries;

public sealed record GetCustomerProfileQuery() : IRequest<Result<CustomerProfileDto>>;

public sealed class GetCustomerProfileQueryHandler(
                        ICustomerRepository _customerRepo,
                        ICustomerContext _customerContext
                    ) : IRequestHandler<GetCustomerProfileQuery, Result<CustomerProfileDto>>
{
  public async Task<Result<CustomerProfileDto>> Handle(GetCustomerProfileQuery _req, CancellationToken _token)
  {
    // 1. Giriş yapmış müşterinin ID'sini al
    var customerId = _customerContext.GetCustomerId();
    if (customerId == Guid.Empty) return Result<CustomerProfileDto>.Failure("Giriş yapmış bir müşteri bulunamadı!");

    // 2. Müşteriyi getir
    var customer = await _customerRepo.GetByExpressionAsync(c => c.Id == customerId, _token);
    if (customer is null) return Result<CustomerProfileDto>.Failure("Müşteri bulunamadı!");

    // 3. DTO'ya dönüştür
    var dto = new CustomerProfileDto(
        Id: customer.Id,
        Type: customer.Type.ToString(),
        Email: customer.Email,
        PhoneNumber: customer.PhoneNumber,
        Address: customer.Address,
        City: customer.City,
        Country: customer.Country,
        PostalCode: customer.PostalCode,

        // Bireysel
        FirstName: customer.FirstName,
        LastName: customer.LastName,
        IdentityNumber: customer.IdentityNumber,
        BirthDate: customer.BirthDate,
        Gender: customer.Gender,
        Age: customer.GetAge(),

        // Kurumsal
        CompanyName: customer.CompanyName,
        TaxNumber: customer.TaxNumber,
        TaxOffice: customer.TaxOffice,

        // Ehliyet
        DriverLicenseNumber: customer.DriverLicenseNumber,
        DriverLicenseIssueDate: customer.DriverLicenseIssueDate,
        DriverLicenseClass: customer.DriverLicenseClass,
        HasValidDriverLicense: customer.HasValidDriverLicense(),

        // Finansal
        FindeksScore: customer.FindeksScore,

        // Durum
        IsEmailVerified: customer.IsEmailVerified,
        IsPhoneVerified: customer.IsPhoneVerified,
        IsBlacklisted: customer.IsBlacklisted,
        BlacklistReason: customer.BlacklistReason,

        // İş kuralları (Angular guard için faydalı)
        CanRent: customer.CanRent(),
        CanRentLuxury: customer.CanRentLuxury(),

        // Audit
        CreatedAt: customer.CreatedAt,
        UpdatedAt: customer.UpdatedAt ?? customer.CreatedAt
    );

    return Result<CustomerProfileDto>.Succeed(dto);
  }
}