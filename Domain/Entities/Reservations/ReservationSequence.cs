namespace Domain.Entities.Reservations;

/// Rezervasyon numarası üretimi için yıllık sıra tablosu.
/// Her yıl için son kullanılan numarayı tutar.
/// Örnek: 2025 → RES-2025-00042, 2026 → RES-2026-00001 (yıllık sıfırlanır)
public sealed class ReservationSequence
{
  /// Yıl (PK)
  public int Year { get; private set; }

  /// O yıl içinde en son kullanılan sıra numarası
  public int LastNumber { get; private set; }

  // EF Core için
  private ReservationSequence() { }

  public ReservationSequence(int year)
  {
    if (year < 2000 || year > 2100)
      throw new ArgumentOutOfRangeException(nameof(year), "Geçersiz yıl!");

    Year = year;
    LastNumber = 0;
  }

  /// Bir sonraki sıra numarasını döner ve iç sayacı artırır.
  public int GetNextNumber()
  {
    // O anki LastNumber değeri 1 artırılır (++) ve yeni numara dış dünyaya servis edilir.
    LastNumber++;
    return LastNumber;
  }
}

/*
    Sistemin hangi yılda olduğunu takip eden ve her yeni rezervasyon talebinde bana sorulan 
    yılı baz alarak sayacımı bir adım ileri taşıyan güvenli bir numara üretim merkeziyim. 
    Dışarıdan kural dışı müdahalelere kapalı.
*/