import { HttpClient, HttpContext } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { LoginResponse } from '../models/login.response';
import { Result } from '../models/result.model';
import { Observable, tap } from 'rxjs';
import { BYPASS_INTERCEPTOR } from '../interceptors/error.interceptor';
import { TokenPayload } from '../models/tokenPayload.model';
import { TokenService } from './token.service';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private http = inject(HttpClient);
  private readonly apiUrl = environment.apiUrl;
  private tokenService = inject(TokenService);

  // 🔄 Kullanıcı bilgisini reaktif bir Signal olarak saklıyoruz
  currentUser = signal<TokenPayload | null>(null);

  // 🧠 UI tarafında anlık dinlenebilecek computed selector'lar
  isAuthenticated = computed(() => {
    const user = this.currentUser();
    if (!user) return false;

    const token = this.getToken();
    if (!token) return false;

    return !this.isTokenExpired();
  });

  // .NET JWT çıktılarında tekil veri diziye girmeyebilir; Array kontrolü eklendi
  userPermissions = computed(() => {
    const perm = this.currentUser()?.Permission;
    if (!perm) return [];
    return Array.isArray(perm) ? perm : [perm];
  });

  userRoles = computed(() => {
    const role = this.currentUser()?.Role;
    if (!role) return [];
    return Array.isArray(role) ? role : [role];
  });

  isAdmin = computed(() => this.userRoles().includes('Admin'));

  constructor() {
    // Sayfa yenilendiğinde (F5) veya uygulama ilk açıldığında oturumu otomatik yükle
    this.loadSession();
  }

  // ==================== LOGIN ====================
  login(credentials: any, rememberMe: boolean = false) {
    return this.http.post<Result<LoginResponse>>(
      this.apiUrl + '/auth/login',
      credentials,
      { context: new HttpContext().set(BYPASS_INTERCEPTOR, true) }
    ).pipe(
      tap(res => {
        if (res.isSuccessful && res.data) {
          // 1. Access token'ı kaydet
          this.saveToken(res.data.token);

          // 2. Refresh token varsa onu da güvenle sakla
          if (res.data.refreshToken) {
            this.tokenService.setUserRefreshToken(res.data.refreshToken);
          }

          // 3. Kullanıcı oturum bilgilerini (email, fullName, rememberedEmail) kaydet
          this.saveUserSession(res.data, rememberMe);

          // 4. Token'ı decode edip reaktif state'e yaz
          const decoded = this.tokenService.decodeToken(res.data.token);
          if (decoded) {
            this.currentUser.set(decoded);
          }
        }
      })
    );
  }

  // ==================== SESSION ====================
  // Kullanıcı giriş sonrası oturum bilgilerini localStorage'a kaydeder
  saveUserSession(data: LoginResponse, rememberMe: boolean) {
    if (rememberMe) {
      localStorage.setItem('rememberedEmail', data.email);
    } else {
      localStorage.removeItem('rememberedEmail');
    }
    localStorage.setItem('fullName', data.fullName);
    localStorage.setItem('email', data.email);
  }

  // Sayfa yenilendiğinde oturumu otomatik yükler
  loadSession() {
    const token = this.getToken();
    if (token && !this.isTokenExpired()) {
      const decoded = this.getDecodedToken();
      if (decoded) {
        this.currentUser.set(decoded); // Hafızaya fırlattık, her yer reaktif!
        return;
      }
    }
    // ⚠️ Sadece in-memory state'i temizle — refresh token'a DOKUNMA!
    // Böylece access token expired olsa bile refresh token ile yeni token alınabilir.
    this.currentUser.set(null);
  }

  // ==================== TOKEN ====================
  saveToken(token: string) {
    this.tokenService.setUserToken(token);
  }

  getToken(): string | null {
    return this.tokenService.getUserToken();
  }

  getDecodedToken(): TokenPayload | null {
    const token = this.getToken();
    return token ? this.tokenService.decodeToken(token) : null;
  }

  isTokenExpired(): boolean {
    const token = this.getToken();
    return !token || this.tokenService.isTokenExpired(token);
  }

  // ==================== LOGOUT ====================
  logout() {
    this.tokenService.removeUserToken();
    this.tokenService.removeUserRefreshToken();
    localStorage.removeItem('fullName');
    localStorage.removeItem('email');
    // rememberedEmail korunur — bir sonraki login'de otomatik doldurulur
    this.currentUser.set(null);
  }

  // Dışarıdan çağrılan alias — logout ile aynı işi yapar
  clearSession(): void {
    this.logout();
  }

  // ==================== YETKİ KONTROLÜ ====================
  hasPermission(permissionName: string): boolean {
    if (this.isAdmin()) return true;

    const permissions = this.userPermissions();
    if (!permissions.length) return false;

    const target = permissionName.trim().toLowerCase();
    return permissions.some(p => p.trim().toLowerCase() === target);
  }

  // ==================== ŞİFRE SIFIRLAMA ====================
  // Backend'deki 'forgot-password' endpoint'ine kullanıcının e-postasını fırlatır
  forgotPassword(email: string): Observable<any> {
    return this.http.post<any>(
      this.apiUrl + '/auth/forgot-password',
      { email },
      { context: new HttpContext().set(BYPASS_INTERCEPTOR, true) }
    );
  }

  // E-posta, doğrulama kodu ve yeni şifreyi toplu paket halinde backend'e post eder
  resetPassword(resetData: any): Observable<any> {
    return this.http.post<any>(
      this.apiUrl + '/auth/reset-password',
      resetData,
      { context: new HttpContext().set(BYPASS_INTERCEPTOR, true) }
    );
  }

  // 📱 1. AŞAMA: Telefon numarasını backend'e gönderip SMS kodunu tetikler
  smsSendResetCode(phoneNumber: string): Observable<any> {
    return this.http.post<any>(
      this.apiUrl + '/auth/sms-reset-code',
      { phoneNumber },
      { context: new HttpContext().set(BYPASS_INTERCEPTOR, true) }
    );
  }

  // 📱 2. AŞAMA: Telefon, SMS kodu ve yeni şifreyi backend'e post eder
  smsResetPassword(smsResetData: { phoneNumber: string; code: string; newPassword: string }): Observable<any> {
    return this.http.post<any>(
      this.apiUrl + '/auth/sms-reset-password',
      smsResetData,
      { context: new HttpContext().set(BYPASS_INTERCEPTOR, true) }
    );
  }

  // ==================== TÜM CİHAZLARDAN ÇIKIŞ ====================
  logoutAllDevices() {
    return this.http.post<Result<any>>(
      this.apiUrl + '/auth/logout-all-devices',
      {},
      { context: new HttpContext().set(BYPASS_INTERCEPTOR, true) }
    ).pipe(
      tap(res => {
        if (res.isSuccessful) {
          this.logout(); // Tüm cihazlardan çıkınca kendi ekranımızı da uçuralım!
        }
      })
    );
  }
}