using System.Security.Claims;
using Domain.Abstractions;
using Domain.Branchs;
using Domain.Categories;
using Domain.Roles;
using Domain.Users;
using GenericRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Context;

// DbContext sınıfından türeterek veritabanı yönetim sınıfımızı oluşturuyoruz
public class AppDbContext : DbContext, IUnitOfWork
{
  private readonly IHttpContextAccessor _httpContextAccessor;

  public AppDbContext(DbContextOptions _options, IHttpContextAccessor httpContextAccessor) : base(_options)
  {
    _httpContextAccessor = httpContextAccessor;
  }

  public DbSet<Branch> Branches { get; set; }
  public DbSet<User> Users { get; set; }
  public DbSet<Role> Roles { get; set; }
  public DbSet<UserRole> UserRoles { get; set; }
  public DbSet<Permission> Permissions { get; set; }
  public DbSet<PermissionRole> PermissionRoles { get; set; }
  public DbSet<Category> Categories { get; set; }

  protected override void OnModelCreating(ModelBuilder _modelBuilder)
  {
    _modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    _modelBuilder.ApplyGlobalFilters();
    // Sistemdeki tüm Permission sorgularına otomatik olarak bu WHERE şartını ekler
    _modelBuilder.Entity<Permission>().HasQueryFilter(p => !p.IsDeleted && p.IsActive);
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
        : Guid.Parse("00000000-0000-0000-0000-000000000001");


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


