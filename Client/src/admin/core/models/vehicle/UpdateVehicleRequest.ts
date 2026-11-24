export interface UpdateVehicleRequest {
  id: string;
  brand: string;
  model: string;
  year: string;
  plate: string;
  color: string;
  vehicleModelId: string;
  branchId: string;
  vinNumber: string;
  engineNumber: string;
  fuelType: string;
  transmission: string;
  tractionType: string;
  engineVolume: number;
  enginePower: number;
  fuelConsumption: number;
  seatCount: number;
  doorCount: number;
  minAge: number | null;
  dailyPrice: number;
  depositAmount: number;
  weeklyDiscountRate: number;
  monthlyDiscountRate: number;
  lastMaintenanceKm: number;
  nextMaintenanceKm: number;
  lastMaintenanceDate: string;   // ISO string
  inspectionDate: string;
  insuranceEndDate: string;
  cascoEndDate: string;
  tireStatus: string;
  generalStatus: string;
  description: string | null;
  isActive: boolean;
}