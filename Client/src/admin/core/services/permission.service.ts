import { computed, inject, Injectable, signal } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient, HttpContext } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Result } from '../../../core/models/result.model';
import { PermissionModel, PermissionResponseDto } from '../models/permission.molel';
import { BYPASS_INTERCEPTOR } from '../../../core/interceptors/error.interceptor';
import { AuthService } from '../../../core/services/auth.service';

@Injectable({
  providedIn: 'root',
})
export class PermissionService {
  private authService = inject(AuthService);
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/Permissions`;

  // Artık sinyali kendimiz tutmuyoruz, AuthService'den okuyoruz!
  userPermissions = computed(() => this.authService.userPermissions());

  // * =========================================>
  // * Sistemdeki kod tarafında (Assembly) taranmış tüm izinleri getirir.
  getAllPermissions(): Observable<Result<string[]>> {
    return this.http.get<Result<string[]>>(`${this.apiUrl}/get-all-permissions`, {
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true)
    });
  }

  //  * İlgili rolün veritabanındaki izin havuzunu getiren sorgu.
  //  * NOT: Eğer backend'de role göre izin getiren özel bir endpoint yazdıysan burayı ona göre eşitle kanka.
  getPermissionsByRoleId(roleId: string): Observable<Result<PermissionModel[]>> {
    return this.http.get<Result<PermissionModel[]>>(`${this.apiUrl}/get-by-role/${roleId}`, {
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true)
    });
  }

  //  * Bir izni AKTİF hale getirir (Aç/Kapa Switch - Açık pozisyonu).
  //  * Backend: [HttpPut("activate/{id:guid}")]
  activatePermission(roleId: string, permissionId: string): Observable<PermissionResponseDto> {
    return this.http.put<PermissionResponseDto>(`${this.apiUrl}/activate`, { roleId, permissionId }, {
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true)
    });
  }

  //  * Bir izni PASİF / Devre dışı hale getirir (Aç/Kapa Switch - Kapalı pozisyonu).
  //  * Backend: [HttpPut("deactivate/{id:guid}")]
  deactivatePermission(roleId: string, permissionId: string): Observable<PermissionResponseDto> {
    return this.http.put<PermissionResponseDto>(`${this.apiUrl}/deactivate`, { roleId, permissionId }, {
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true)
    });
  }

  // ROLE - PERMİSSİON ------------->

  // Yetki kontrolü (Uygulamanın her yerinde kullanacağız)
  hasPermission(permission: string): boolean {
    return this.authService.hasPermission(permission);
  }






}
