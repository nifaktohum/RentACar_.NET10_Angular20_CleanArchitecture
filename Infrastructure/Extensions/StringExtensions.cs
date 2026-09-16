namespace Infrastructure.Extensions;

public static class StringExtensions
{
  public static string Slugify(this string text)
  {
    if (string.IsNullOrWhiteSpace(text)) return string.Empty;

    return text.ToLowerInvariant()
        .Replace("ç", "c").Replace("ğ", "g").Replace("ı", "i")
        .Replace("ö", "o").Replace("ş", "s").Replace("ü", "u")
        .Replace("İ", "i").Replace("Ğ", "g").Replace("Ü", "u")
        .Replace("Ş", "s").Replace("Ö", "o").Replace("Ç", "c")
        .Replace(" ", "-")
        .Replace(".", "")
        .Replace(",", "");
  }
}