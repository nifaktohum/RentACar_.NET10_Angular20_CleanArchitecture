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
  public static IQueryable<T> ApplyIncludes<T>(
      this IQueryable<T> query,
      BaseSpecification<T> spec) where T : class
  {
    foreach (var include in spec.Includes)
    {
      query = query.Include(include);
    }
    return query;
  }

}