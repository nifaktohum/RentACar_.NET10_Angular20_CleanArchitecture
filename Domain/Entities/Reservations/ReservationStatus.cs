namespace Domain.Entities.Reservations;

/// Rezervasyonun yaşam döngüsündeki durumunu belirtir.
public enum ReservationStatus
{
  // ==================== AKTİF SÜREÇ ====================
  
  PendingPayment = 1,   /// Kapora bekleniyor (24 saat)
  Confirmed = 2,        /// Kapora alındı, araç kilitli, teslim bekleniyor (24 saat)
  Active = 3,           /// Araç teslim edildi, kiralama başladı
  Completed = 4,        /// Araç iade edildi

  // ==================== KÖTÜ SONUÇLAR ====================
  
  Expired = 5,          /// 24 saat içinde kapora gelmedi
  NoShow = 6,           /// 24 saat içinde teslim alınmadı (kapora yandı)
  Cancelled = 7         /// Müşteri iptal etti
}