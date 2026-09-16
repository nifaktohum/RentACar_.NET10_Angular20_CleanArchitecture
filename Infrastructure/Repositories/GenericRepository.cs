using Domain.Abstractions;
using Domain.Repositories;
using Infrastructure.Context;
using Infrastructure.Specifications;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class GenericRepository<T>(AppDbContext _context) : IGenericRepository<T> where T : BaseEntity
{
  /// Verilen ID değerine sahip varlığı veritabanından asenkron olarak bulur ve döner.
  public async Task<T?> GetByIdAsync(int id)
  {
    return await _context.Set<T>().FindAsync(id);
  }

  /// İlgili tabloya ait tüm kayıtları asenkron olarak liste şeklinde getirir.
  public async Task<IReadOnlyList<T>> ListAllAsync()
  {
    return await _context.Set<T>().ToListAsync();
  }

  /// Gönderilen Specification kurallarına (filtre, include vb.) göre tek bir varlık getirir.
  public async Task<T?> GetEntityWithSpec(ISpecification<T> _spec)
  {
    return await ApplySpecification(_spec).FirstOrDefaultAsync();
  }

  /// Gönderilen Specification kurallarına göre filtrelenmiş kayıtları liste halinde getirir.
  public async Task<IReadOnlyList<T>> ListAsync(ISpecification<T> _spec)
  {
    return await ApplySpecification(_spec).ToListAsync();
  }

  /// Specification'ı IQueryable'a çeviren yardımcı metottur.
  /// Filtre, Include, Order, Paging mantığını tek bir yerde toplar, 
  /// sorgu kurallarını alıp veritabanına gönderilebilir hale getirir.
  private IQueryable<T> ApplySpecification(ISpecification<T> _spec)
  {
    return SpecificationEvaluator<T>.GetQuery(_context.Set<T>().AsQueryable(), _spec);
  }

  /// Specification kullanarak veriyi filtreler ve sonucu farklı bir tipe (TResult - örn. DTO) projelendirerek tek bir kayıt döner.
  public async Task<TResult?> GetEntityWithSpec<TResult>(ISpecification<T, TResult> _spec)
  {
    return await ApplySpecification(_spec).FirstOrDefaultAsync();
  }

  /// Specification kullanarak verileri filtreler ve sonuçları farklı bir tipe (TResult - örn. DTO listesi) projelendirerek liste halinde döner.
  public async Task<IReadOnlyList<TResult>> ListAsync<TResult>(ISpecification<T, TResult> _spec)
  {
    return await ApplySpecification(_spec).ToListAsync();
  }

  /// Projelendirme (Select/DTO dönüşümü) içeren Specification kurallarını IQueryable<TResult> tipine dönüştüren yardımcı metottur.
  private IQueryable<TResult> ApplySpecification<TResult>(ISpecification<T, TResult> _spec)
  {
    return SpecificationEvaluator<T>.GetQuery<T, TResult>(_context.Set<T>().AsQueryable(), _spec);
  }
}