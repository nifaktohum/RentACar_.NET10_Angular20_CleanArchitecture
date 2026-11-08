import { ReservationStatusEnum } from "../../../../shared/enums/reservation-status";

export interface ReservationUpdateStatus {
  reservationId: string;
  newStatus: ReservationStatusEnum;
  reason?: string | null;
  adminNotes?: string | null;
  paymentId?: string | null;
  actualPickupAt?: string | null;
  actualReturnAt?: string | null;
}