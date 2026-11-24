
export interface CreateReservationExtra {
  extraId: string;
  quantity: number;
}

export interface CreateReservationRequest {
  customerId: string;
  vehicleId: string;
  pickupBranchId: string;
  returnBranchId: string | null;
  plannedStartDate: string;   // ISO string (Date.toISOString())
  plannedEndDate: string;
  notes: string | null;
  extras: CreateReservationExtra[];
}