namespace Application.Features.Reservations.Dto.Requests;

public sealed record CancelReservationDto(string Reason);

/*
     -  müşterinin veya sistemin (örneğin ödeme süresi dolduğu için otomatik olarak) 
        bir rezervasyonu iptal etmesi gerektiği senaryoda kullanılır.

        Neden Reason (Sebep) alanı var? RentACar projelerinde 
        iptal edilen rezervasyonların neden iptal edildiğini bilmek 
        (müşteri vazgeçti, araç arızalandı, Findeks onay vermedi vb.) 
        raporlama, müşteri memnuniyeti ve operasyonel takip için çok önemlidir. 
        Bu yüzden iptal sebebi string olarak dışarıdan alınır.
*/
