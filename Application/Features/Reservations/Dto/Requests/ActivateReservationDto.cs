namespace Application.Features.Reservations.Dto.Requests;

public sealed record ActivateReservationDto(DateTimeOffset ActualPickupAt);


/*
// * müşterinin rezerve ettiği aracı fiziksel olarak teslim alıp kiralama sözleşmesini 
//   resmen başlattığı (Aktif hale getirdiği)

    Yani süreç şöyle işler:
      1. Müşteri günler öncesinden internetten veya telefondan rezervasyon yapar
          (Pending / Beklemede durumundadır).
      2. Müşteri şubeye gelir, araç kontrolden geçer, evraklar imzalanır.
      3. Yetkili veya sistem aracı teslim ederken "Rezervasyonu Şimdi Aktifleştir" butonuna basar.
      4. İşte bu esnada frontend'den backend'e bu DTO fırlatılır.

*/