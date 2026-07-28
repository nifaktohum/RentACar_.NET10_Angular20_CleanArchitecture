import { inject, Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient, HttpContext } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ExtraSummary } from '../models/extra/extraSummary';
import { Result } from '../../../core/models/result.model';
import { Extra } from '../models/extra/extra';
import { CreateExtraRequest } from '../models/extra/createExtraRequest';
import { UpdateExtraRequest } from '../models/extra/updateExtraRequest';
import { BYPASS_INTERCEPTOR } from '../../../core/interceptors/error.interceptor';

@Injectable({
  providedIn: 'root',
})
export class ExtraService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/extra`;


  // ==================================================
  // ==================== QUERY =======================

  //  * Tüm extra'ları getirir (Özet liste)
  getAll(): Observable<Result<ExtraSummary[]>> {
    return this.http.get<Result<ExtraSummary[]>>(`${this.baseUrl}/extra-all`, {
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true)
    });
  }

  //  * ID'ye göre extra getirir (Detay)
  getById(id: string): Observable<Result<Extra>> {
    return this.http.get<Result<Extra>>(`${this.baseUrl}/extra-by-id/${id}`, {
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true)
    });
  }

  // * Kategoriye göre extra'ları getirir
  getByCategory(category: number): Observable<Result<ExtraSummary[]>> {
    return this.http.get<Result<ExtraSummary[]>>(`${this.baseUrl}/extra-by-category/${category}`, {
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true)
    });
  }

  //  * Fiyat tipine göre extra'ları getirir
  getByPriceType(priceType: number): Observable<Result<ExtraSummary[]>> {
    return this.http.get<Result<ExtraSummary[]>>(`${this.baseUrl}/extra-by-priceType/${priceType}`, {
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true)
    });
  }

  //  * Stokta olan extra'ları getirir
  getInStock(): Observable<Result<ExtraSummary[]>> {
    return this.http.get<Result<ExtraSummary[]>>(`${this.baseUrl}/extra-in-stock`, {
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true)
    });
  }


  //  * Önerilen extra'ları getirir
  getRecommended(): Observable<Result<ExtraSummary[]>> {
    return this.http.get<Result<ExtraSummary[]>>(`${this.baseUrl}/extra-recommended`, {
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true)
    });
  }

  // ==================================================
  // ==================== COMMANDS ====================

  //  * Yeni extra oluşturur
  create(data: CreateExtraRequest): Observable<Result<Extra>> {
    return this.http.post<Result<Extra>>(`${this.baseUrl}/extra-create`, data, {
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true)
    });
  }

  //  * Extra günceller
  update(data: UpdateExtraRequest): Observable<Result<Extra>> {
    return this.http.put<Result<Extra>>(`${this.baseUrl}/extra-update`, data, {
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true)
    });
  }

  //  * Extra siler (Soft Delete)
  delete(id: string): Observable<Result<boolean>> {
    return this.http.delete<Result<boolean>>(`${this.baseUrl}/extra-delete/${id}`, {
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true)
    });
  }

  //  * Extra aktif/pasif durumunu değiştirir
  toggleStatus(id: string): Observable<Result<boolean>> {
    return this.http.patch<Result<boolean>>(`${this.baseUrl}/toggle-status/${id}`, {
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true)
    });
  }





}
