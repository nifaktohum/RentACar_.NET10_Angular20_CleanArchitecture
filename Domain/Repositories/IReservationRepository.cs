using Domain.Entities.Reservations;
using Domain.Specifications;
using GenericRepository;

namespace Domain.Repositories;

public interface IReservationRepository : IRepository<Reservation>
{
  // ⭐ Specification Pattern için
  Task<List<Reservation>> GetAsync(BaseSpecification<Reservation> spec, CancellationToken cancellationToken = default);
  Task<Reservation?> GetFirstAsync(BaseSpecification<Reservation> spec, CancellationToken cancellationToken = default);
  Task<int> CountAsync(BaseSpecification<Reservation> spec, CancellationToken cancellationToken = default);
} 