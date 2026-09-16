// src/app/core/models/vehicle/vehicle-filter-params.ts

export interface VehicleFilterParams {
  // ============ LİSTE FİLTRELERİ ============
  brands?: string[];
  colors?: string[];
  models?: string[];
  types?: string[];
  fuelTypes?: string[];
  transmissions?: string[];
  years?: string[];
  searchTerms?: string[];

  // ============ TEKİL FİLTRELER ============
  brand?: string;
  model?: string;
  color?: string;
  fuelType?: string;
  transmission?: string;
  year?: string;
  searchTerm?: string;
  sort?: string;

  // ============ İLİŞKİLİ TABLOLAR ============
  vehicleTypeId?: string;  

  // ============ SAYISAL FİLTRELER ============
  minPrice?: number;
  maxPrice?: number;
  seatCounts?: number;
  doorCounts?: number;

  // ============ DURUM FİLTRELERİ ============
  isAvailable?: boolean | null;
  isActive?: boolean;

  // ============ TARİH FİLTRELERİ ============
  startDate?: string;
  endDate?: string;

  // ============ SIRALAMA ============
  sortBy?: string;
  sortDescending?: boolean;

  // ============ SAYFALAMA ============
  pageNumber?: number;
  pageSize?: number;
}