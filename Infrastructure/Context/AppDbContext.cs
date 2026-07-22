using System.Security.Claims;
using Domain.Abstractions;
using Domain.Entities.Branchs;
using Domain.Entities.Categories;
using Domain.Entities.Protection;
using Domain.Entities.Roles;
using Domain.Entities.Users;
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

  // DbSet'ler
  public DbSet<ProtectionPackage> ProtectionPackages { get; set; }
  public DbSet<ProtectionBenefit> ProtectionBenefits { get; set; }
  public DbSet<ProtectionPricing> ProtectionPricings { get; set; }
  public DbSet<BenefitCategory> BenefitCategories { get; set; }

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
    _modelBuilder.Entity<User>()
        .HasIndex(u => u.Email)
        .HasDatabaseName("IX_Users_Email")
        .IsUnique()
        .HasFilter("\"IsDeleted\" = false");

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


