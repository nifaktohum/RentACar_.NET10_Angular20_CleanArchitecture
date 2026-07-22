import { inject, Injectable } from '@angular/core';
import { environment } from '../../../../environments/environment';
import { HttpClient, HttpContext, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Result } from '../../../../core/models/result.model';
import { ProtectionPackage } from '../../models/protection-package/protection-package.model';
import { CreateProtectionPackageRequest, UpdateProtectionPackageRequest } from '../../models/protection-package/protection-package-request.model';
import { BYPASS_INTERCEPTOR } from '../../../../core/interceptors/error.interceptor';

@Injectable({
  providedIn: 'root',
})
export class ProtectionPackageService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/ProtectionPackages`;

  // * Tüm paketleri getir.
  getAll(onlyActive?: boolean, onlyRecommended?: boolean): Observable<Result<ProtectionPackage[]>> {
    let params = new HttpParams();
    if (onlyActive !== undefined) params = params.set('onlyActive', onlyActive);
    if (onlyRecommended !== undefined) params = params.set('onlyRecommended', onlyRecommended);

    return this.http.get<Result<ProtectionPackage[]>>(`${this.apiUrl}/get-all-packages`, {
      params,
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true)
    },);
  }
  // * ID'ye göre paket getirir
  getById(id: string): Observable<Result<ProtectionPackage>> {
    return this.http.get<Result<ProtectionPackage>>(`${this.apiUrl}/get-by-id-packages`, {
      params: { id },
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true)
    });
  }

  // * Önerilen paketleri getirir
  getRecommended(): Observable<Result<ProtectionPackage[]>> {
    return this.http.get<Result<ProtectionPackage[]>>(`${this.apiUrl}/get-recommended`, {
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true)
    });
  }

  // * Yeni paket oluşturur
  create(data: CreateProtectionPackageRequest): Observable<Result<ProtectionPackage>> {
    return this.http.post<Result<ProtectionPackage>>(`${this.apiUrl}/create-packages`, data, {
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true)
    });
  }

  // * Paket günceller
  update(data: UpdateProtectionPackageRequest): Observable<Result<ProtectionPackage>> {
    return this.http.put<Result<ProtectionPackage>>(`${this.apiUrl}/update-packages`, data, {
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true)
    });
  }

  // * Paket siler (Soft Delete)
  delete(id: string): Observable<Result<void>> {
    return this.http.delete<Result<void>>(`${this.apiUrl}/delete-packages/${id}`, {
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true)
    });
  }
  // * Paket aktif/pasif durumunu değiştirir
  toggleStatus(id: string, isActive: boolean): Observable<Result<ProtectionPackage>> {
    // Önce paketi getir, sonra güncelle
    return this.http.patch<Result<ProtectionPackage>>(`${this.apiUrl}/update-isactive/${id}/${isActive}`, {
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true)
    });
  }
}
