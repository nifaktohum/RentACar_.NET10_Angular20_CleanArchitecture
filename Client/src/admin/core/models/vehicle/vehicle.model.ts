import { VehicleImage } from "./vehicle-image.model";

export interface Vehicle {
  id: string;
  // Temel
  brand: string;
  model: string;
  year: string;
  plate: string;
  color: string;
  vehicleModelId: string;
  vehicleModelName?: string;        
  vehicleTypeName?: string;         
  branchId: string;                 
  vinNumber: string;                
  engineNumber: string;             
  // Teknik
  fuelType: string;
  transmission: string;
  tractionType: string;             
  engineVolume: number;             
  enginePower: number;              
  fuelConsumption: number;          
  seatCount: number;
  doorCount: number;
  // Ticari
  minAge: number | null;
  dailyPrice: number;
  depositAmount: number;            
  weeklyDiscountRate: number;       
  monthlyDiscountRate: number;      
  // Bakım & Durum
  currentKilometer: number;         
  lastMaintenanceKm: number;        
  nextMaintenanceKm: number;        
  lastMaintenanceDate: string;      
  inspectionDate: string;           
  insuranceEndDate: string;         
  cascoEndDate: string;             
  tireStatus: string;               
  generalStatus: string;            
  status: string;                   
  // Diğer
  isAvailable: boolean;
  description: string | null;
  mainImageUrl: string | null;      // ⬅️ imageUrl yerine mainImageUrl
  isActive: boolean;
  createdAt: string;
  createdBy: string;
  createdByName: string;
  updatedAt: string | null;
  updatedBy: string | null;
  updatedByName: string | null;

  // UI için
  images?: VehicleImage[];
}