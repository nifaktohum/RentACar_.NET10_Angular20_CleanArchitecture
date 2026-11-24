export interface ReservationStats {
  totalCount: number;
  pendingPaymentCount: number;
  confirmedCount: number;
  depositOnlyCount: number;      // 🆕
  fullPaidCount: number;         // 🆕
  activeCount: number;
  completedCount: number;
  expiredCount: number;
  noShowCount: number;
  cancelledCount: number;
}