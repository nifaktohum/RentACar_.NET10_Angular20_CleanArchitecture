using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using Domain.Repositories;
using Domain.Users;
using GenericRepository;
using Infrastructure.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

// Primary Constructor kullanarak base sınıfa context'i direkt fırlatıyoruz
public sealed class UserRepository : Repository<User, AppDbContext>, IUserRepository
{
  private AppDbContext _context;
  private readonly IHttpContextAccessor _httpContextAccessor;

  public UserRepository(AppDbContext context, IHttpContextAccessor httpContextAccessor) : base(context)
  {
    _context = context;
    _httpContextAccessor = httpContextAccessor;
  }
  //===================================================>
  // sisteme giriş yapmış olan kullanıcının ID'si

  
  public Guid GetCurrentUserId()
  {
    var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    // Eğer userId boşsa Guid.Empty döner, aksi halde parse eder.
    return Guid.TryParse(userId, out var id) ? id : Guid.Empty;
  }
  public async Task<User?> GetByEmailWithRolesAsync(string email, CancellationToken _token = default)
  {
    // 1. ADIM: Sadece kullanıcıyı çek (hafif sorgu)
    var user = await _context.Users
        .FirstOrDefaultAsync(u => u.Email == email, _token);

    if (user is null) return null;

    // ✅ 2. ADIM: Branch'i YÜKLE! (Yorumu kaldır!)
    await _context.Entry(user)
        .Reference(u => u.Branch)
        .LoadAsync(_token);

    // 3. ADIM: Rolleri yükle
    await _context.Entry(user)
        .Collection(u => u.UserRoles)
        .Query()
        .Include(ur => ur.Role)
        .LoadAsync(_token);

    return user;
  }
  public async Task<string?> GetSecurityStampByIdAsync(Guid userId, CancellationToken _token = default)
  {
    // token validation sırasında kullanıcının stamp'ini kontrol ediyorsun. rol değişince token geçersiz olsun
    return await _context.Users                           // 1. Users tablosuna git
                            .Where(u => u.Id == userId)   // 2. Users tablosunda Id'si verilen kullanıcıyı bul
                            .Select(u => u.SecurityStamp) // 3. SADECE SecurityStamp alanını getir (tüm User'ı değil)
                            .FirstOrDefaultAsync(_token); // 3. İlkini (tekini) al, yoksa default (null)
  }

  public async Task<Dictionary<Guid, string>> GetUserNamesByIdsAsync(List<Guid> userIds, CancellationToken _token)
  {
    if (userIds == null || !userIds.Any())
      return new Dictionary<Guid, string>();

    // _context kullan (base'den gelen protected field)
    var usersList = await _context.Set<User>()
        .Where(u => userIds.Contains(u.Id))
        .Select(u => new { u.Id, Name = u.FullName })
        .ToListAsync(_token);

    return usersList.ToDictionary(x => x.Id, x => x.Name);
  }

  public async Task<List<string>> GetUserPermissionsAsync(Guid userId, CancellationToken _token)
  {
    if (userId == Guid.Empty) return new List<string>();

    return await _context.UserRoles
        .Where(ur => ur.UserId == userId)
        .Select(ur => ur.Role)                    // 1. Kullanıcının rollerine geçiş yap
        .SelectMany(role => role.PermissionRoles) // 2. O rollerin içindeki PermissionRoles listelerini patlat (düzleştir)
        .Select(pr => pr.Permission.Name)         // 3. Doğrudan ana Permission tablosundaki Name alanını çek
        .Distinct()                               // 4. Farklı rollerden gelen mükerrer izinleri tekilleştir
        .ToListAsync(_token);
  }

  public async Task<User?> GetUserWithRolesAsync(Guid userId, CancellationToken _token)
  {
    return await _context.Users
                .AsNoTracking() // Takibi kapat, sadece oku
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                        .ThenInclude(r => r.PermissionRoles)
                            .ThenInclude(pr => pr.Permission)
                .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, _token);
  }

  public async Task<List<string>> GetUserRoleNamesAsync(Guid userId, CancellationToken cancellationToken)
  {
    return await _context.Users
        .Where(u => u.Id == userId && !u.IsDeleted)
        .SelectMany(u => u.UserRoles.Select(ur => ur.Role.Name))  // ⭐ IsDeleted kontrolü KALDIRILDI
        .Distinct()
        .ToListAsync(cancellationToken);
  }

  public async Task<bool> HasRoleAsync(Guid userId, string[] roleNames, CancellationToken cancellationToken)
  {
    var userRoles = await GetUserRoleNamesAsync(userId, cancellationToken);
    return roleNames.Any(role => userRoles.Contains(role));
  }
  
  //  bir kullanıcının belirli bir role sahip olup olmadığını kontrol eder.
  public async Task<bool> IsUserInRoleAsync(Guid userId, string roleName, CancellationToken cancellationToken)
  {
    return await _context.Users
        .Where(u => u.Id == userId && !u.IsDeleted)
        .SelectMany(u => u.UserRoles.Select(ur => ur.Role))
        .AnyAsync(r => r.Name == roleName, cancellationToken);  // ⭐ !r.IsDeleted KALDIRILDI
  }
  
  // "Şu an giriş yapmış olan kullanıcı, Admin rolüne sahip mi?" diye sorar.
  public async Task<bool> IsCurrentUserInRoleAsync(string roleName, CancellationToken cancellationToken)
  {
    var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userId)) return false;

    return await IsUserInRoleAsync(Guid.Parse(userId), roleName, cancellationToken);
  }

  public async Task<List<User>> GetUsersAsync(CancellationToken token)
  {
    return await _context.Users
        .AsNoTracking()
        .Where(u => !u.IsDeleted)
        .Include(u => u.Branch)          // ⬅️ Branch zorunlu, Include et!
        .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
        .OrderByDescending(u => u.CreatedAt)
        .ToListAsync(token);
  }

  public async Task<User?> GetUserWithDetailsByIdAsync(Guid userId, CancellationToken token)
  {
    var res = await _context.Users
        .Include(u => u.Branch) // Şube verisi
        .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role) // Rol verisi
                .ThenInclude(r => r.PermissionRoles) // Rol -> PermissionRoles
                    .ThenInclude(pr => pr.Permission) // PermissionRoles -> Permission
        .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, token);

    return res;
  }

  public async Task<bool> IsEmailUniqueExceptUserAsync(string email, Guid userId, CancellationToken token = default)
  {
    return !await _context.Users
         .AnyAsync(u => u.Email == email && u.Id != userId && !u.IsDeleted, token);
  }

  public async Task<List<User>> GetUsersWithDetailsAsync(CancellationToken token = default)
  {

    var res = await _context.Users
        .AsNoTracking()
        .Where(u => !u.IsDeleted)
        .Include(u => u.Branch)
        .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
        .OrderByDescending(u => u.CreatedAt)
        .ToListAsync(token);

    return res;
  }

  public async Task<bool> IsEmailUniqueAsync(string email, CancellationToken token)
  {
    // Sadece silinmemiş (aktif) kullanıcılar arasında bak!
    return !await _context.Users
        .AnyAsync(u => u.Email == email && !u.IsDeleted, token);
  }
  
  // Branch adına göre kullanıcıları getiren yeni metot
  public async Task<List<User>> GetUsersByBranchIdAsync(Guid branchId, CancellationToken token = default)
  {
    return await _context.Users
        .AsNoTracking()
        .Where(u => !u.IsDeleted && u.BranchId == branchId) // Guid karşılaştırması
        .Include(u => u.Branch)
        .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
        .OrderByDescending(u => u.CreatedAt)
        .ToListAsync(token);
  }

}
