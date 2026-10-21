using Application.Common.Extensions;
using Domain.Entities.Reservations;
using Domain.Repositories;
using Domain.Specifications;
using GenericRepository;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class ReservationRepository : Repository<Reservation, AppDbContext>, IReservationRepository
{
  private readonly AppDbContext _context;
  public ReservationRepository(AppDbContext context) : base(context)
  {
    _context = context;
  }

  public async Task<Reservation?> GetFirstAsync(BaseSpecification<Reservation> spec, CancellationToken cancellationToken = default)
  {
    return await _context.Reservations.ApplySpecification(spec).FirstOrDefaultAsync(cancellationToken);
  }

  public async Task<List<Reservation>> GetAsync(BaseSpecification<Reservation> spec, CancellationToken cancellationToken = default)
  {
    return await _context.Reservations.ApplySpecification(spec).ToListAsync(cancellationToken);
  }

  public async Task<int> CountAsync(BaseSpecification<Reservation> spec, CancellationToken cancellationToken = default)
  {
    // ⭐ Paging ve Ordering uygulanmaz, sadece Criteria + Includes
    var query = _context.Reservations.AsQueryable();

    if (spec.Criteria != null)
      query = query.Where(spec.Criteria);

    return await query.CountAsync(cancellationToken);
  }
}
