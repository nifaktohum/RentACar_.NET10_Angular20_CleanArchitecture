import { Injectable } from '@angular/core';
import { TokenPayload } from '../models/tokenPayload.model';
import { jwtDecode } from 'jwt-decode';

@Injectable({
  providedIn: 'root',
})
export class TokenService {
  private readonly USER_TOKEN_KEY = 'token';
  private readonly CUSTOMER_TOKEN_KEY = 'customer_token';
  private readonly USER_REFRESH_KEY = 'refreshToken';
  private readonly CUSTOMER_REFRESH_KEY = 'customer_refreshToken';

  // ==================== USER TOKEN ====================
  setUserToken(token: string): void {
    localStorage.setItem(this.USER_TOKEN_KEY, token);
  }

  getUserToken(): string | null {
    return localStorage.getItem(this.USER_TOKEN_KEY);
  }

  removeUserToken(): void {
    localStorage.removeItem(this.USER_TOKEN_KEY);
  }

  // ==================== CUSTOMER TOKEN ====================
  setCustomerToken(token: string): void {
    localStorage.setItem(this.CUSTOMER_TOKEN_KEY, token);
  }

  getCustomerToken(): string | null {
    return localStorage.getItem(this.CUSTOMER_TOKEN_KEY);
  }

  removeCustomerToken(): void {
    localStorage.removeItem(this.CUSTOMER_TOKEN_KEY);
  }

  // ==================== REFRESH TOKEN ====================
  setUserRefreshToken(token: string): void {
    localStorage.setItem(this.USER_REFRESH_KEY, token);
  }

  getUserRefreshToken(): string | null {
    return localStorage.getItem(this.USER_REFRESH_KEY);
  }

  removeUserRefreshToken(): void {
    localStorage.removeItem(this.USER_REFRESH_KEY);
  }

  setCustomerRefreshToken(token: string): void {
    localStorage.setItem(this.CUSTOMER_REFRESH_KEY, token);
  }

  getCustomerRefreshToken(): string | null {
    return localStorage.getItem(this.CUSTOMER_REFRESH_KEY);
  }

  removeCustomerRefreshToken(): void {
    localStorage.removeItem(this.CUSTOMER_REFRESH_KEY);
  }

  // ==================== ORTAK ====================
  decodeToken(token: string): TokenPayload | null {
    try {
      return jwtDecode<TokenPayload>(token);
    } catch {
      return null;
    }
  }

  // Token'ın süresinin dolup dolmadığını kontrol eder
  isTokenExpired(token: string): boolean {
    if (!token) return true;
    try {
      const decoded = jwtDecode<TokenPayload>(token);
      if (!decoded || !decoded.exp) return true;
      return Date.now() > decoded.exp * 1000;
    } catch {
      return true;
    }
  }

  isCustomerToken(token: string): boolean {
    if (!token) return false;
    const decoded = this.decodeToken(token);
    return decoded?.UserType === 'Customer';
  }
}