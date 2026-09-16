using Application.Common.Paging;

namespace Application.Specifications;

public record VehicleSpecParams : BasePagedRequest
{
  public string? Sort { get; set; }

  private List<string> _brands = [];   // Gelen ham verileri arka planda güvenle saklar.
  public List<string> Brands      // dış dünyaya açılan kapı Vehicles property'si
  {
    // Biri benden veriyi okumak (get) istediğinde, kasadaki _vehicles listesini olduğu gibi dışarıya sunarım.
    get => _brands;
    // (API üzerinden URL'den) bana bir değer (value) atandığı an devreye girer.
    set => _brands = [.. value.SelectMany(x => x.Split(',', StringSplitOptions.RemoveEmptyEntries)).Select(item => item.Trim())];

    // Adım A (Gelen Veriyi Karşılama): value olarak dışarıdan şöyle bir metin geldiğini düşün: "Toyota, BMW , , Audi".
    // Adım B (Virgüllerden Bölme - Split): x.Split(',') diyerek bu metni virgüllerin geçtiği her yerden bıçak gibi keserim. Elimde artık şunlar kalır: ["Toyota", " BMW ", " ", " Audi"] (Arada boşluklar ve boş elemanlar kaldı).
    // Adım C (Çöpleri Temizleme - RemoveEmptyEntries): StringSplitOptions.RemoveEmptyEntries parametresi sayesinde ortalıkta kalan o boş string'leri ve çöpü anında çöpe atarım. Geriye sadece dolu metinler kalır: ["Toyota", " BMW "].
    // Adım D (Düzleştirme - SelectMany): Eğer dışarıdan birden fazla parametre kümesi array olarak gelirse, SelectMany bunları tek bir düz liste(flat list) haline getirir.
    // Adım E (Listeye Çevirme ve Kaydetme - ToList): Elde ettiğim bu tertemiz sonuçları.ToList() ile tam bir C# listesine çevirip, en baştaki gizli kasam olan _vehicles değişkenine gururla kaydederim.
  }

  // Renk filtreleri için
  private List<string> _colors = [];
  public List<string> Colors
  {
    get => _colors;
    set => _colors = [.. value.SelectMany(x => x.Split(',', StringSplitOptions.RemoveEmptyEntries)).Select(item => item.Trim())];
  }

  // Model filtreleri için
  private List<string> _models = [];
  public List<string> Models
  {
    get => _models;
    set => _models = [.. value.SelectMany(x => x.Split(',', StringSplitOptions.RemoveEmptyEntries)).Select(item => item.Trim())];
  }

  // Model filtreleri için
  private List<string> _types = [];
  public List<string> Types
  {
    get => _types;
    set => _types = value != null
        ? [.. value.SelectMany(x => x.Split(',', StringSplitOptions.RemoveEmptyEntries)).Select(item => item.Trim())]
        : [];
  }
  // Yakıt filtreleri için
  private List<string> _fuelTypes = [];
  public List<string> FuelTypes
  {
    get => _fuelTypes;
    set => _fuelTypes = [.. value.SelectMany(x => x.Split(',', StringSplitOptions.RemoveEmptyEntries)).Select(item => item.Trim())];
  }

  //  Vites filtresi için
  private List<string> _transmissions = [];
  public List<string> Transmissions
  {
    get => _transmissions;
    set => _transmissions = [.. value.SelectMany(x => x.Split(',', StringSplitOptions.RemoveEmptyEntries)).Select(item => item.Trim())];
  }

  //  Yıl filtresi için
  private List<string> _years = [];
  public List<string> Years
  {
    get => _years;
    set => _years = [.. value.SelectMany(x => x.Split(',', StringSplitOptions.RemoveEmptyEntries)).Select(item => item.Trim())];
  }

  // ============ ⭐ TARİH FİLTRELERİ ============
  private DateTime? _startDate;
  public DateTime? StartDate
  {
    get => _startDate;
    set => _startDate = value != null && DateTime.TryParse(value.ToString(), out var result)
        ? DateTime.SpecifyKind(result, DateTimeKind.Utc)  // ⬅️ UTC'ye çevir!
        : null;
  }

