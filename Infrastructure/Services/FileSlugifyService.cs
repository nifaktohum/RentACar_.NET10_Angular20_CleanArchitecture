using Application.Services;
using Infrastructure.Extensions;

namespace Infrastructure.Services;

public sealed class FileSlugifyService : IFileSlugifyService
{
  public string Slugify(string value)
  {
    return value.Slugify();
  }

  public string SlugifyWithDateTime(string value)
  {
    var slug = value.Slugify();
    var dateTime = DateTime.Now.ToString("yyyyMMdd_HHmmss");
    return string.IsNullOrEmpty(slug) ? $"unknown_{dateTime}" : $"{slug}_{dateTime}";
  }

  public string SlugifyWithGuid(string value)
  {
    var slug = value.Slugify();
    var guid = Guid.NewGuid().ToString("N")[..8];
    return string.IsNullOrEmpty(slug) ? $"unknown_{guid}" : $"{slug}_{guid}";
  }
}
