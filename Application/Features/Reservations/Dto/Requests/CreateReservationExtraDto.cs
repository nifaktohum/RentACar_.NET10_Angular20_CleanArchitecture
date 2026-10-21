namespace Application.Features.Reservations.Dto.Requests;

public sealed record CreateReservationExtraDto(
                        Guid ExtraId,   // Seçilen ekstra hizmetin (bebek koltuğu, GPS, ek sigorta vb.) sistemdeki benzersiz ID'si
                        int Quantity    // Bu ekstra hizmetten kaç adet veya kaç birim istenildiğinin bilgisi
                      );
