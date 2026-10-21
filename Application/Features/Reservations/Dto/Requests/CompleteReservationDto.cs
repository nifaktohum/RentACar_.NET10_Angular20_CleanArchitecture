namespace Application.Features.Reservations.Dto.Requests;

public sealed record CompleteReservationDto(DateTimeOffset ActualReturnAt);

/*
    - müşterinin kiraladığı aracı şubeye sağ salim teslim ettiği, son kontrollerin 
      (kilometre, yakıt, hasar kontrolü vb.) yapıldığı ve kiralama sözleşmesinin 
      resmi olarak kapatıldığı (Completed / Returned) operasyonda kullanılır.

      Neden ==> ActualReturnAt var? Müşteri aracı normalde salı günü teslim edecektir 
                ama pazartesi akşamı getirmiştir veya trafiktir, işleri uzamıştır çarşamba sabahı getirmiştir. 
                Sistemde fatura kesilirken, gecikme cezası hesaplanırken veya 
                aracın durumu tekrar Available (Müsait) yapılırken bu fiili dönüş zamanı baz alınır.
*/