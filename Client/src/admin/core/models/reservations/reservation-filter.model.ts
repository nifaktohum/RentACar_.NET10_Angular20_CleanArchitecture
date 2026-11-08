export interface ReservationFilter {
  searchTerm?: string | null;
  status?: string | null;
  customerId?: string | null;
  pageNumber: number;
  pageSize: number;
  sort?: string | null;
}