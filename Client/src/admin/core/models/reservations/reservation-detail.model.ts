import { Extra } from "../extra/extra";
import { Reservation } from "./reservation.model";

export interface ReservationDetail extends Reservation {
  customerId: string;
  customerPhone: string;
  customerEmail: string;
  vehicleId: string;
  vehicleBrand: string;
  vehicleModel: string;
  vehiclePlate: string;
  vehicleImageUrl?: string;
  pickupBranchName: string;
  returnBranchName?: string;
  actualPickupAt?: string;
  actualReturnAt?: string;
  dailyPrice: number;
  billablePrice?: number;
  remainingAmount: number;
  fullPaymentPaid: boolean;
  fullPaymentPaidAt?: string;
  fullPaymentId?: string;
  depositLockDeadline?: string;
  confirmedAt?: string;
  completedAt?: string;
  cancelledAt?: string;
  cancellationReason?: string;
  extras: ReservationExtra[];  
  extrasTotalPrice: number;
  notes?: string;
  adminNotes?: string;
  isDepositRefunded?: boolean;
  refundedAmount?: number;
  forfeitedAmount?: number;
}

export interface ReservationExtra {
  extraId: string;
  name: string;
  unitPrice: number;
  quantity: number;
  rentalDayCount: number;
  priceType: string;
  totalPrice: number;
}