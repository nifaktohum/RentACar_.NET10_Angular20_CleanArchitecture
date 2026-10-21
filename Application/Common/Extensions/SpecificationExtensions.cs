using Application.Common.Paging;
using Domain.Specifications;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Extensions;

public static class SpecificationExtensions
{
  public static void ApplyPaging<T>(this BaseSpecification<T> spec, BasePagedRequest request) where T : class
  {
    spec.ApplyPaging(request.Skip, request.PageSize);
  }

  public static IQueryable<T> ApplyOrdering<T>(this IQueryable<T> query, BaseSpecification<T> spec) where T : class
  {
    if (spec.OrderBy != null)
    {
      var orderedQuery = query.OrderBy(spec.OrderBy);

      if (spec.ThenBy != null)
        orderedQuery = orderedQuery.ThenBy(spec.ThenBy);
      else if (spec.ThenByDescending != null)
        orderedQuery = orderedQuery.ThenByDescending(spec.ThenByDescending);

      return orderedQuery;
    }

    if (spec.OrderByDescending != null)
    {
      var orderedQuery = query.OrderByDescending(spec.OrderByDescending);

      if (spec.ThenBy != null)
        orderedQuery = orderedQuery.ThenBy(spec.ThenBy);
      else if (spec.ThenByDescending != null)
        orderedQuery = orderedQuery.ThenByDescending(spec.ThenByDescending);

      return orderedQuery;
    }

    return query;
  }

  // ✅ Include'ler için extension
  public static IQueryable<T> ApplyIncludes<T>(this IQueryable<T> query, BaseSpecification<T> spec) where T : class
  {
    foreach (var include in spec.Includes)
    {
      query = query.Include(include);
    }

    // ⭐ String include'lar (ThenInclude için)
    foreach (var include in spec.IncludeStrings)
    {
      query = query.Include(include);
    }
    return query;
  }

  // ⭐ Specification'dan IQueryable oluştur
  public static IQueryable<T> ApplySpecification<T>(this IQueryable<T> query, BaseSpecification<T> spec) where T : class
  {
    // 1. Criteria (WHERE): Şartnamede belirtilen LINQ koşulunu (Örn: x => x.Status == "Active") sorguya ekler.
    if (spec.Criteria != null)
      query = query.Where(spec.Criteria);

    // 2. Includes: İlişkili tabloları (Customer, Vehicle, Branches vb.) eager loading ile sorguya dahil eder.
    query = query.ApplyIncludes(spec);

    // 3. Ordering: Sıralama kurallarını (OrderBy / OrderByDescending) sorguya işler.
    query = query.ApplyOrdering(spec);

    // 4. Distinct: Eğer tekrarlayan kayıtları engellemek istiyorsak sorguya Distinct atar.
    if (spec.IsDistinct)
      query = query.Distinct();

    // 5. Paging: Sayfalama aktifse (Sayfa başı X kayıt, Y kadar atla) Skip ve Take uygular.
    if (spec.IsPagingEnabled)
      query = query.Skip(spec.Skip).Take(spec.Take);

    return query;
  }

  // ⭐ Liste getir
  public static async Task<List<T>> GetAsync<T>(this IQueryable<T> query, BaseSpecification<T> spec, CancellationToken cancellationToken = default) where T : class
  {
    return await query.ApplySpecification(spec).ToListAsync(cancellationToken);
  }

  // ⭐ İlk kaydı getir
  public static async Task<T?> GetFirstAsync<T>(this IQueryable<T> query, BaseSpecification<T> spec, CancellationToken cancellationToken = default) where T : class
  {
    return await query.ApplySpecification(spec).FirstOrDefaultAsync(cancellationToken);
  }

  // ⭐ Sayı
  public static async Task<int> CountAsync<T>(this IQueryable<T> query, BaseSpecification<T> spec, CancellationToken cancellationToken = default) where T : class
  {
    var q = query.AsQueryable();
    if (spec.Criteria != null)
      q = q.Where(spec.Criteria);

    return await q.CountAsync(cancellationToken);
  }

}