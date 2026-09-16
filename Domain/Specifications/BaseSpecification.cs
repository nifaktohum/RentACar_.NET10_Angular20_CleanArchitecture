using System.Linq.Expressions;
using Domain.Repositories;

namespace Domain.Specifications;

public class BaseSpecification<T>(Expression<Func<T, bool>>? _criteria) : ISpecification<T>
{
  // Bazen senden verileri filtrelemeni (WHERE şartı) istemeyeceğim, sadece tablodaki verileri getir.
  public BaseSpecification() : this(null) { }

  // Bu sınıf, bir kriter alıp onu saklayan ve gerektiğinde dışarıya sunan temel bir spesifikasyon sınıfı.
  public Expression<Func<T, bool>>? Criteria => _criteria;
  public List<Expression<Func<T, object>>> Includes { get; } = new();

  public Expression<Func<T, object>>? OrderBy { get; private set; }
  public Expression<Func<T, object>>? OrderByDescending { get; private set; }

  // ikincil ve üçüncül sıralama (ThenBy / ThenByDescending) işlemleri
  public Expression<Func<T, object>>? ThenBy { get; private set; }
  public Expression<Func<T, object>>? ThenByDescending { get; private set; }

  public bool IsDistinct { get; private set; }

  // SAYFALAMA PROPERTY'LERİ
  public int Skip { get; private set; }
  public int Take { get; private set; }
  public bool IsPagingEnabled { get; private set; }

  // Sayfalama metodu
  public void ApplyPaging(int skip, int take)
  {
    Skip = skip;
    Take = take;
    IsPagingEnabled = true;
  }




  // ============================================================ // 
  public void AddInclude(Expression<Func<T, object>> includeExpression)
  {
    Includes.Add(includeExpression);
  }

  public void ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
  {
    OrderBy = orderByExpression;
  }

  public void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescExpression)
  {
    OrderByDescending = orderByDescExpression;
  }


  public void ApplyThenBy(Expression<Func<T, object>> thenByExpression)
  {
    ThenBy = thenByExpression;
  }

  public void ApplyThenByDescending(Expression<Func<T, object>> thenByDescExpression)
  {
    ThenByDescending = thenByDescExpression;
  }

  public void ApplyDistinct()
  {
    IsDistinct = true;
  }
 }

public class BaseSpecification<T, TResult>(Expression<Func<T, bool>> criteria) : BaseSpecification<T>(criteria), ISpecification<T, TResult>
{
  public Expression<Func<T, TResult>>? Select { get; private set; }

  protected void AddSelect(Expression<Func<T, TResult>> selectExpression)
  {
    Select = selectExpression;
  }
}

/*  ThenBy / ThenByDescending

    ==> İlk sıralama (OrderBy veya OrderByDescending) yapıldıktan sonra, 
    aynı değere sahip verilerin kendi arasında hangi kritere göre sıralanmaya devam edeceğini tutan 
    linq expression yapılarını saklar.

    ==> ApplyThenBy Metodu: 
            Dışarıdan gelen artan (Ascending) ikincil sıralama kuralını (thenByExpression) yakalayıp 
            ThenBy property'sine atar. Örneğin: "Önce markaya göre sırala, 
            markalar aynıysa isme göre A'dan Z'ye (ThenBy) sırala" senaryosunda kullanılır.

    ==> ApplyThenByDescending Metodu: 
            Dışarıdan gelen azalan (Descending) ikincil sıralama kuralını yakalayıp ThenByDescending property'sine atar. 
            Örneğin: "Önce fiyata göre sırala, fiyatlar aynıysa en yeni tarihe (ThenByDescending) göre sırala" 
            durumunda devreye girer.
*/