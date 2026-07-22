using API.Middlewares;
using Application;
using Infrastructure;
using Infrastructure.Context;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var _builder = WebApplication.CreateBuilder(args);
var _congig = _builder.Services.AddSingleton<IConfiguration>(_builder.Configuration);

var adminEmail = _builder.Configuration["SeedData:AdminEmail"] ?? "admin@rentacar.com";


// --- 1. SERVIS KAYITLARI (DEPENDENCY INJECTION) ---
_builder.Services.AddHttpContextAccessor();
_builder.Services.AddApplication();
_builder.Services.AddInfrastructure(_builder.Configuration);
_builder.Services.AddExceptionHandler<API.Exceptions.ExceptionHandler>().AddProblemDetails();
_builder.Services.AddControllers();
// _builder.Services.AddControllers().AddOData(_opt => { _opt.Select().Filter().Count().Expand().OrderBy().SetMaxTop(100); });
_builder.Services.AddEndpointsApiExplorer();
_builder.Services.AddSwaggerGen(c =>
{
  // 🚗 Standart REST ve MediatR operasyonları için bir doküman tanımı
  c.SwaggerDoc("v1-All", new OpenApiInfo { Title = "RentCar API - All Endpoints", Version = "v1"});
  c.SwaggerDoc("v1-Branches", new() { Title = "RentCar Branches", Version = "v1" });
  c.SwaggerDoc("v1-Categories", new() { Title = "RentCar Categories", Version = "v1" });
  c.SwaggerDoc("v1-Auth", new() { Title = "RentCar Auth", Version = "v1" });
  c.SwaggerDoc("v1-Users", new() { Title = "RentCar Users", Version = "v1" });
  c.SwaggerDoc("v1-Profiles", new() { Title = "RentCar Profiles", Version = "v1" });
  c.SwaggerDoc("v1-Roles", new() { Title = "RentCar Roles", Version = "v1" });
  c.SwaggerDoc("v1-Permissions", new() { Title = "RentCar Permissions", Version = "v1" });
  c.SwaggerDoc("v1-ProtectionPackages", new() { Title = "RentCar ProtectionPackages", Version = "v1" });
  c.SwaggerDoc("v1-BenefitCategories", new() { Title = "RentCar BenefitCategories", Version = "v1" });

  // Endpoint'leri GroupName'e göre doğru sekmeye dağıtan sihirli kural kanka!
  c.DocInclusionPredicate((docName, apiDesc) =>
  {
    // ALL dokümanı tüm endpointleri içerir
    if (docName == "v1-All")
      return true;

    // Diğer dokümanlar sadece kendi GroupName'lerini içerir
    return apiDesc.GroupName == docName;
  });

  // JWT Authentication Tanımlaması
  c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
  {
    In = ParameterLocation.Header,
    Description = "Please enter token",
    Name = "Authorization",
    Type = SecuritySchemeType.Http,
    BearerFormat = "JWT",
    Scheme = "Bearer"
  });

  c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
  {
    [new OpenApiSecuritySchemeReference("Bearer", document)] = []
  });
});

_builder.Services.AddCors(options =>
{
  options.AddPolicy("AllowAngularApp", policy =>
  {
    policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
          .AllowAnyMethod()
          .AllowAnyHeader()
          .AllowCredentials()
          .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
  });
});

_builder.Services.AddResponseCompression(opt =>
{
  opt.EnableForHttps = true;
});

_builder.Services.AddTransient<SecurityStampMiddleware>();
//==========================================================================================>
// --- 2. MIDDLEWARE BORU HATTI (PIPELINE) SIRALAMASI ---
var app = _builder.Build();

// 1. EN DIŞ KALE: Hataları küresel yakalama
app.UseExceptionHandler();

// 2. GÜVENLİK PROTOKOLÜ: HTTPS Yönlendirmesi
app.UseHttpsRedirection();

// 3. PERFORMANS KATMANI: Giden veriyi sıkıştırma
app.UseResponseCompression();


//===================================================================>
// 4. GELİŞTİRİCİ ARAÇLARI: Swagger Paneli
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI(c =>
  {
    c.SwaggerEndpoint("/swagger/v1-All/swagger.json", "RentCar API (All)");
    c.SwaggerEndpoint("/swagger/v1-Branches/swagger.json", "RentCar Branches");
    c.SwaggerEndpoint("/swagger/v1-Categories/swagger.json", "RentCar Categories");
    c.SwaggerEndpoint("/swagger/v1-Auth/swagger.json", "RentCar Auth");
    c.SwaggerEndpoint("/swagger/v1-Users/swagger.json", "RentCar Users");
    c.SwaggerEndpoint("/swagger/v1-Profiles/swagger.json", "RentCar Profiles");
    c.SwaggerEndpoint("/swagger/v1-Roles/swagger.json", "RentCar Roles");
    c.SwaggerEndpoint("/swagger/v1-Permissions/swagger.json", "RentCar Permissions");
    c.SwaggerEndpoint("/swagger/v1-ProtectionPackages/swagger.json", "RentCar ProtectionPackages");
    c.SwaggerEndpoint("/swagger/v1-BenefitCategories/swagger.json", "RentCar BenefitCategories");
  });
}

// 5. ADRESLEME KATMANI
app.UseRouting();

// 6. DIŞ DÜNYA İZNİ: CORS politikası
app.UseCors("AllowAngularApp");

// 7. HIZ SINIRLAYICI (Rate Limiter): Yetkilendirmeden ÖNCE çalışarak API'yi korur kanks
app.UseRateLimiter();

// 8. KİMLİK DOĞRULAMA (Authentication)
app.UseAuthentication();

// 9. GÜVENLİK DAMGASI (Custom Middleware)
app.UseMiddleware<SecurityStampMiddleware>();

// 10. YETKİ KONTROLÜ (Authorization)
app.UseAuthorization();

// 11. ENDPOINT MAPPING
app.MapControllers().RequireAuthorization();
// =====================================================================================
// 👑 VERITABANI DATA SEED İŞLEMLERİ (MANTIKSAL SIRALAMA)
// =====================================================================================
// 👑 VERITABANI DATA SEED İŞLEMLERİ
using (var scope = app.Services.CreateScope())
{
  var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
  var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

  try
  {
    await context.Database.MigrateAsync();
    await app.InitializeDatabaseSeedAsync(config);
  }
  catch (Exception ex)
  {
    // Uygulamanın loglama servisini (ILogger) kullanarak hatayı yazdır
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Veritabanı migration veya seed işlemi sırasında hata oluştu.");
    // Gerekirse uygulamayı durdur veya hata ile devam et
    throw;
  }
}
// =====================================================================================

// 12. MOTORU ÇALIŞTIR
await app.RunAsync();