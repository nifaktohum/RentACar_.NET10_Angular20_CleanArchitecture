import { HttpClient, HttpContext } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { TokenService } from '../token.service';
import { environment } from '../../../environments/environment';
import { TokenPayload } from '../../models/tokenPayload.model';
import { Result } from '../../models/result.model';
import { Observable, tap } from 'rxjs';
import { BYPASS_INTERCEPTOR } from '../../interceptors/error.interceptor';
import { CustomerLoginResponse } from '../../models/customer-auth.model';
import { TokenType, withAuthOptions } from '../../interceptors/token-context';

@Injectable({
  providedIn: 'root',
})
export class CustomerAuthService {
  private http = inject(HttpClient);
  private tokenService = inject(TokenService);
  private readonly apiUrl = environment.apiUrl + '/CustomersAuth';


  // 🔄 Müşteri state
  currentCustomer = signal<TokenPayload | null>(null);

  isAuthenticated = computed(() => {
    const customer = this.currentCustomer();

    if (!customer) return false;
    const token = this.tokenService.getCustomerToken();

    if (!token) return false;

    return !this.tokenService.isTokenExpired(token);
  });

  customerType = computed(() => this.currentCustomer()?.['CustomerType'] || null);
  isIndividual = computed(() => this.customerType() === 'Individual');
  isCorporate = computed(() => this.customerType() === 'Corporate');

  // Müşterinin e-posta doğrulama durumu (true / false)
  isEmailVerified = computed(() => this.currentCustomer()?.IsEmailVerified === 'True');

  // Müşterinin telefon doğrulama durumu (true / false)
  isPhoneVerified = computed(() => this.currentCustomer()?.IsPhoneVerified === 'True');

  // Müşterinin kara listede olup olmadığı bilgisi (true / false)
  isBlacklisted = computed(() => this.currentCustomer()?.IsBlacklisted === 'True');

  constructor() {
    this.loadSession();
  }

  // ==================== LOGIN ====================
  login(credentials: { email: string; password: string; rememberMe: boolean }) {
    return this.http.post<Result<CustomerLoginResponse>>(
      this.apiUrl + '/customer-login',
      credentials,
      withAuthOptions(TokenType.None, true)  
    ).pipe(
      tap(res => {
        if (res.isSuccessful && res.data) {
          this.tokenService.setCustomerToken(res.data.token);
          this.tokenService.setCustomerRefreshToken(res.data.refreshToken);

          const decoded = this.tokenService.decodeToken(res.data.token);
          this.currentCustomer.set(decoded);

          // Extra bilgi sakla
          localStorage.setItem('customer_fullName', res.data.fullName);
          localStorage.setItem('customer_email', res.data.email);
          localStorage.setItem('customer_type', res.data.customerType);
        }
      })
    );
  }

  // ==================== REGISTER ====================
  registerIndividual(data: any): Observable<Result<string>> {
    return this.http.post<Result<string>>(this.apiUrl + '/register-individual',
      data,
      withAuthOptions(TokenType.None, true)  
    );
  }

  registerCorporate(data: any): Observable<Result<string>> {
    return this.http.post<Result<string>>(this.apiUrl + '/register-corporate',
      data,
      withAuthOptions(TokenType.None, true)  
    );
  }

  // ==================== REFRESH TOKEN ====================
  refreshToken() {
    const refreshToken = this.tokenService.getCustomerRefreshToken();
    if (!refreshToken) return null;

    return this.http.post<Result<CustomerLoginResponse>>(
      this.apiUrl + '/refresh-token',
      { refreshToken },
      withAuthOptions(TokenType.None, true)   // ⭐ Anonim + bypass
    ).pipe(
      tap(res => {
        if (res.isSuccessful && res.data) {
          this.tokenService.setCustomerToken(res.data.token);
          this.tokenService.setCustomerRefreshToken(res.data.refreshToken);
          const decoded = this.tokenService.decodeToken(res.data.token);
          this.currentCustomer.set(decoded);
        }
      })
    );
  }

