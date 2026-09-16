export interface VehicleModel {
  id: string;
  brand: string;
  name: string;
  description: string | null;
  stock: number;
  availableStock: number;
  isInStock: boolean;
  vehicleTypeId: string;
  vehicleTypeName: string;
  isActive: boolean;
  createdAt: string;
}

export interface VehicleModelDetail extends VehicleModel {
  createdBy: string;
  createdByUserName: string;
  updatedAt: string | null;
  updatedBy: string | null;
  updatedByUserName: string | null;
  deletedAt: string | null;
  deletedBy: string | null;
  deletedByUserName: string | null;
}

export interface CreateVehicleModelRequest {
  brand: string;
  name: string;
  description?: string | null;
  stock: number;
  vehicleTypeId: string;
}

export interface UpdateVehicleModelRequest {
  id: string;
  brand: string;
  name: string;
  description?: string | null;
  stock: number;
  vehicleTypeId: string;
}

export interface VehicleModelFilterParams {
  pageNumber?: number;
  pageSize?: number;
  searchTerm?: string;
  brands?: string[];
  models?: string[];
  vehicleTypeIds?: string[];
  isInStock?: boolean | null;
  minStock?: number | null;
  maxStock?: number | null;
  sort?: string | null;
}