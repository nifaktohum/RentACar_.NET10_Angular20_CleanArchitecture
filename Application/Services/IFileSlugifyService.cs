namespace Application.Services;

public interface IFileSlugifyService
{
  string Slugify(string value);
  string SlugifyWithDateTime(string value);
  string SlugifyWithGuid(string value);
}