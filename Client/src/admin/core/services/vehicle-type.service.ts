import { HttpClient, HttpContext } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';
import { PagedResult, Result } from '../../../core/models/result.model';
import { CreateVehicleTypeRequest, UpdateVehicleTypeRequest, VehicleType, VehicleTypeDetail, VehicleTypeFilterParams } from '../models/vehicle/vehicle-type.model';
import { BYPASS_INTERCEPTOR } from '../../../core/interceptors/error.interceptor';

@Injectable({
  providedIn: 'root',
})
export class VehicleTypeService {
  private http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/vehicleTypes`;


  getAllType(params?: VehicleTypeFilterParams): Observable<Result<PagedResult<VehicleType>>> {
    return this.http.get<Result<PagedResult<VehicleType>>>(`${this.baseUrl}/get-all`, {
      params: params as any,
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true)
    });
  }

  getById(id: string): Observable<Result<VehicleTypeDetail>> {
    return this.http.get<Result<VehicleTypeDetail>>(`${this.baseUrl}/get-by-id/${id}`, {
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true)
    });
  }

  create(request: CreateVehicleTypeRequest): Observable<Result<VehicleType>> {
    return this.http.post<Result<VehicleType>>(`${this.baseUrl}/create`, { request }, {
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true)
    });
  }

  update(request: UpdateVehicleTypeRequest): Observable<Result<VehicleType>> {
    return this.http.put<Result<VehicleType>>(`${this.baseUrl}/update`, { request }, {
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true)
    });
  }

  delete(id: string): Observable<Result<void>> {
    return this.http.delete<Result<void>>(`${this.baseUrl}/delete/${id}`);
  }

  toggleStatus(id: string): Observable<Result<void>> {
    return this.http.patch<Result<void>>(`${this.baseUrl}/toggle-status/${id}`, {}, {
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true)
    });
  }





}
