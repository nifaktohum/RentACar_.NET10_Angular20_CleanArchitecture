using Application.Features.Reservations.Dto.Requests;
using Application.Features.Reservations.Dto.Responses;
using Application.Specifications;

namespace Application.Services;

public interface IReservationService
{
  // Komutlar (Command)
  Task<Guid> CreateAsync(CreateReservationDto dto, Guid userId, CancellationToken cancellationToken = default);
  // Kapora / Güvence ödemesi
  Task PayDepositAsync(Guid id, PayDepositDto dto, Guid userId, CancellationToken cancellationToken = default);
  // Aracı teslim etme / Kiralama başlatma
  Task ActivateAsync(Guid id, ActivateReservationDto dto, Guid userId, CancellationToken cancellationToken = default);
  // Aracı teslim alma / Kiralama tamamlama
  Task CompleteAsync(Guid id, CompleteReservationDto dto, Guid userId, CancellationToken cancellationToken = default);
  // İptal etme
  Task CancelAsync(Guid id, CancelReservationDto dto, Guid userId, CancellationToken cancellationToken = default);

  // Sorgular (Query)
  Task<ReservationDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
  Task<List<ReservationDto>> GetListAsync(ReservationSpecParams specParams, CancellationToken cancellationToken = default);
}
