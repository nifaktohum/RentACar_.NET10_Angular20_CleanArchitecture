import { Booleanish } from "primeng/ts-helpers";

export interface VehicleType {
  id: string;
  name: string;
  description: string | null;
  icon: string | null;
  displayOrder: number;
  isActive: boolean;
  vehicleCount?: number;
}

// core/models/vehicle/vehicle-type.model.ts

export interface VehicleTypeDetail {
  id: string;
  name: string;
  description: string | null;       // ✅ null olabilir
  icon: string | null;              // ✅ null olabilir
  displayOrder: number;
  isActive: boolean;
  vehicleModelCount: number;
  createdAt: string;
  createdBy: string;
  createdByName: string | null;     // ✅ null olabilir
  updatedAt: string | null;         // ✅ null olabilir
  updatedBy: string | null;         // ✅ null olabilir
  updatedByName: string | null;     // ✅ null olabilir
  deletedAt: string | null;         // ✅ Ekle
  deletedBy: string | null;         // ✅ Ekle
  deletedByName: string | null;     // ✅ Ekle
}

export interface CreateVehicleTypeRequest {
  name: string;
  description?: string | null;
  icon?: string | null;
  displayOrder: number;
}

export interface UpdateVehicleTypeRequest {
  id: string;
  name: string;
  description?: string | null;
  icon?: string | null;
  displayOrder: number;
}

export interface VehicleTypeFilterParams {
  // ============ SAYFALAMA ============
  pageNumber?: number;
  pageSize?: number;
}