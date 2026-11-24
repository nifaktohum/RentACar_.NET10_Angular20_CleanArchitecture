// reservation-filter.model.ts
export interface ReservationFilter {
  pageNumber: number;
  pageSize: number;
  searchTerm?: string | null;
  status?: string | null;
  customerId?: string | null;
  sort?: string | null;

  // 🆕 AKTİF REZERVASYONLAR FİLTRESİ
  activeOnly?: boolean;
  paymentFilter?: string | null;   // 'deposit' | 'full' | 'active' | null
}