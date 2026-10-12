using Application.Common.Extensions;
using Domain.Entities.Customers;
using Domain.Repositories;
using Domain.Specifications;
using GenericRepository;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class CustomerRepository : Repository<Customer, AppDbContext>, ICustomerRepository
{
  private AppDbContext _context;
  public CustomerRepository(AppDbContext context) : base(context)
  {
    _context = context;
  }

  public async Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
  {
    return await GetAllWithTracking().FirstOrDefaultAsync(c => c.Email == email, cancellationToken);
  }

  public async Task<Customer?> GetByIdentityNumberAsync(string identityNumber, CancellationToken cancellationToken = default)
  {
    return await GetAll().FirstOrDefaultAsync(c => c.IdentityNumber == identityNumber, cancellationToken);
  }

  public async Task<Customer?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default)
  {
    return await GetAll().FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber, cancellationToken);
  }

  public async Task<Customer?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
  {
    return await GetAll().FirstOrDefaultAsync(c => c.RefreshToken == refreshToken, cancellationToken);
  }

  public async Task<Customer?> GetByTaxNumberAsync(string taxNumber, CancellationToken cancellationToken = default)
  {
    return await GetAll().FirstOrDefaultAsync(c => c.TaxNumber == taxNumber, cancellationToken);
  }

  public async Task<bool> IsEmailExistsAsync(string email, CancellationToken cancellationToken = default)
  {
    return await AnyAsync(c => c.Email == email, cancellationToken);
  }

  public async Task<bool> IsIdentityNumberExistsAsync(string identityNumber, CancellationToken cancellationToken = default)
  {
    return await AnyAsync(c => c.IdentityNumber == identityNumber, cancellationToken);
  }

  public async Task<bool> IsPhoneNumberExistsAsync(string phoneNumber, CancellationToken cancellationToken = default)
  {
    return await AnyAsync(c => c.PhoneNumber == phoneNumber, cancellationToken);
  }

  public async Task<bool> IsTaxNumberExistsAsync(string taxNumber, CancellationToken cancellationToken = default)
  {
    return await AnyAsync(c => c.TaxNumber == taxNumber, cancellationToken);
  }

  public async Task<string?> GetSecurityStampByIdAsync(Guid customerId, CancellationToken cancellationToken = default)
  {
    return await _context.Customers
        .Where(c => c.Id == customerId)
        .Select(c => c.SecurityStamp)
        .FirstOrDefaultAsync(cancellationToken);
  }

  // ==================== ⭐ SPECIFICATION METOTLARI ====================

  public async Task<List<Customer>> GetListWithSpecAsync(ISpecification<Customer> spec, CancellationToken cancellationToken = default)
  {
    var query = _context.Customers.AsQueryable();

    // 1. Criteria (WHERE)
    if (spec.Criteria != null)
      query = query.Where(spec.Criteria);

    // 2. Includes
    query = query.ApplyIncludes((BaseSpecification<Customer>)spec);

    // 3. Ordering
    query = query.ApplyOrdering((BaseSpecification<Customer>)spec);

    // 4. Distinct
    if (spec.IsDistinct)
      query = query.Distinct();

    // 5. Paging
    if (spec.IsPagingEnabled)
      query = query.Skip(spec.Skip).Take(spec.Take);

    return await query.AsNoTracking().ToListAsync(cancellationToken);
  }

  public async Task<int> CountWithSpecAsync(ISpecification<Customer> spec, CancellationToken cancellationToken = default)
  {
    var query = _context.Customers.AsQueryable();

    if (spec.Criteria != null)
      query = query.Where(spec.Criteria);

    // Distinct
    if (spec.IsDistinct)
      query = query.Distinct();

    return await query.CountAsync(cancellationToken);
  }

  //  include gerektiğinde kullanılır.
  public async Task<Customer?> GetSingleWithSpecAsync(ISpecification<Customer> spec, CancellationToken cancellationToken = default)
  {
    var query = _context.Customers.AsQueryable();

    if (spec.Criteria != null)
      query = query.Where(spec.Criteria);

    query = query.ApplyIncludes((BaseSpecification<Customer>)spec);

    return await query.AsNoTracking().FirstOrDefaultAsync(cancellationToken);
  }

}