  // Stok filtresi (VehicleModel için)
  private bool? _isInStock;
  public bool? IsInStock
  {
    get => _isInStock;
    set => _isInStock = value;
  }

  private DateTime? _endDate;
  public DateTime? EndDate
  {
    get => _endDate;
    set => _endDate = value != null && DateTime.TryParse(value.ToString(), out var result)
        ? DateTime.SpecifyKind(result, DateTimeKind.Utc)  // ⬅️ UTC'ye çevir!
        : null;
  }

  //  Mİnimun Fiyat filtresi için
  private decimal? _minPrice;
  public decimal? MinPrice
  {
    get => _minPrice;

    set => _minPrice = value != null && decimal.TryParse(value.ToString(), out var result) ? result : null;
  }

  //  Mİnimun Fiyat filtresi için
  private decimal? _maxPrice;
  public decimal? MaxPrice
  {
    get => _maxPrice;
    set => _maxPrice = value != null && decimal.TryParse(value.ToString(), out var result) ? result : null;
  }

  // Koltuk sayısı filtresi için
  private int? _seatCounts;
  public int? SeatCounts
  {
    get => _seatCounts;
    set => _seatCounts = value != null && int.TryParse(value.ToString(), out var result) ? result : null;
  }

  // Koltuk sayısı filtresi için
  private int? _doorCounts;
  public int? DoorCounts
  {
    get => _doorCounts;
    set => _doorCounts = value != null && int.TryParse(value.ToString(), out var result) ? result : null;
  }

  // ============ GENEL ARAMA FİLTRESİ (TEKİL) ============
  private string? _searchTerm;
  public string? SearchTerm
  {
    get => _searchTerm;
    set => _searchTerm = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
  }

  // ============ MÜSAİTLİK DURUMU FİLTRESİ ============
  private bool? _isAvailable;
  public bool? IsAvailable
  {
    get => _isAvailable;
    set => _isAvailable = value != null && bool.TryParse(value.ToString(), out var result) ? result : null;
  }

  // ⭐ ============ AKTİFLİK DURUMU FİLTRESİ ============
  private bool? _isActive;
  public bool? IsActive
  {
    get => _isActive;
    set => _isActive = value != null && bool.TryParse(value.ToString(), out var result) ? result : null;
  }

  // VehicleTypeId filtreleri (çoklu) - VehicleModel için eklendi
  private List<Guid> _vehicleTypeIds = [];
  public List<Guid> VehicleTypeIds
  {
    get => _vehicleTypeIds;
    set => _vehicleTypeIds = value ?? [];
  }

  // Minimum stok (VehicleModel için)
  private int? _minStock;
  public int? MinStock
  {
    get => _minStock;
    set => _minStock = value;
  }

  // Maksimum stok (VehicleModel için)
  private int? _maxStock;
  public int? MaxStock
  {
    get => _maxStock;
    set => _maxStock = value;
  }


}

/*[ .. (işlem) ]

  Sonundaki .ToList() metodunu uçurduk.
  Yerine [ .. (işlem) ] şeklinde olan Spread Operator yapısını kullandık. 
      Bu ifade, LINQ'dan gelen akışı doğrudan yeni bir List<string> içine toplar; 
      hem daha okunabilirdir hem de modern C# tavsiyesidir (IDE0305 uyarısını anında yok eder).
*/

/* set => _minPrice = value != null && decimal.TryParse(value.ToString(), out var result) ? result : null;

  // string üzerinden gelen değeri kontrol edip güvenli bir şekilde ondalıklı sayıya (decimal?) çevir  
            - Eğer gelen metin gerçekten geçerli bir sayısal ifadeyse (örneğin "150"), dönüşüm başarılı olur, sayıyı result değişkenine atar ve ifade true döner.
            - Eğer gelen metin sayıya çevrilemeyecek bir şeyse (örneğin "abc"), hata fırlatmak yerine sessizce false döner ve programın çökmesini engeller.

  dışarıdan gelen verinin güvenli bir şekilde sayıya çevrilmesini, olası hataların engellenmesini ve geçersiz durumlarda değerin null kalmasını sağlar.          
*/
