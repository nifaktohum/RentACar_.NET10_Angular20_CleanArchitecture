import { ReservationStatusEnum } from "../../../../shared/enums/reservation-status";

export interface ReservationUpdateStatus {
  reservationId: string;
  newStatus: ReservationStatusEnum;
  reason?: string | null;
  adminNotes?: string | null;
  paymentId?: string | null;       // Depozito ID
  fullPaymentId?: string | null;   // 🚨 Tam ödeme ID (YENİ)
  actualPickupAt?: string | null;
  actualReturnAt?: string | null;
  returnKilometer?: number | null; // 🚨 İade Km (YENİ)
}