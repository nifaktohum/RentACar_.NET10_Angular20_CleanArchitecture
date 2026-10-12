using Application.Common.Extensions;
using Application.Common.Helpers.Filters;
using Application.Common.Helpers.Sorting;
using Application.Specifications;
using Domain.Entities.Customers;
using Domain.Specifications;

namespace Application.Features.Customers.Specifications;

public sealed class CustomerWithFiltersSpecification : BaseSpecification<Customer>
{
  public CustomerWithFiltersSpecification(CustomerSpecParams specParams)
      : base(CustomerFilterExtensions.BuildFilter(specParams, includeAll: true)) // includeAll: true ==> Tüm kullanıcıları getir. IsActive = false
  {
    // ⭐ Sıralama
    this.ApplyCustomerSorting(specParams.Sort);

    // ⭐ Paging — SpecParams null olsa bile default params ile çalışır
    this.ApplyPaging(specParams ?? new CustomerSpecParams());
  }
}
