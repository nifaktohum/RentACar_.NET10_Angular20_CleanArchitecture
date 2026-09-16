namespace Application.Common.Models;

public sealed record PagedResponse<T>(
                          IReadOnlyList<T> Items,
                          int TotalCount,
                          int PageNumber,
                          int PageSize,
                          int TotalPages
                      )
{
  // Kullanıcı şu an 1. sayfadaysa, daha öncesi olmadığı için bu değer false
  public bool HasPreviousPage => PageNumber > 1;      // Önceki Sayfa Var mı?
  // Kullanıcı örneğin toplamda 3 sayfa olan bir listede son sayfada (3. sayfa) oturuyorsa, daha ileri gidemeyeceği için bu değer false döner.
  public bool HasNextPage => PageNumber < TotalPages; // Sonraki Sayfa Var mı?

  public PagedResponse(
              IReadOnlyList<T> items,
              int totalCount,
              int pageNumber,
              int pageSize): this(items,
                                  totalCount,
                                  pageNumber,
                                  pageSize,
                                  (int)Math.Ceiling(totalCount / (double)pageSize))
  {
  }
}                      