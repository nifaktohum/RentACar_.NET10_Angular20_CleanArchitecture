using Application.Behaviors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
  public static IServiceCollection AddApplication(this IServiceCollection _services)
  {
    // MEDIATR-KAYDI: TS.MediatR paketini ve bu katmandaki tüm Command/Query Handler sınıflarını IoC Container'a kaydeder.
    _services.AddMediatR(config =>
    {
      // projenin içindeki tüm handler'ları otomatik buluyorum."
      config.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
      config.AddOpenBehavior(typeof(ValidationBehavior<,>));
      config.AddOpenBehavior(typeof(PermissionBehavior<,>));
    });
    // FLUENTVALIDATION - KAYDI: Katmanda yazacağın tüm AbstractValidator(doğrulama) sınıflarını otomatik bulur ve DI sistemine mühürler.
    // isteği handler'a göndermeden önce o bir ön kontrol yapıyor."
    _services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

    return _services;
  }
}
