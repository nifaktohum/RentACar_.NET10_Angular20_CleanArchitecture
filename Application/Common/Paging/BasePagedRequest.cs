namespace Application.Common.Paging;

public abstract record BasePagedRequest
{
  private int _pageNumber = 1;
  private int _pageSize = 3;
  private const int MaxPageSize = 50;

  public int PageNumber
  {
    get => _pageNumber;
    init => _pageNumber = value < 1 ? 1 : value;
  }

  public int PageSize
  {
    get => _pageSize;
    init => _pageSize = value < 1 ? 3 : value > MaxPageSize ? MaxPageSize : value;
  }

  public int Skip => (PageNumber - 1) * PageSize;
}
