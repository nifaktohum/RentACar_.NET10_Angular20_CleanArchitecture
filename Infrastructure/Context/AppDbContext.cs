using System.Security.Claims;
using Domain.Abstractions;
using Domain.Entities.Branchs;
using Domain.Entities.Categories;
using Domain.Entities.Customers;
using Domain.Entities.Extras;
using Domain.Entities.Protection;
using Domain.Entities.Reservations;
using Domain.Entities.Roles;
using Domain.Entities.Users;
using Domain.Entities.Vehicles;
using GenericRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Context;

// DbContext sınıfından türeterek veritabanı yönetim sınıfımızı oluşturuyoruz
public class AppDbContext : DbContext, IUnitOfWork
{
  private readonly IHttpContextAccessor _httpContextAccessor;
  private readonly IConfiguration _config;
  private readonly ILogger<AppDbContext> _logger;

  public AppDbContext(DbContextOptions _options, IHttpContextAccessor httpContextAccessor, IConfiguration config, ILogger<AppDbContext> logger) : base(_options)
  {
    _httpContextAccessor = httpContextAccessor;
    _config = config;
    _logger = logger;
  }

  // ==================== DB SETS ====================
  public DbSet<Branch> Branches { get; set; }
  public DbSet<User> Users { get; set; }
  public DbSet<Role> Roles { get; set; }
  public DbSet<UserRole> UserRoles { get; set; }
  public DbSet<Permission> Permissions { get; set; }
  public DbSet<PermissionRole> PermissionRoles { get; set; }
  public DbSet<Category> Categories { get; set; }

  // Extras
  public DbSet<Extra> Extras { get; set; }
  public DbSet<RentalExtra> RentalExtras { get; set; }

  // Protection
  public DbSet<ProtectionPackage> ProtectionPackages { get; set; }
  public DbSet<ProtectionBenefit> ProtectionBenefits { get; set; }
  public DbSet<ProtectionPricing> ProtectionPricings { get; set; }
  public DbSet<BenefitCategory> BenefitCategories { get; set; }

  // Vehicles
  public DbSet<Vehicle> Vehicles { get; set; }
  public DbSet<VehicleModel> VehicleModels { get; set; }
  public DbSet<VehicleType> VehicleTypes { get; set; }
  public DbSet<VehicleImage> VehicleImages { get; set; }

  // Customers
  public DbSet<Customer> Customers { get; set; }

  // Reservations
  public DbSet<Reservation> Reservations { get; set; }
  public DbSet<ReservationSequence> ReservationSequences { get; set; }


  // ==================== MODEL CREATING ====================
  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    // 1. Tüm Configuration'ları otomatik bul
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

    // 2. Global filter uygula (tüm BaseEntity türevleri için !IsDeleted)
    modelBuilder.ApplyGlobalFilters();

    // 3. Permission için özel filter (IsDeleted + IsActive)
    modelBuilder.Entity<Permission>()
        .HasQueryFilter(p => !p.IsDeleted && p.IsActive);

    // 4. User için unique email index
    modelBuilder.Entity<User>()
        .HasIndex(u => u.Email)
        .HasDatabaseName("IX_Users_Email")
        .IsUnique()
        .HasFilter("\"IsDeleted\" = false");

