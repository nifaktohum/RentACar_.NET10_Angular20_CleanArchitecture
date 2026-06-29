using FluentValidation;
using MediatR;
using TS.Result;

namespace Application.Behaviors;

public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> _validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
  public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
  {
    // 1. Eğer istek için yazılmış bir validator kuralı yoksa direkt handler'a geçiyoruz
    if (!_validators.Any())
      return await next();

    // 2. Gelen request'i validasyon bağlamına alıp asenkron ve paralel olarak kuralları işletiyoruz
    var context = new ValidationContext<TRequest>(request);
    var validationResults = await Task.WhenAll(
        _validators.Select(v => v.ValidateAsync(context, cancellationToken))
    );

    // 3. Oluşan tüm validation hatalarını tek bir listede topluyoruz
    var failures = validationResults
        .SelectMany(r => r.Errors)
        .Where(f => f != null)
        .ToList();

    // 4. 🔥 SİHİRLİ DOKUNUŞ: Eğer hata varsa, reflection ile Result taklidi yapmıyoruz!
    // Doğrudan Exception fırlatarak bizim o kitap gibi yazdığın ExceptionHandler'a paslıyoruz kanka!
    if (failures.Count != 0)
    {
      throw new ValidationException(failures);
    }

    // 5. Hiçbir hata yoksa akışın iş koduna (Handler'a) geçmesine izin veriyoruz
    return await next();
  }
}