import { HttpClient, HttpContext, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';
import { PagedResult, Result } from '../../../core/models/result.model';
import { CreateVehicleModelRequest, UpdateVehicleModelRequest, VehicleModel, VehicleModelDetail, VehicleModelFilterParams } from '../models/vehicle/VehicleModel.model';
import { BYPASS_INTERCEPTOR } from '../../../core/interceptors/error.interceptor';

@Injectable({
  providedIn: 'root',
})
export class VehicleModelService {
  private http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/vehicleModels`;

  getAll(params?: VehicleModelFilterParams): Observable<Result<PagedResult<VehicleModel>>> {
    let httpParams = new HttpParams();

    if (params) {
      if (params.pageNumber != null) httpParams = httpParams.set('pageNumber', params.pageNumber.toString());
      if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());
      if (params.searchTerm) httpParams = httpParams.set('searchTerm', params.searchTerm);
      if (params.sort) httpParams = httpParams.set('sort', params.sort);

      // Çoklu brand
      if (params.brands?.length) {
        params.brands.forEach(b => {
          httpParams = httpParams.append('brands', b);
        });
      }

      // Çoklu model
      if (params.models?.length) {
        params.models.forEach(m => {
          httpParams = httpParams.append('models', m);
        });
      }

      // Çoklu vehicleTypeId
      if (params.vehicleTypeIds?.length) {
        params.vehicleTypeIds.forEach(id => {
          httpParams = httpParams.append('vehicleTypeIds', id);
        });
      }

      // Stok filtresi
      if (params.isInStock !== null && params.isInStock !== undefined) {
        httpParams = httpParams.set('isInStock', params.isInStock.toString());
      }
      if (params.minStock !== null && params.minStock !== undefined) {
        httpParams = httpParams.set('minStock', params.minStock.toString());
      }
      if (params.maxStock !== null && params.maxStock !== undefined) {
        httpParams = httpParams.set('maxStock', params.maxStock.toString());
      }
    }

    return this.http.get<Result<PagedResult<VehicleModel>>>(`${this.baseUrl}/get-all`, {
      params: httpParams
    });
  }


  getById(id: string): Observable<Result<VehicleModelDetail>> {
    return this.http.get<Result<VehicleModelDetail>>(`${this.baseUrl}/get-by-id/${id}`);
  }

  create(request: CreateVehicleModelRequest): Observable<Result<VehicleModel>> {
    return this.http.post<Result<VehicleModel>>(`${this.baseUrl}/create`, request);
  }

  update(request: UpdateVehicleModelRequest): Observable<Result<VehicleModel>> {
    return this.http.put<Result<VehicleModel>>(`${this.baseUrl}/update`, request);
  }

  delete(id: string): Observable<Result<void>> {
    return this.http.delete<Result<void>>(`${this.baseUrl}/delete/${id}`, {
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true)
    });
  }

  toggleStatus(id: string): Observable<Result<void>> {
    return this.http.patch<Result<void>>(`${this.baseUrl}/toggle-status/${id}`, {});
  }

  getBrands(): Observable<Result<string[]>> {
    return this.http.get<Result<string[]>>(`${this.baseUrl}/brands`);
  }
}