    base.OnModelCreating(modelBuilder);
  }


  // ==================== CONFIGURE CONVENTIONS ====================
  protected override void ConfigureConventions(ModelConfigurationBuilder builder)
  {
    // 1. Enum'ları PostgreSQL'de TEXT olarak sakla (okunabilir)
    builder.Properties<Enum>().HaveConversion<string>();

    // 2. DateTime / DateTimeOffset → timestamp with time zone
    builder.Properties<DateTime>().HaveColumnType("timestamp with time zone");
    builder.Properties<DateTime?>().HaveColumnType("timestamp with time zone");
    builder.Properties<DateTimeOffset>().HaveColumnType("timestamp with time zone");
    builder.Properties<DateTimeOffset?>().HaveColumnType("timestamp with time zone");

    // 3. Decimal precision (para birimi için)
    builder.Properties<decimal>().HavePrecision(18, 2);
    builder.Properties<decimal?>().HavePrecision(18, 2);

    base.ConfigureConventions(builder);
  }

  // ==================== SAVE CHANGES ====================
  public override int SaveChanges()
  {
    ApplyAuditMetadata();
    return base.SaveChanges();
  }

  public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
  {
    ApplyAuditMetadata();
    return await base.SaveChangesAsync(cancellationToken);
  }


  // ==================== PRIVATE YARDIMCILAR ====================
  private void ApplyAuditMetadata()
  {
    var userId = ResolveUserId();
    var entries = ChangeTracker.Entries<BaseEntity>();

    foreach (var entry in entries)
    {
      switch (entry.State)
      {
        case EntityState.Added:
          entry.Entity.SetCreateMetadata(userId);
          break;

        case EntityState.Modified:
          entry.Entity.UpdateMetadata(userId);
          break;

        case EntityState.Deleted:
          entry.State = EntityState.Modified;
          entry.Entity.SoftDelete(userId);
          break;
      }
    }
  }

  private Guid ResolveUserId()
  {
    // 1. HTTP context'ten kullanıcı ID'si
    var userIdClaim = _httpContextAccessor.HttpContext?.User?
        .FindFirst(ClaimTypes.NameIdentifier)?.Value;

    if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var parsedGuid))
      return parsedGuid;

    // 2. Fallback: config'den admin ID
    if (Guid.TryParse(_config["SeedData:AdminUserId"], out var adminGuid))
      return adminGuid;

    // 3. Hiçbiri yoksa hata
    throw new InvalidOperationException(
        "Kullanıcı ID'si belirlenemedi! Ne HTTP context'te claim var ne de config'de SeedData:AdminUserId tanımlı.");
  }

}








