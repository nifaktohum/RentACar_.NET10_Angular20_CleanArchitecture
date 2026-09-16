using System.Linq.Expressions;

namespace Domain.Repositories;

public interface ISpecification<T>
{
  //Bu arayüzü implement eden sınıflar belirli bir veriyi nasıl getireceğini tanımlar 
  // ama bu işlem veri erişim katmanında (repository) uygulanır.
  Expression<Func<T, bool>>? Criteria { get; } // filtreleme 
  Expression<Func<T, object>>? OrderBy { get; } // artan sıralama
  Expression<Func<T, object>>? OrderByDescending { get; } //azalan sıralama
  // List<Expression<Func<T, object>>> Includes { get; } //
  // List<string> IncludeStrings { get; } //

  // veritabanından veri çekerken tekrar eden (mükerrer) kayıtların elemine edilmesini sağlamaktır.
  bool IsDistinct { get; }
  int Skip { get; } // Kaç kayıt atlanacağını belirtir
  int Take { get; } // Kaç kayıt alınacağını belirtir
  bool IsPagingEnabled { get; } // Sayfalama aktif mi?
  // IQueryable<T> ApplyCriteria(IQueryable<T> query);

}

// Tablodan sadece ihtiyacın olan alanları seç
// "Bu veriyi getir ama sadece şu alanlarını seçerek (projeksiyon yaparak) getir" dememizi sağlar.
public interface ISpecification<T, TResult> : ISpecification<T>
{
  Expression<Func<T, TResult>>? Select { get; }
}