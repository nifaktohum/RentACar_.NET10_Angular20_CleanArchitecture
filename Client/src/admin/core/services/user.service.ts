import { HttpClient, HttpContext } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';
import { Result } from '../../../core/models/result.model';
import { UserModel } from '../models/user.model';
import { BYPASS_INTERCEPTOR } from '../../../core/interceptors/error.interceptor';
import { CreateUserRequest } from '../models/createUser.moder';
import { UpdateUserRequest } from '../models/user.model';
import { UserDetail } from '../models/detailUser.model';

@Injectable({
  providedIn: 'root',
})
export class UserService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/users`;

  // 1. Kullanıcıları Listele
  getUsers(): Observable<Result<UserModel[]>> {
    return this.http.get<Result<UserModel[]>>(`${this.apiUrl}/get-all`, {
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true)
    });
  }

  // 2. Kullanıcı Detay Getir (Update öncesi veya profil için)
  getUserById(id: string): Observable<Result<UserDetail>> {
    return this.http.get<Result<UserDetail>>(`${this.apiUrl}/get-by-id/${id}`, {
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true)
    });
  }

  // 5. Yeni Kullanıcı Oluştur
  createUser(request: CreateUserRequest): Observable<Result<UserModel>> {
    return this.http.post<Result<UserModel>>(`${this.apiUrl}/create`, request, {
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true)
    });
  }

  // 3. Kullanıcı Güncelle (Handler'ındaki UpdateUserCommand ile tam uyumlu!)
  updateUser(request: UpdateUserRequest): Observable<Result<UserModel>> {
    return this.http.put<Result<UserModel>>(`${this.apiUrl}/update`, request, {
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true)
    });
  }

  // 4. Kullanıcı Sil
  deleteUser(id: string): Observable<Result<void>> {
    return this.http.delete<Result<void>>(`${this.apiUrl}/delete`, {
      body: { id: id },
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true)
    });
  }
}