  // ==================== LOGOUT ====================
  logout() {
    return this.http.post<Result<string>>(this.apiUrl + '/customer-logout',
      {},
      withAuthOptions(TokenType.Customer, true))
      .pipe(
        tap(() => this.clearSession())
      );
  }

  logoutAllDevices() {
    return this.http.post<Result<string>>(this.apiUrl + '/logout-all-devices',
      {},
      withAuthOptions(TokenType.Customer, true))
      .pipe(
        tap(() => this.clearSession())
      );
  }


  loadSession() {
    const token = this.tokenService.getCustomerToken();
    if (token && !this.tokenService.isTokenExpired(token)) {
      const decoded = this.tokenService.decodeToken(token);
      if (decoded) {
        this.currentCustomer.set(decoded);
        return;
      }
    }
    this.clearSession();
  }

  clearSession() {
    this.tokenService.removeCustomerToken();
    this.tokenService.removeCustomerRefreshToken();
    localStorage.removeItem('customer_fullName');
    localStorage.removeItem('customer_email');
    localStorage.removeItem('customer_type');
    this.currentCustomer.set(null);
  }

  // ==================== EMAIL/PHONE VERIFICATION ====================
  sendEmailVerificationCode() {
    return this.http.post<Result<string>>(this.apiUrl + '/send-email-verification-code',
      {},
      withAuthOptions(TokenType.Customer));
  }

  // verifyEmail(code: string) {
  //   return this.http.post<Result<string>>(this.apiUrl + '/verify-email', { code });
  // }

  verifyEmail(code: string) {
    return this.http.post<Result<CustomerLoginResponse>>(this.apiUrl + '/verify-email',
      { code },
      withAuthOptions(TokenType.Customer) )
      .pipe(
        tap(res => {
          if (res.isSuccessful && res.data) {
            // 1. Yeni token'ı local storage'a kaydet
            this.tokenService.setCustomerToken(res.data.token);
            this.tokenService.setCustomerRefreshToken(res.data.refreshToken);

            // 2. Decode edip Signal'i güncelle (isEmailVerified otomatik true olacak!)
            const decoded = this.tokenService.decodeToken(res.data.token);
            this.currentCustomer.set(decoded);
          }
        })
      );
  }

  sendSmsVerificationCode() {
    return this.http.post<Result<string>>(this.apiUrl + '/send-sms-verification-code',
      {},
      withAuthOptions(TokenType.Customer))
      .pipe();
  }

  verifyPhone(code: string) {
    return this.http.post<Result<CustomerLoginResponse>>(this.apiUrl + '/verify-phone',
      { code },
      withAuthOptions(TokenType.Customer) )
      .pipe(
        tap(res => {
          if (res.isSuccessful && res.data) {
            // 1. Yeni token'ı local storage'a kaydet
            this.tokenService.setCustomerToken(res.data.token);
            this.tokenService.setCustomerRefreshToken(res.data.refreshToken);

            // 2. Decode edip Signal'i güncelle (isEmailVerified otomatik true olacak!)
            const decoded = this.tokenService.decodeToken(res.data.token);
            this.currentCustomer.set(decoded);
          }
        })
      );;
  }

  // ==================== PASSWORD RESET ====================
  forgotPasswordByEmail(email: string) {
    console.log(email);
    
    return this.http.post<Result<string>>(
      this.apiUrl + '/forgot-password-email-code',
      { email },
      withAuthOptions(TokenType.None, true) 
    );
  }

  resetPasswordByEmail(data: { email: string; code: string; newPassword: string }) {
    return this.http.post<Result<string>>(this.apiUrl + '/reset-password-email',
      data,
      withAuthOptions(TokenType.None, true) 
    );
  }

  forgotPasswordBySms(phoneNumber: string) {
    return this.http.post<Result<string>>(
      this.apiUrl + '/forgot-password-sms-code',
      { phoneNumber },
      withAuthOptions(TokenType.None, true) 
    );
  }

  resetPasswordBySms(data: { phoneNumber: string; code: string; newPassword: string }) {
    return this.http.post<Result<string>>(
      this.apiUrl + '/reset-password-sms',
      data,
      withAuthOptions(TokenType.None, true) 
    );
  }

  




}
