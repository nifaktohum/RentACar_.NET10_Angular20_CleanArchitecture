export interface UpdateVehicleRequest {
  id: string;
  vehicleModelId: string;
  brand: string;
  model: string;
  year: string;
  plate: string;
  color: string;
  fuelType: string;
  transmission: string;
  seatCount: number;
  doorCount: number;
  minAge: number | null;
  dailyPrice: number;
  description: string | null;
  isActive: boolean;
  // ⭐ imageFiles YOK!
}