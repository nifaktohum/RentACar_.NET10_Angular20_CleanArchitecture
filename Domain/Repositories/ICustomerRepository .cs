using Domain.Entities.Customers;
using GenericRepository;

namespace Domain.Repositories;

public interface ICustomerRepository : IRepository<Customer>
{
  /// Email ile müşteri getirir (Login ve unique kontrolü için)
  Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

  /// Telefon numarası ile müşteri getirir (unique kontrolü için)
  Task<Customer?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default);

  /// T.C. Kimlik No ile müşteri getirir (bireysel müşteri kontrolü)
  Task<Customer?> GetByIdentityNumberAsync(string identityNumber, CancellationToken cancellationToken = default);

  /// Vergi numarası ile müşteri getirir (kurumsal müşteri kontrolü)
  Task<Customer?> GetByTaxNumberAsync(string taxNumber, CancellationToken cancellationToken = default);

  /// Email kullanımda mı? (Register validasyonu) Register'da hızlı unique kontrolü (entity yüklemeden)
  Task<bool> IsEmailExistsAsync(string email, CancellationToken cancellationToken = default);

  /// Telefon kullanımda mı? (Register validasyonu)
  Task<bool> IsPhoneNumberExistsAsync(string phoneNumber, CancellationToken cancellationToken = default);

  /// T.C. Kimlik No kullanımda mı? Aynı T.C. ile 2. kayıt engeli
  Task<bool> IsIdentityNumberExistsAsync(string identityNumber, CancellationToken cancellationToken = default);

  /// Vergi numarası kullanımda mı? Aynı vergi no ile 2. kurumsal kayıt engeli
  Task<bool> IsTaxNumberExistsAsync(string taxNumber, CancellationToken cancellationToken = default);

  /// Refresh token ile müşteri getirir
  Task<Customer?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

  Task<string?> GetSecurityStampByIdAsync(Guid customerId, CancellationToken cancellationToken = default);

  // ==================== ⭐ SPECIFICATION METOTLARI ====================
  /// Specification ile filtrelenmiş + sıralanmış + sayfalanmış liste getirir
  Task<List<Customer>> GetListWithSpecAsync(
      ISpecification<Customer> spec,
      CancellationToken cancellationToken = default);

  /// Specification ile toplam sayıyı getirir (paging'siz)
  Task<int> CountWithSpecAsync(
      ISpecification<Customer> spec,
      CancellationToken cancellationToken = default);

  /// Specification ile tek kayıt getirir (detay için) //  include gerektiğinde kullanılır.
  Task<Customer?> GetSingleWithSpecAsync(
      ISpecification<Customer> spec,
      CancellationToken cancellationToken = default);
}
