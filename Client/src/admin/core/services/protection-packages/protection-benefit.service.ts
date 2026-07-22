import { HttpClient, HttpContext } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../../../environments/environment';
import { Observable } from 'rxjs';
import { Result } from '../../../../core/models/result.model';
import { ProtectionBenefit } from '../../models/protection-package/protection-benefit.model';
import { CreateProtectionBenefitRequest, UpdateProtectionBenefitRequest } from '../../models/protection-package/protection-benefit-request.model';
import { BYPASS_INTERCEPTOR } from '../../../../core/interceptors/error.interceptor';

@Injectable({
  providedIn: 'root',
})
export class ProtectionBenefitService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/ProtectionPackages`;


  //  * Tüm benefit'leri getirir
  getAll(): Observable<Result<ProtectionBenefit[]>> {
    return this.http.get<Result<ProtectionBenefit[]>>(`${this.apiUrl}/get-all-benefits`, {
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true)
    });
  }

  //  * ID'ye göre benefit getirir
  getById(id: string): Observable<Result<ProtectionBenefit>> {
    return this.http.get<Result<ProtectionBenefit>>(`${this.apiUrl}/get-by-id-benefits`, {
      params: { id },
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true)
    });
  }

  //  * Yeni benefit oluşturur
  create(data: CreateProtectionBenefitRequest): Observable<Result<ProtectionBenefit>> {
    return this.http.post<Result<ProtectionBenefit>>(`${this.apiUrl}/create-benefits`, data, {
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true)
    });
  }

  //  * Benefit günceller
  update(data: UpdateProtectionBenefitRequest): Observable<Result<ProtectionBenefit>> {
    return this.http.put<Result<ProtectionBenefit>>(`${this.apiUrl}/update-benefits`, data, {
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true)
    });
  }

  //  * Benefit siler
  delete(id: string): Observable<Result<void>> {
    return this.http.delete<Result<void>>(`${this.apiUrl}/delete-benefits`, {
      body: { id: id }, 
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true)
    });
  }
}
