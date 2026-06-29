using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Models;

public class Pagination<T> where T : class
{
  // C# tarafında nesne üretilirken verileri zorunlu olarak içeri almak için yapıcı metot (Constructor) kullanıyoruz.
  public Pagination(int pageSize, int pageIndex, long count, IReadOnlyList<T> data)
  {
    PageSize = pageSize;
    PageIndex = pageIndex;
    Count = count;
    Data = data;
  }

  // Sayfa başına kaç adet kayıt gösterileceğini tutar (Örn: 10, 20, 50).
  public int PageSize { get; set; }

  // Kullanıcının şu an kaçıncı sayfada olduğunu tutar (Örn: 1. sayfa, 2. sayfa).
  public int PageIndex { get; set; }

  // Veritabanında filtrelere uyan TOPLAM kayıt sayısını tutar (Sayfa limitinden bağımsız, tüm kayıtların adedi).
  public long Count { get; set; }

  // Sadece o sayfaya ait olan, veritabanından çekilmiş asıl veri listesini tutar (Örn: 10 tane araç nesnesi).
  public IReadOnlyList<T> Data { get; set; }

  // Toplam kaç sayfa oluştuğunu dinamik olarak hesaplayan akıllı salt-okunur (Readonly) özellik.
  // Toplam kayıt sayısını, sayfa boyutuna bölüp yukarı yuvarlar (Örn: 25 kayıt / 10'arlı sayfa = 3 sayfa yapar).
  public int TotalPages => (int)Math.Ceiling(Count / (double)PageSize);

  // Ön yüzde "Önceki Sayfa" butonunun aktif mi pasif mi olacağını belirlemek için kullanılır.
  // Eğer mevcut sayfa 1'den büyükse, geriye dönebileceği bir sayfa var demektir (true döner).
  public bool HasPreviousPage => PageIndex > 1;

  // Ön yüzde "Sonraki Sayfa" butonunun aktif mi pasif mi olacağını belirlemek için kullanılır.
  // Eğer mevcut sayfa, toplam sayfa sayısından küçükse ileriye gidebilir demektir (true döner).
  public bool HasNextPage => PageIndex < TotalPages;
}
