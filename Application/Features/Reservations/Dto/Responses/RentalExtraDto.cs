namespace Application.Features.Reservations.Dto.Responses;

public sealed record RentalExtraDto(
    Guid ExtraId,               // Ekstra hizmetin sistemdeki benzersiz ID'si
    string Name,                // Ekstra hizmetin adı (Örn: Bebek Koltuğu, GPS, Ek Sürücü)
    decimal UnitPrice,          // Ekstra hizmetin birim fiyatı
    int Quantity,               // Bu ekstradan kaç adet seçildiğinin bilgisi
    int RentalDayCount,         // Kiralamanın toplam gün sayısı (Günlük hesaplamalarda çarpan olarak kritik önem taşır)
    string PriceType,           // Ücretlendirme tipi: Günlük mü ("Daily") yoksa tek seferlik flat ücret mi ("Rental")
    decimal TotalPrice          // Bu ekstra hizmet için hesaplanan toplam tutar (Birim Fiyat x Adet x Gün Sayısı vb.)
);

/*

  Bu DTO Nerede Kullanılır?
        Rezervasyon Detay (GetReservationDetails) Sayfasında: 
                Müşteri veya yetkili rezervasyonun detayını açtığında, 
                o kiralamaya hangi ekstraların eklendiğini, günlük mü yoksa tek seferlik mi 
                ücretlendirildiğini ve kalemin toplam ne kadar tuttuğunu ekranda listelemek için kullanılır.

        Fatura / Sözleşme Basımında: 
                Fatura kesilirken ekstraların kalem kalem dökümünü göstermek için 
                backend'den frontend'e taşınan veri paketidir.

*/