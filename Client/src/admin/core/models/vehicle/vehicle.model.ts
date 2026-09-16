import { CreateVehicleRequest } from "./create-vehicle-request";
import { VehicleImage } from "./vehicle-image.model";

export interface Vehicle {
  id: string;
  brand: string;
  model: string;
  year: string;
  plate: string;
  color: string;
  vehicleModelId: string;
  fuelType: string;
  transmission: string;
  seatCount: number;
  doorCount: number;
  minAge: number | null;
  dailyPrice: number;
  description: string | null;
  imageUrl: string | null;       // ⭐ Ana resim URL'i
  isActive: boolean;
  createdAt: string;
  createdBy: string;
  createdByName: string;
  updatedAt: string | null;
  updatedBy: string | null;
  updatedByName: string | null;

  // ⭐ UI için ek alanlar
  images?: VehicleImage[];
}
// export interface UpdateVehicleRequest extends CreateVehicleRequest {
//   id: string;
//   isActive: boolean;
// }