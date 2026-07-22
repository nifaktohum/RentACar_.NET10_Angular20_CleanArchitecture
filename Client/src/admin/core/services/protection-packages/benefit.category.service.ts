import { HttpClient, HttpContext } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../../../environments/environment';
import { Observable } from 'rxjs';
import { Result } from '../../../../core/models/result.model';
import { BenefitCategory } from '../../models/protection-package/benefit-category/benefit-category.model';
import { CreateBenefitCategoryRequest } from '../../models/protection-package/benefit-category/CreateBenefitCategoryRequest';
import { UpdateBenefitCategoryRequest } from '../../models/protection-package/benefit-category/UpdateBenefitCategoryRequest';
import { BYPASS_INTERCEPTOR } from '../../../../core/interceptors/error.interceptor';

@Injectable({
  providedIn: 'root',
})
export class BenefitCategoryService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/BenefitCategories`;

  getAll(): Observable<Result<BenefitCategory[]>> {
    return this.http.get<Result<BenefitCategory[]>>(`${this.apiUrl}/benefit-category-get-all`, {
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true)
    });
  }

  getById(id: string): Observable<Result<BenefitCategory>> {
    return this.http.get<Result<BenefitCategory>>(`${this.apiUrl}/benefit-category-get-id/${id}`, {
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true)
    });
  }

  create(data: CreateBenefitCategoryRequest): Observable<Result<BenefitCategory>> {
    return this.http.post<Result<BenefitCategory>>(`${this.apiUrl}/benefit-category-create`, data, {
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true)
    });
  }

  update(data: UpdateBenefitCategoryRequest): Observable<Result<BenefitCategory>> {
    return this.http.put<Result<BenefitCategory>>(`${this.apiUrl}/benefit-category-update`, data, {
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true)
    });
  }

  delete(id: string): Observable<Result<void>> {
    return this.http.delete<Result<void>>(`${this.apiUrl}/benefit-category-delete/${id}`, {
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true)
    });
  }



}
