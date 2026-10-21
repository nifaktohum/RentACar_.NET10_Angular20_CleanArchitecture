namespace Application.Features.Reservations.Dto.Requests;

// Sanal POS (Iyzico, banka entegrasyonu vb.) üzerinden başarılı ödeme sonrasında dönen benzersiz işlem veya ödeme referans numarası
public sealed record PayDepositDto(string PaymentId);

/*
  Bu DTO Nerede Kullanılır?
        Kapora Ödemesi: Müşteri rezervasyon yaptığında aracı kesinleştirmek için ön ödeme (kapora) yapması gerekir. 
                        Ödeme aracı kuruluşta (örn. Iyzico) tamamlandığında, 
                        banka bize bir PaymentId (işlem numarası) verir.

        Backend'e Bildirim: Frontend, bu PaymentId'yi backend'e fırlatarak "Bu rezervasyonun kapora ödemesi başarıyla alındı, 
                            işleme onay verebilirsin" der. Backend de bu ID ile ödemeyi doğrular ve rezervasyonun 
                            durumunu Confirmed (Onaylandı) seviyesine çeker.
*/
