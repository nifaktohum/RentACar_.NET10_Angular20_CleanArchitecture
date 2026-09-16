export interface CreateVehicleRequest {
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
  imageFile: File | null;   // ⭐ TEK resim
}