using Application.Services;
using Domain.Repositories;
using Domain.Repositories.Protection;
using GenericRepository;
using Infrastructure.Context;
using Infrastructure.Options;
using Infrastructure.Repositories;
using Infrastructure.Repositories.Protection;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
  public static IServiceCollection AddInfrastructure(this IServiceCollection _services, IConfiguration _config)
  {
    // =============================================================================
    // 1. SİSTEM VE ALTYAPI SERVİSLERİ
    // =============================================================================
    // SaveChangesAsync içinde JWT Token'dan kullanıcı ID'sini (Claim) okuyabilmek için bu servis şart.
    _services.AddHttpContextAccessor();
    _services.AddScoped<IUserContext, UserContext>();
    _services.AddScoped<IPasswordHasher, PasswordHasher>();
    _services.AddScoped<IJwtProvider, JwtProvider>();
    _services.AddScoped<IEmailService, EmailService>();
    _services.AddScoped<IRoleRepository, RoleRepository>();
    _services.AddScoped<IProtectionPackageRepository, ProtectionPackageRepository>();
    _services.AddScoped<IProtectionBenefitRepository, ProtectionBenefitRepository>();
    _services.AddScoped<IProtectionPricingRepository, ProtectionPricingRepository>();

    // Rate Limiting ve Kimlik Doğrulama Seçeneklerini (Options Pattern) bağlıyoruz 
    _services.AddRateLimiter();
    _services.ConfigureOptions<JwtSetupOptions>();
    _services.ConfigureOptions<RateLimiterSetupOptions>();

    // Kimlik doğrulama şemasını sisteme bomboş tanıtıyoruz, ayarları zaten JwtSetupOptions yapacak 
    _services.AddAuthentication(options =>
    {
      options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
      options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
      // 🔥 İşte sorunu kökünden çözecek, claim mapping ayarı:
      options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
      {
        ValidateIssuer = true,
        // ... (Eğer JwtSetupOptions içinde zaten validate parametrelerin varsa, 
        // onları oradan okumaya devam et, burada sadece RoleClaimType'ı set et yeterli)
        RoleClaimType = "Role"
      };
    });

    // Yetkilendirme duvarını da sisteme dahil ediyoruz
    _services.AddAuthorization();

    // =============================================================================
    // 2. VERİTABANI VE VERİ ERİŞİM ALTYAPISI (EF Core & UnitOfWork)
    // =============================================================================
    _services.AddDbContext<AppDbContext>(opt =>
    {
      // appsettings.json içerisinden "PostgreSQL" anahtarına ait bağlantı dizesini güvenli bir şekilde okuyoruz.
      string connectionString = _config.GetConnectionString("PostgreSQL")
          ?? throw new InvalidOperationException("PostgreSQL bağlantı dizesi bulunamadı.");

      // PostgreSQL sürücüsünü bağlıyoruz.
      opt.UseNpgsql(connectionString);
    });

    // DbContext'i IUnitOfWork olarak resolve etmek için doğru yaklaşım.
    _services.AddScoped<IUnitOfWork>(srv => srv.GetRequiredService<AppDbContext>());

    // =============================================================================
    // 3. REPOSITORY (DEPO) KAYITLARI
    // =============================================================================
    // Proje büyüdükçe buraya yeni repository'ler eklenecek.
    // En altta toplu durmaları kod takibini kolaylaştırır.
    _services.AddScoped<IUserRepository, UserRepository>();
    _services.AddScoped<IBranchRepository, BranchRepository>();
    _services.AddScoped<ISmsService, SmsTwilioService>();
    _services.AddScoped<IPermissionRepository, PermissionRepository>();
    _services.AddScoped<ICategoryRepository, CategoryRepository>();

    return _services;
  }
}
