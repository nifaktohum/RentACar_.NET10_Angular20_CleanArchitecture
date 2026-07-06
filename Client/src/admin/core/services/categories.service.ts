import { HttpClient, HttpContext } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';
import { Result } from '../../../core/models/result.model';
import { Category, CategoryRequest } from '../models/category.model';
import { BYPASS_INTERCEPTOR } from '../../../core/interceptors/error.interceptor';

@Injectable({
  providedIn: 'root',
})
export class CategoriesService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/categories`;


  // ==============================================>
  getAll(): Observable<Result<Category[]>> {
    return this.http.get<Result<Category[]>>(`${this.apiUrl}/get-all`, {
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true)
    });
  }

  getById(id: string): Observable<Result<Category>> {
    return this.http.get<Result<Category>>(`${this.apiUrl}/get-by-id/${id}`, {
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true)
    });
  }

  getHierarchy(): Observable<Result<Category[]>> {
    return this.http.get<Result<Category[]>>(`${this.apiUrl}/gethierarchy`, {
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true)
    });
  }

  create(data: CategoryRequest): Observable<Result<Category>> {
    return this.http.post<Result<Category>>(`${this.apiUrl}/create`, data, {
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true)
    });
  }

  update(data: CategoryRequest): Observable<Result<Category>> {
    return this.http.put<Result<Category>>(`${this.apiUrl}/update`, data, {
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true)
    });
  }

  delete(id: string): Observable<Result<void>> {
    return this.http.delete<Result<void>>(`${this.apiUrl}/delete`, { 
      body: { id: id }, 
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true)
      });
  }
  
}
