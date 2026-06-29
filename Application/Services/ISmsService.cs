namespace Application.Services;

public interface ISmsService
{
  Task<bool> SendSmsAsync(string toNumber, string message, CancellationToken cancellationToken = default);
}
