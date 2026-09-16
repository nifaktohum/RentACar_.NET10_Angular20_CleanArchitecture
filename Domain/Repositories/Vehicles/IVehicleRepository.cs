using System.Linq.Expressions;
using Domain.Entities.Vehicles;
using GenericRepository;

namespace Domain.Repositories.Vehicles;

public interface IVehicleRepository : IRepository<Vehicle>
{
  // Belirtilen ID'ye sahip aracı, ilişkili görselleriyle (Images) birlikte veritabanından getirir.
  Task<Vehicle?> GetVehicleWithImagesAsync(Guid id, CancellationToken cancellationToken = default);

  // Belirtilen ID'ye sahip aracı; türü ve görselleri gibi tüm detay ilişkileriyle birlikte getirir.
  Task<Vehicle?> GetVehicleWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);

  // Plakanın sistemde daha önce kayıtlı olup olmadığını kontrol eder. Güncelleme işlemlerinde mevcut aracın ID'si excludeId ile hariç tutulabilir.
  Task<bool> IsPlateUniqueAsync(string plate, Guid? excludeId = null, CancellationToken cancellationToken = default);

  // Belirli bir ID'ye sahip araçtan istenen miktarda (quantity) stokta bulunup bulunmadığını kontrol eder.
  Task<bool> HasStockAsync(Guid id, int quantity = 1, CancellationToken cancellationToken = default);


  // Araç kiralandığında veya stok düştüğünde, belirtilen miktarda stok miktarını azaltır ve başarılı olup olmadığını döner.
  Task<bool> DecreaseStockAsync(Guid id, int quantity = 1, CancellationToken cancellationToken = default);

  // İptal veya iade durumlarında, belirtilen miktarda araç stok miktarını artırır.
  Task<bool> IncreaseStockAsync(Guid id, int quantity = 1, CancellationToken cancellationToken = default);


  // Dinamik filtreleme, sıralama (sorting) ve sayfalama (pagination - skip/take) kriterlerine göre araç listesini getirir.
  Task<List<Vehicle>> GetFilteredAsync(
      Expression<Func<Vehicle, bool>>? filter = null,
      string? sortBy = null,
      bool descending = false,
      int? skip = null,
      int? take = null,
      CancellationToken cancellationToken = default
  );

  // Verilen filtre kriterlerine uyan toplam araç sayısını (sayfalama yapmadan önce) döner.
  Task<int> GetFilteredCountAsync(
      Expression<Func<Vehicle, bool>>? filter = null,
      CancellationToken cancellationToken = default
  );
}
