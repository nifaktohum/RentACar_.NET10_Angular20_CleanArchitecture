namespace Application.Services;

public interface IReservationNumberGenerator
{
  Task<string> GenerateAsync(CancellationToken cancellationToken = default);
}
