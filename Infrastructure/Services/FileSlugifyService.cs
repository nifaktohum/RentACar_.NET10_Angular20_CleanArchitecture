using Application.Services;
using Infrastructure.Extensions;

namespace Infrastructure.Services;

// Açıklama => Dosya yükleme (File Upload) işlemlerinde, dosyaların adındaki bozuk karakterleri temizleyip sonuna benzersiz bir imza ekleyerek güvenli ve düzenli dosya isimleri üretmek için tasarlanmış bir servis!

public sealed class FileSlugifyService : IFileSlugifyService
{
  // : Girilen metni doğrudan standart bir slug formatına çevirir
  public string Slugify(string value)
  {
    return value.Slugify();
  }

  // Metni slug'a çevirir ve sonuna YYYYYMMDD_HHmmss formatında güncel zaman damgası ekler. 
  // Eğer metin boş gelirse unknown_tarih şeklinde yedek bir isim üretir. Özellikle aynı isimli dosyaların üst üste yazılmasını (overwrite) önlemek için harikadır.
  public string SlugifyWithDateTime(string value)
  {
    var slug = value.Slugify();
    var dateTime = DateTime.Now.ToString("yyyyMMdd_HHmmss");
    return string.IsNullOrEmpty(slug) ? $"unknown_{dateTime}" : $"{slug}_{dateTime}";
  }

  // Metni slug'a çevirir ve sonuna benzersiz bir Guid'in ilk 8 karakterini ekler. 
  // Tarih yerine rastgele/benzersiz bir ek istendiğinde kullanılır.
  public string SlugifyWithGuid(string value)
  {
    var slug = value.Slugify();
    var guid = Guid.NewGuid().ToString("N")[..8];
    return string.IsNullOrEmpty(slug) ? $"unknown_{guid}" : $"{slug}_{guid}";
  }
}
