using Domain.Abstractions;

namespace Domain.Repositories;


public interface IGenericRepository<T> where T : BaseEntity
{
  /// Belirtilen ID değerine sahip varlığı asenkron olarak getirir. Bulunamazsa null döner.
  Task<T?> GetByIdAsync(int id);

  /// Varlık türüne ait tüm kayıtları salt okunur bir liste olarak getirir.
  Task<IReadOnlyList<T>> ListAllAsync();

  /// Verilen Specification (şart/kriter) nesnesine uygun tek bir varlığı asenkron olarak getirir.
  Task<T?> GetEntityWithSpec(ISpecification<T> _spec);

  /// Verilen Specification nesnesindeki filtre ve kurallara göre eşleşen varlıkları liste olarak getirir.
  Task<IReadOnlyList<T>> ListAsync(ISpecification<T> _spec);

  /// Specification kullanarak veriyi filtreler ve sonucu farklı bir tipte (TResult - örn. DTO) projelendirerek tek bir kayıt döner.
  Task<TResult?> GetEntityWithSpec<TResult>(ISpecification<T, TResult> _spec);

  /// Specification kullanarak verileri filtreler ve sonuçları farklı bir tipte (TResult - örn. DTO listesi) projelendirerek liste halinde döner.
  Task<IReadOnlyList<TResult>> ListAsync<TResult>(ISpecification<T, TResult> _spec);
}