/*
using System.Security.Claims;
using Domain.Abstractions;
using Domain.Entities.Branchs;
using Domain.Entities.Categories;
using Domain.Entities.Customers;
using Domain.Entities.Extras;
using Domain.Entities.Protection;
using Domain.Entities.Reservations;
using Domain.Entities.Roles;
using Domain.Entities.Users;
using Domain.Entities.Vehicles;
using GenericRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Context;

// DbContext sınıfından türeterek veritabanı yönetim sınıfımızı oluşturuyoruz
public class AppDbContext : DbContext, IUnitOfWork
{
  private readonly IHttpContextAccessor _httpContextAccessor;
  private readonly IConfiguration _config;

  public AppDbContext(DbContextOptions _options, IHttpContextAccessor httpContextAccessor, IConfiguration config) : base(_options)
  {
    _httpContextAccessor = httpContextAccessor;
    _config = config;
  }

  public DbSet<Branch> Branches { get; set; }
  public DbSet<User> Users { get; set; }
  public DbSet<Role> Roles { get; set; }
  public DbSet<UserRole> UserRoles { get; set; }
  public DbSet<Permission> Permissions { get; set; }
  public DbSet<PermissionRole> PermissionRoles { get; set; }
  public DbSet<Category> Categories { get; set; }
  // Extras
  public DbSet<Extra> Extras { get; set; }
  public DbSet<RentalExtra> RentalExtras { get; set; }
  // ProtectionPackage
  public DbSet<ProtectionPackage> ProtectionPackages { get; set; }
  public DbSet<ProtectionBenefit> ProtectionBenefits { get; set; }
  public DbSet<ProtectionPricing> ProtectionPricings { get; set; }
  public DbSet<BenefitCategory> BenefitCategories { get; set; }
  public DbSet<Vehicle> Vehicles { get; set; }
  public DbSet<VehicleModel> VehicleModels { get; set; }
  public DbSet<VehicleType> VehicleTypes { get; set; }
  public DbSet<VehicleImage> VehicleImages { get; set; }
  public DbSet<Customer> Customers { get; set; }

  // Reservations
  public DbSet<Reservation> Reservations { get; set; }
  public DbSet<ReservationSequence> ReservationSequences { get; set; }


  protected override void OnModelCreating(ModelBuilder _modelBuilder)
  {
    // 1. Tüm Configuration'ları otomatik bul
    _modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    // 2. Global filter uygula (tüm entity'ler için)
    _modelBuilder.ApplyGlobalFilters();
    // 3. Permission için özel filter
    // Sistemdeki tüm Permission sorgularına otomatik olarak bu WHERE şartını ekler
    _modelBuilder.Entity<Permission>().HasQueryFilter(p => !p.IsDeleted && p.IsActive);
    // 4. User için özel index
    _modelBuilder.Entity<User>().HasIndex(u => u.Email)
                                .HasDatabaseName("IX_Users_Email")
                                .IsUnique()
                                .HasFilter("\"IsDeleted\" = false");

    // WHERE [IsDeleted] = 0 koşulunu kendisi ekler.
    _modelBuilder.Entity<Extra>().HasQueryFilter(e => !e.IsDeleted);

    // WHERE [IsDeleted] = 0 koşulunu kendisi ekler.
    _modelBuilder.Entity<RentalExtra>().HasQueryFilter(re => !re.IsDeleted);

    base.OnModelCreating(_modelBuilder);
  }

  protected override void ConfigureConventions(ModelConfigurationBuilder _builder)
  {
    // 1. Enum'ları PostgreSQL'de TEXT olarak sakla (OKUNABİLİR)
    _builder.Properties<Enum>().HaveConversion<string>();
    // 2. Hem normal hem de nullable tüm DateTime alanlarını PostgreSQL'in sevdiği formatta eşitler
    _builder.Properties<DateTime>().HaveColumnType("timestamp with time zone");
    _builder.Properties<DateTime?>().HaveColumnType("timestamp with time zone");
    // 3. Projede DateTimeOffset kullandıysan (ki kurumsal mimaride tavsiyemdir), onları da eşitler
    _builder.Properties<DateTimeOffset>().HaveColumnType("timestamp with time zone");
    _builder.Properties<DateTimeOffset?>().HaveColumnType("timestamp with time zone");
    // Decimal precision (para birimi için)
    _builder.Properties<decimal>().HavePrecision(18, 4); // 14 tam, 4 kuruş
    _builder.Properties<decimal?>().HavePrecision(18, 4); // 14 tam, 4 kuruş

    base.ConfigureConventions(_builder);
  }

  // SAVECHANGES OVERRIDE: Senin akıllı metotlarını burada tetikliyoruz
  public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
  {
    var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    Guid userId = (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var parsedGuid))
        ? parsedGuid
        : Guid.Parse(_config["SeedData:AdminUserId"]!);


    var entries = ChangeTracker.Entries<BaseEntity>();

    // 3. Her bir entity için işlem yap
    foreach (var entry in entries)
    {
      Console.WriteLine(
       $"{entry.Entity.GetType().Name} - {entry.State}"
   );

      switch (entry.State)
      {
        case EntityState.Added:
          // Yeni eklenen: CreatedBy, CreatedAt set et
          entry.Entity.SetCreateMetadata(userId);
          break;

        case EntityState.Modified:
          // Güncellenen: UpdatedBy, UpdatedAt set et
          entry.Entity.UpdateMetadata(userId);
          break;

        case EntityState.Deleted:
          // Silinen: Soft Delete yap (IsDeleted = true)
          entry.State = EntityState.Modified;
          entry.Entity.SoftDelete(userId);
          break;
      }
    }

    return await base.SaveChangesAsync(cancellationToken);

  }
}
*/


