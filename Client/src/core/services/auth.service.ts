import { HttpClient, HttpContext } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { LoginResponse } from '../models/login.response';
import { Result } from '../models/result.model';
import { Observable, tap } from 'rxjs';
import { CurrentModel } from '../models/current.model';
import { jwtDecode } from 'jwt-decode';
import { BYPASS_INTERCEPTOR } from '../interceptors/error.interceptor';
import { TokenPayload } from '../models/tokenPayload.model';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private http = inject(HttpClient);
  private readonly apiUrl = environment.apiUrl;

  // 🔄 Kullanıcı bilgisini reaktif bir Signal olarak saklıyoruz kanka!
  currentUser = signal<TokenPayload | null>(null);
  // 🧠 UI tarafında anlık dinlenebilecek akıllı computed selector'lar
  isAuthenticated = computed(() => {
    const user = this.currentUser();
    if (!user) return false;

    // Token var mı kontrol et
    const token = this.getToken();
    if (!token) return false;

    // Token süresi dolmuş mu kontrol et
    return !this.isTokenExpired();
  });
  userPermissions = computed(() => this.currentUser()?.Permission || []);
  isAdmin = computed(() => this.userRoles().includes('Admin'));

  userRoles = computed(() => {
    const role = this.currentUser()?.Role;
    if (!role) return [];
    return Array.isArray(role) ? role : [role];
  });

  constructor() {
    // Sayfa yenilendiğinde (F5) veya uygulama ilk açıldığında oturumu otomatik yükle
    this.loadSession();
  }

  login(credentials: any) {
    // post metodunda 2. parametre body, 3. parametre options (context dahil) olmalı
    return this.http.post<Result<LoginResponse>>(this.apiUrl + '/auth/login', credentials,
      { context: new HttpContext().set(BYPASS_INTERCEPTOR, true) }).pipe(
        tap(res => {
          if (res.isSuccessful && res.data) {
            this.saveToken(res.data.token);

            const decoded = jwtDecode<TokenPayload>(res.data.token);
            this.currentUser.set(decoded);
          }
        })
      );
  }
  
  // Bu kod, kullanıcı giriş işlemini başarıyla tamamladıktan sonra, 
  // oturum bilgilerini tarayıcının yerel hafızasına(localStorage) kalıcı olarak kaydetmekten sorumludur.
  saveUserSession(data: LoginResponse, rememberMe: boolean) {
    if (rememberMe) {
      localStorage.setItem('rememberedEmail', data.email);
    } else {
      localStorage.removeItem('rememberedEmail');
    }
    localStorage.setItem('fullName', data.fullName);
    localStorage.setItem('email', data.email);
    // Token kaydını zaten yapıyorsun
    // this.saveToken(data.token);
    
  }

  loadSession() {
    const token = this.getToken();
    if (token && !this.isTokenExpired()) {
      const decoded = this.getDecodedToken();
      if (decoded) {
        this.currentUser.set(decoded); // Hafızaya fırlattık, her yer reaktif!
        return;
      }
    }
    this.logout(); // Token geçersiz veya yoksa tertemiz et hafızayı
  }

  saveToken(token: string) {
    localStorage.setItem('token', token);
  };

  getToken(): string | null {
    // İhtiyaç anında yerel hafızaya gidip kaydedilmiş olan token'ı geri okuyoruz.
    return localStorage.getItem('token');
  };

  getDecodedToken(): TokenPayload | null {
    const token = this.getToken();
    if (!token) return null;
    try {
      return jwtDecode<TokenPayload>(token);
    } catch {
      return null;
    }
  }

  logout() {
    // Sadece auth ile ilgili olanları temizle
    localStorage.removeItem('token');
    localStorage.removeItem('fullName');
    localStorage.removeItem('email');

    // 🔥 'rememberedEmail' KULLANICI İSTEĞİYLE KORUNUR
    // localStorage.removeItem('rememberedEmail'); // ❌ BUNU YAPMA

    this.currentUser.set(null);
  }

  // // AuthService sınıfının içine eklenecek olan süre kontrol metodu:
  isTokenExpired(): boolean {
    const token = this.getToken();
    if (!token) {
      return true;
    }

    try {
      const decoded = this.getDecodedToken();
      if (!decoded || !decoded.exp) {
        return true;
      }

      const expiryTime = decoded.exp * 1000; // Saniyeyi JS'e uygun milisaniyeye çevirdik
      const currentTime = Date.now();

      return currentTime > expiryTime; // Süre bittiyse true fırlatır kanka
    } catch {
      return true;
    }
  }

  // 🛡️ İZİN KONTROLÜ: Artık localStorage karmaşasından değil, direkt token içindeki diziden okuyoruz Joe!
  hasPermission(permissionName: string): boolean {
    if (this.userRoles().includes('Admin')) return true;

    const permissions = this.userPermissions();
    // includes() büyük/küçük harfe duyarlıdır, .trim() ekleyerek riski azaltın
    return permissions ? permissions.some(p => p.trim() === permissionName.trim()) : false;
  }
  // // Backend'deki 'forgot-password' endpoint'ine kullanıcının e-postasını fırlatır kanka
  forgotPassword(email: string): Observable<any> {
    // // Projedeki base API url düzenine göre (/api/auth/forgot-password) post isteği atıyoruz
    return this.http.post<any>(this.apiUrl + '/auth/forgot-password', { email }, {
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true)
    });
  }

  // // E-posta, 5 haneli doğrulama kodu ve yeni şifreyi toplu paket halinde backend'e post eder kanka
  resetPassword(resetData: any): Observable<any> {
    // // resetData nesnesi içerik olarak { email, code, newPassword } taşır kanka
    return this.http.post<any>(this.apiUrl + '/auth/reset-password', resetData, {
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true)
    });
  }

  // // 📱 1. AŞAMA: Telefon numarasını backend'deki 'sms-reset-code' endpoint'ine gönderip Twilio'yu tetikler kanka
  smsSendResetCode(phoneNumber: string): Observable<any> {
    // // Backend'deki SmsResetCodeCommand nesnesinin beklediği 'phoneNumber' gövdesini hazırlıyoruz
    return this.http.post<any>(this.apiUrl + '/auth/sms-reset-code', { phoneNumber }, {
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true)
    });
  }

  // // 📱 2. AŞAMA: Telefon numarası, SMS ile gelen 6 haneli kod ve yeni şifreyi backend'e post eder kanka
  smsResetPassword(smsResetData: { phoneNumber: string, code: string, newPassword: string }): Observable<any> {
    // // smsResetData nesnesi tam olarak backend'deki SmsResetPasswordCommand rekoru ile birebir eşleşir kanka
    return this.http.post<any>(this.apiUrl + '/auth/sms-reset-password', smsResetData, {
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true)
    });
  }

  // // Tüm cihazlardan çıkış
  logoutAllDevices() {
    return this.http.post<Result<any>>(this.apiUrl + '/auth/logout-all-devices', {}, {
      context: new HttpContext().set(BYPASS_INTERCEPTOR, true)
    }).pipe(
      tap(res => {
        if (res.isSuccessful) {
          this.logout(); // Tüm cihazlardan çıkınca kendi ekranımızı da uçuralım!
        }
      })
    );
  }
  
  // uygulamanın "Güvenli Çıkış"(Logout) veya "Oturum Sonlandırma" mekanizmasının merkezidir.
  clearSession(): void {
    // Sadece auth ile ilgili olanları temizle
    localStorage.removeItem('token');
    localStorage.removeItem('fullName');
    localStorage.removeItem('email');
    localStorage.removeItem('refreshToken'); // Varsa

    // 🔥 'rememberedEmail' KULLANICI İSTEĞİYLE KORUNUR - SİLME!
    // localStorage.removeItem('rememberedEmail'); // ❌ BUNU YAPMA

    // Signal'leri temizle
    this.currentUser.set(null);
  }

}
