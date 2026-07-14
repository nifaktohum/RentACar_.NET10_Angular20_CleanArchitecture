using Domain.Entities.Protection;
using Domain.Repositories.Protection;
using GenericRepository;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class ProtectionPackageRepository : Repository<ProtectionPackage, AppDbContext>, IProtectionPackageRepository
{
  private readonly AppDbContext _context;

  public ProtectionPackageRepository(AppDbContext context) : base(context)
  {
    this._context = context;
  }

  public void Attach(object entity)
  {
    _context.Attach(entity);
  }


  // ==================== PAKET METODLARI ====================
  public async Task<List<ProtectionPackage>> GetActivePackagesAsync(CancellationToken cancellationToken = default)
  {
    // QueryFilter zaten IsDeleted'i eliyor, sadece IsActive kontrolü kalıyor
    return await Where(p => p.IsActive)
            .OrderBy(p => p.DisplayOrder)
            .ToListAsync(cancellationToken);
  }

  public IQueryable<ProtectionPackage> GetAll(bool ignoreFilters = false)
  {
    var query = _context.Set<ProtectionPackage>();

    return ignoreFilters ? query.IgnoreQueryFilters() : query;
  }

  // Ben paketi bulur, Include metodumla tüm detaylarını kuşanıp sana öyle getiririm.
  public async Task<ProtectionPackage?> GetPackageWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
  {
    return await _context.ProtectionPackages
            .Include(p => p.Benefits)
            .Include(p => p.Pricing)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
  }
  // Senin 'önerilen' olarak işaretlediğin ve aynı zamanda aktif olan paketler, bizim en çok para kazandıran ürünlerimizdir
  public async Task<List<ProtectionPackage>> GetRecommendedPackagesAsync(CancellationToken cancellationToken = default)
  {
    return await Where(p => p.IsRecommended && p.IsActive)
                .OrderBy(p => p.DisplayOrder)
                .ToListAsync(cancellationToken);
  }






}

/*  Attach ==>
        "Ben bir 'Elçi'yim. Eğer elinde daha önce veritabanıyla bağı kopmuş veya izlenmeyen bir nesne varsa, 
        onu getir; anında sistemin takibine alırım. 
        _context ile olan bağını yeniden kurarım, böylece üzerinde yapacağın değişikliklerden anında haberdar oluruz."


*/
