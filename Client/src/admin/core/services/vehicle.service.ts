import { HttpClient, HttpContext, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { VehicleFilterParams } from '../models/vehicle/vehicle-filter-params';
import { Observable } from 'rxjs';
import { PagedResult, Result } from '../../../core/models/result.model';
import { CreateVehicleRequest } from '../models/vehicle/create-vehicle-request';
import { VehicleType } from '../models/vehicle/vehicle-type.model';
import { BYPASS_INTERCEPTOR } from '../../../core/interceptors/error.interceptor';
import { Vehicle } from '../models/vehicle/vehicle.model';
import { UpdateVehicleRequest } from '../models/vehicle/UpdateVehicleRequest';

@Injectable({
  providedIn: 'root',
})
  
export class VehicleService {
  private http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/vehicles`;

  // ==================== GET ALL ====================
  getAll(params?: VehicleFilterParams): Observable<Result<PagedResult<Vehicle>>> {
    let httpParams = new HttpParams();

    if (params) {
      if (params.pageNumber) httpParams = httpParams.set('pageNumber', params.pageNumber.toString());
      if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());
      if (params.searchTerm) httpParams = httpParams.set('searchTerm', params.searchTerm);
      if (params.sort) httpParams = httpParams.set('sort', params.sort);
      if (params.isAvailable !== null && params.isAvailable !== undefined) {
        httpParams = httpParams.set('isAvailable', params.isAvailable.toString());
      }

      if (params.brands?.length) {
        params.brands.forEach(b => httpParams = httpParams.append('brands', b));
      }
      if (params.models?.length) {
        params.models.forEach(m => httpParams = httpParams.append('models', m));
      }
      if (params.types?.length) {
        params.types.forEach(t => httpParams = httpParams.append('types', t));
      }
    }

    return this.http.get<Result<PagedResult<Vehicle>>>(`${this.baseUrl}/get-all`, {
      params: httpParams,
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true)
    });
  }

  // ==================== GET BY ID ====================
  getById(id: string): Observable<Result<Vehicle>> {
    return this.http.get<Result<Vehicle>>(`${this.baseUrl}/Get-By-Id/${id}`);
  }

  // ==================== CREATE (MULTIPART, TEK RESİM) ====================
  create(request: CreateVehicleRequest): Observable<Result<Vehicle>> {
    const formData = new FormData();

    // ⭐ Metin alanları
    formData.append('VehicleModelId', request.vehicleModelId);
    formData.append('Brand', request.brand);
    formData.append('Model', request.model);
    formData.append('Year', request.year);
    formData.append('Plate', request.plate);
    formData.append('Color', request.color);
    formData.append('FuelType', request.fuelType);
    formData.append('Transmission', request.transmission);
    formData.append('SeatCount', request.seatCount.toString());
    formData.append('DoorCount', request.doorCount.toString());
    formData.append('DailyPrice', request.dailyPrice.toString());

    if (request.minAge !== null) {
      formData.append('MinAge', request.minAge.toString());
    }

    if (request.description) {
      formData.append('Description', request.description);
    }

    // ⭐ TEK resim (vitrin)
    if (request.imageFile) {
      formData.append('ImageFile', request.imageFile, request.imageFile.name);
    }

    return this.http.post<Result<Vehicle>>(`${this.baseUrl}/create`, formData);
  }

  // ==================== UPDATE (JSON, RESİM YOK) ====================
  update(request: UpdateVehicleRequest): Observable<Result<Vehicle>> {
    return this.http.put<Result<Vehicle>>(`${this.baseUrl}/update`, request);
  }

  // ==================== DELETE ====================
  delete(id: string): Observable<Result<void>> {
    return this.http.delete<Result<void>>(`${this.baseUrl}/delete/${id}`);
  }

  // ==================== TOGGLE STATUS ====================
  toggleStatus(id: string): Observable<Result<void>> {
    return this.http.patch<Result<void>>(`${this.baseUrl}/toggle-status/${id}`, {});
  }

  // ==================== DISTINCT ====================
  getBrandsDistinct(): Observable<Result<string[]>> {
    return this.http.get<Result<string[]>>(`${this.baseUrl}/brand-distinct`);
  }

  getModelsDistinct(): Observable<Result<string[]>> {
    return this.http.get<Result<string[]>>(`${this.baseUrl}/model-distinct`);
  }

  getTypesDistinct(): Observable<Result<string[]>> {
    return this.http.get<Result<string[]>>(`${this.baseUrl}/type-distinct`);
  }

  getFuelTypeDistinct(): Observable<Result<string[]>> {
    return this.http.get<Result<string[]>>(`${this.baseUrl}/fuel-type-distinct`);
  }

  getTransmissionDistinct(): Observable<Result<string[]>> {
    return this.http.get<Result<string[]>>(`${this.baseUrl}/transmission-distinct`);
  }

  getColorDistinct(): Observable<Result<string[]>> {
    return this.http.get<Result<string[]>>(`${this.baseUrl}/color-distinct`);
  }
}
