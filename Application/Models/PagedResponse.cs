namespace Application.Models;

public sealed record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages
)
{
  public bool HasPreviousPage => PageNumber > 1;
  public bool HasNextPage => PageNumber < TotalPages;

  public PagedResponse(IReadOnlyList<T> items, int totalCount, int pageNumber, int pageSize)
      : this(items, totalCount, pageNumber, pageSize, (int)Math.Ceiling(totalCount / (double)pageSize))
  {
  }
}