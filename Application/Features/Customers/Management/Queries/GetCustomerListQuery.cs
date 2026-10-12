using Application.Behaviors;
using Application.Common.Models;
using Application.Features.Customers.Management.Dto;
using Application.Features.Customers.Specifications;
using Application.Specifications;
using Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TS.Result;

namespace Application.Features.Customers.Management.Queries;

[Permission("Customers.Read")]
public sealed record GetCustomerListQuery(CustomerSpecParams SpecParams): IRequest<Result<PagedResponse<CustomerListDto>>>;

public sealed class GetCustomerListQueryHandler(
                        ICustomerRepository _customerRepo
                    ) : IRequestHandler<GetCustomerListQuery, Result<PagedResponse<CustomerListDto>>>
{
  public async Task<Result<PagedResponse<CustomerListDto>>> Handle(GetCustomerListQuery _req, CancellationToken _token)
  {
    // 1. Spec oluştur (filtre + sıralama + paging)
    var spec = new CustomerWithFiltersSpecification(_req.SpecParams);

    // 2. Toplam sayı (paging'siz — repository kendi içinde paging'i uygulamaz)
    var totalCount = await _customerRepo.CountWithSpecAsync(spec, _token);

    // 3. Filtreli + sıralı + sayfalı liste
    var customers = await _customerRepo.GetListWithSpecAsync(spec, _token);

    // 4. DTO'ya dönüştür
    var items = customers.Select(c => new CustomerListDto(
        Id: c.Id,
        Type: c.Type.ToString(),
        Email: c.Email,
        PhoneNumber: c.PhoneNumber,
        FirstName: c.FirstName,
        LastName: c.LastName,
        CompanyName: c.CompanyName,
        City: c.City,
        Country: c.Country,
        IsEmailVerified: c.IsEmailVerified,
        IsPhoneVerified: c.IsPhoneVerified,
        IsBlacklisted: c.IsBlacklisted,
        FindeksScore: c.FindeksScore,
        IsActive: c.IsActive,
        CreatedAt: c.CreatedAt,
        UpdatedAt: c.UpdatedAt
    )).ToList();

    // 5. Paged response
    var response = new PagedResponse<CustomerListDto>(
        items,
        totalCount,
        _req.SpecParams.PageNumber,
        _req.SpecParams.PageSize
    );

    return Result<PagedResponse<CustomerListDto>>.Succeed(response);
  }
}