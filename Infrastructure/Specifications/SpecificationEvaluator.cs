using Domain.Abstractions;
using Domain.Repositories;


namespace Infrastructure.Specifications;


// specification içinde tanımladığın bütün kuralları (Criteria, Includes, OrderBy, Paging vb.) zincirleme bir şekilde sorguya tek tek giydirmek.
// elindeki ham sorguyu alıp, specification kurallarıyla süsleyerek veritabanına fırlatılmaya hazır kusursuz bir SQL adayı haline getiren usta benim!
public static class SpecificationEvaluator<T> where T : BaseEntity
{
  // Veritabanından doğrudan oentity'nin kendisini (Vehicle, Brand vb.) çekmek istediğimiz durumlar içindir.
  public static IQueryable<T> GetQuery(IQueryable<T> _inputQuery, ISpecification<T> _spec)
  {
    var query = _inputQuery;

    // 1️⃣ WHERE (Filtreleme)
    // v => v.Brand == "Toyota" && v.IsAvailable
    if (_spec.Criteria != null)
    {
      query = query.Where(_spec.Criteria);
    }

    // 4️⃣ ORDER BY (Sıralama)
    // .OrderBy(v => v.DailyPrice)
    if (_spec.OrderBy != null)
    {
      query = query.OrderBy(_spec.OrderBy);
    }
    else if (_spec.OrderByDescending != null)
    {
      query = query.OrderByDescending(_spec.OrderByDescending);
    }

    // // 2️⃣ INCLUDES (İlişkili verileri yükleme)
    // // .Include(v => v.VehicleType)
    // // .Include(v => v.Images)
    // query = _spec.Includes.Aggregate(query,
    //     (current, include) => current.Include(include));

    // // 3️⃣ INCLUDE STRINGS (String ile include)
    // // .Include("Brand.Models")
    // query = _spec.IncludeStrings.Aggregate(query,
    //     (current, include) => current.Include(include));


    // // 5️⃣ DISTINCT (Tekrarsız)
    if (_spec.IsDistinct)
      query = query.Distinct();

    // // 6️⃣ PAGING (Sayfalama)
    // // .Skip(10).Take(10)
    if (_spec.IsPagingEnabled)
      query = query.Skip(_spec.Skip).Take(_spec.Take);

    return query;

  }


  // Performans optimizasyonu yapmak veya dışarıya tam tabloyu değil de sadece belirli alanları (DTO) dönmek istediğimiz durumlar içindir
  public static IQueryable<TResult> GetQuery<TSpec, TResult>(IQueryable<T> _inputQuery, ISpecification<T, TResult> _spec)
  {
    var query = _inputQuery;

    //========================================
    if (_spec.Criteria != null)
      query = query.Where(_spec.Criteria);

    //========================================
    if (_spec.OrderBy != null)
      query = query.OrderBy(_spec.OrderBy);
    else if (_spec.OrderByDescending != null)
      query = query.OrderByDescending(_spec.OrderByDescending);

    //========================================
    var selectQuery = query as IQueryable<TResult>;

    if (_spec.Select != null)
      selectQuery = query.Select(_spec.Select);
    
    //========================================
    if (_spec.IsDistinct)
      selectQuery = selectQuery?.Distinct();

    // DTO / Projection tarafında da paging eklemek için:
    if (_spec.IsPagingEnabled)
      selectQuery = selectQuery?.Skip(_spec.Skip).Take(_spec.Take);
    





    return selectQuery ?? query.Cast<TResult>();
  }


}
