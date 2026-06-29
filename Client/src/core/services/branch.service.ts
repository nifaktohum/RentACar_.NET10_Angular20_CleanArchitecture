import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpContext, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BranchModel, GetBranchesResponse } from '../../admin/core/models/branch.model'; // Kendi klasör yapına göre import edersin kanka
import { environment } from '../../environments/environment';
import { BYPASS_INTERCEPTOR } from '../interceptors/error.interceptor';
import { Result } from '../models/result.model';

@Injectable({
  providedIn: 'root'
})
export class BranchService {
  // 🎯 Modern Angular standardı: inject() kullanımı
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/branches`;

  // Base API URL'ini kendi ortamına (environment) göre ayarlayabilirsin

  

  // 1. Şubeleri Count ile Birlikte Listeleme Servisi
  getBranches(page: number = 1, pageSize: number = 10): Observable<Result<GetBranchesResponse>> {

    // Query string parametrelerini güvenli bir şekilde oluşturuyoruz
    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    // İsteği params nesnesiyle birlikte gönderiyoruz
    return this.http.get<Result<GetBranchesResponse>>(this.apiUrl, {
      params: params,
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true)
    });
  }

  // 2. Yeni Şube Ekleme Servisi (branch-create için)
  createBranch(branchData: any): Observable<any> {
    return this.http.post<any>(this.apiUrl, branchData, {
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true)
    });
  }

  // 3. ID'ye Göre Tek Bir Şube Getirme (branch-edit sayfasında formu doldurmak için)
  getBranchById(id: string): Observable<BranchModel> {
    return this.http.get<BranchModel>(`${this.apiUrl}/${id}`, {
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true)
    });
  }

  // 4. Şube Güncelleme Servisi (branch-edit için)
  updateBranch(id: string, branchData: any): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/${id}`, branchData, {
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true)
    });
  }

  deleteBranchById(id: string): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/${id}`, {
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true)
    });
  }
}