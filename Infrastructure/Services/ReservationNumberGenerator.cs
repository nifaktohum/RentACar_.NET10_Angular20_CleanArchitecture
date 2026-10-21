using Application.Services;
using Domain.Entities.Reservations;
using Domain.Exceptions;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public sealed class ReservationNumberGenerator(
                        AppDbContext _context,
                        ILogger<ReservationNumberGenerator> _logger
                        ) : IReservationNumberGenerator

{
  private const int MaxRetries = 5;
  public async Task<string> GenerateAsync(CancellationToken cancellationToken = default)
  {
    // 0. Adım: Yıl değişimlerinde doğru sayaç takibi için güncel yılı alıyoruz
    var year = DateTime.UtcNow.Year;

    // 1. Adım: Aynı anda birden fazla istek geldiğinde çakışmayı yönetmek için Retry döngüsü kuruyoruz
    for (int attempt = 1; attempt <= MaxRetries; attempt++)
    {
      try
      {
        // 2. Adım: İlgili yıla ait sayaç kaydını veritabanından çekiyoruz
        var sequence = await _context.ReservationSequences
            .FirstOrDefaultAsync(s => s.Year == year, cancellationToken);

        // 3. Adım: Eğer o yıla ait ilk kez kayıt açılıyorsa (tablo boşsa) yeni bir sequence nesnesi üretip ekliyoruz
        if (sequence is null)
        {
          sequence = new ReservationSequence(year);
          _context.ReservationSequences.Add(sequence);
        }

        // 4. Adım: Domain mantığımızı çalıştırıp Entity içindeki sayaç değerini 1 artırıyoruz
        var nextNumber = sequence.GetNextNumber();

        // 5. Adım: Değişikliği hemen veritabanına yansıtıyoruz. 
        // ⭐ KRİTİK NOKTA: Eğer biz bu veriyi okuduktan sonra başka bir işlem bu satırı değiştirdiyse, 
        // [IsConcurrencyToken] sayesinde EF Core burada DbUpdateConcurrencyException fırlatacak!
        await _context.SaveChangesAsync(cancellationToken);

        // 6. Adım: İşlem başarılı olduysa formatlı rezervasyon numarasını üretip dışarı dönüyoruz
        return $"RES-{year}-{nextNumber:D5}";
      }
      catch (Exception _ex)
      {
        // 7. Adım: Yarış durumu (Race Condition) yakalandı! Başka bir istek araya girip numarayı kapmış.
        _logger.LogWarning(_ex, "Rezervasyon numarası üretilirken çakışma oldu. Deneme: {Attempt}/{MaxRetries}", attempt, MaxRetries);

        // 8. Adım: ÇOK KRİTİK! DbContext'in hafızasını (ChangeTracker) tamamen temizliyoruz. 
        // Temizlemezsek, bir sonraki döngüde EF Core eski/bayat veriyi hafızadan getirmeye çalışır ve döngü sonsuza kadar patlar.
        _context.ChangeTracker.Clear();

        // 9. Adım: Belirlenen deneme hakkı (MaxRetries) dolduysa kullanıcıya anlamlı bir domain hatası fırlatıyoruz
        if (attempt == MaxRetries)
        {
          throw new DomainValidationException("Rezervasyon numarası üretilemedi. Lütfen tekrar deneyin.");
        }

        // 10. Adım: Çakışan diğer isteklerle üst üste binmemek ve sistemi rahatlatmak için kademeli kısa bir bekleme yapıyoruz
        await Task.Delay(50 * attempt, cancellationToken);
      }
    }


    // Döngü normal yollardan çıkmazsa beklenmeyen durum fırlatıyoruz
    throw new InvalidOperationException("Beklenmeyen bir hata oluştu.");
  }
}

/*
  Bu Kod Neden Kusursuz Çalışır? (Adım Adım İnceleme)
    1 - Optimistic Concurrency (İyimser Kilitleme): Tabloya kilit (pessimistic lock) atıp 
        veritabanını yormak yerine, "Önce işlemi yapayım, kaydederken çakışma var mı bakayım" 
        mantığını (IsConcurrencyToken) seçtiğin için performans açısından efsane bir yaklaşım olmuş.

    2 - _context.ChangeTracker.Clear() Hamlesi: Kodundaki en hayati detaylardan biri budur. 
        catch bloğunda ChangeTracker temizlenmezse, EF Core _context nesnesinin hafızasında 
        eski entity state'ini tutmaya devam eder ve sonraki denemede veritabanına gitmeden 
        yine aynı eski veriyi manipüle etmeye çalışarak tekrar patlar. 
        Bunu sıfırlayarak her döngüde taze veri (SELECT) çekilmesini garantilemişsin.

    3 - Kademeli Gecikme (Task.Delay): Çakışma anında 50 * attempt 
        milisaniye bekleyerek aynı anda tekrar saldırmayı engellemişsin (Micro-Jitter mantığı).
*/
