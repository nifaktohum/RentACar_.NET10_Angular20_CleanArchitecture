using Application.Behaviors;
using Application.Features.Customers.Management.Dto;
using Domain.Repositories;
using MediatR;
using TS.Result;

namespace Application.Features.Customers.Management.Queries;

[Permission("Customers.Read")]
public sealed record GetCustomerByIdQuery(Guid Id) : IRequest<Result<CustomerDetailDto>>;

public sealed class GetCustomerByIdQueryHandler(
                        ICustomerRepository _customerRepo
                    ) : IRequestHandler<GetCustomerByIdQuery, Result<CustomerDetailDto>>
{
  public async Task<Result<CustomerDetailDto>> Handle(GetCustomerByIdQuery _req, CancellationToken _token)
  {
    // 1. Müşteriyi bul
    var customer = await _customerRepo.GetByExpressionAsync(c => c.Id == _req.Id, _token);
    if (customer is null) return Result<CustomerDetailDto>.Failure("Müşteri bulunamadı!");

    // 2. DTO'ya dönüştür
    var dto = new CustomerDetailDto(
        // Kimlik
        Id: customer.Id,
        Type: customer.Type.ToString(),

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

        // İletişim
        Email: customer.Email,
        PhoneNumber: customer.PhoneNumber,
        Address: customer.Address,
        City: customer.City,
        Country: customer.Country,
        PostalCode: customer.PostalCode,

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
        IsActive: customer.IsActive,

        // İş kuralları
        CanRent: customer.CanRent(),
        CanRentLuxury: customer.CanRentLuxury(),

        // Audit
        CreatedAt: customer.CreatedAt,
        UpdatedAt: customer.UpdatedAt
    );

    return Result<CustomerDetailDto>.Succeed(dto);

    throw new NotImplementedException();
  }
}
