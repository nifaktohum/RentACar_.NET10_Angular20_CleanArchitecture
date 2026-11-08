export interface Reservation {
  id: string;
  reservationNumber: string;
  customerFullName: string;
  vehicleInfo: string;
  plannedStartDate: string;
  plannedEndDate: string;
  plannedDays: number;
  status: string;
  totalPrice: number;
  depositAmount: number;
  depositPaid: boolean;
  createdAt: string;
}