import { HttpClient, HttpContext } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';
import { Result } from '../../../core/models/result.model';
import { CreateRoleModel, RoleModel, UpdateRoleModel } from '../models/Role.model';
import { BYPASS_INTERCEPTOR } from '../../../core/interceptors/error.interceptor';




@Injectable({
  providedIn: 'root',
})
export class RoleService {
  private http = inject(HttpClient);

  private apiUrl = `${environment.apiUrl}/roles`;

  // Varsayılan değeri false olan bir token oluşturuyoruz kanka


  // 1. Tüm Rolleri Getir (Panelde listelemek için)
  getRoles(): Observable<Result<RoleModel[]>> {
    return this.http.get<Result<RoleModel[]>>(`${this.apiUrl}/get-all`, {
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true) 
    });
  }

  // 2. Dinamik Yeni Rol Oluştur (Bizim yazdığımız HttpPost("create") endpoint'i!)
  createRole(request: CreateRoleModel): Observable<Result<string>> {
    return this.http.post<Result<string>>(`${this.apiUrl}/create`, request, {
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true) 
    });
  }

  // 3. Rolü Güncelle (Update handler'ın için kanka)
  updateRole( request: UpdateRoleModel): Observable<Result<void>> {
    return this.http.put<Result<void>>(`${this.apiUrl}/update`, request, { 
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true) 
     });
  }

  // 4. Rolü Sil (Hard delete kontrol sorgusu atmıştık ya kanks)
  deleteRole(id: string): Observable<Result<void>> {
    return this.http.delete<Result<void>>(`${this.apiUrl}/delete`, {
      body: { id: id},
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true) 
    });
  }
}

