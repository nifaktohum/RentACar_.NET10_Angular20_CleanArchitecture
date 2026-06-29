import { inject, Injectable } from '@angular/core';
import { SnackbarService } from './snackbar.service';
import { HttpErrorResponse } from '@angular/common/http';

@Injectable({
  providedIn: 'root',
})
export class ErrorService {
  private snackbarService = inject(SnackbarService);

  formatAndShowError(error: HttpErrorResponse): void {
    let errorMessage = this.getErrorMessage(error);
    this.snackbarService.error(errorMessage);
  }

  private getErrorMessage(error: HttpErrorResponse): string {
    // Client-side hatası
    if (error.error instanceof ErrorEvent) {
      return `Bağlantı Hatası: ${error.error.message}`;
    }

    // Backend'den gelen hata mesajını yakala (gelişmiş)
    const backendMessage = this.extractBackendMessage(error.error);

    // Status kodlarına göre mesaj
    switch (error.status) {
      case 400:
        return backendMessage || 'Geçersiz istek. Lütfen bilgilerinizi kontrol edin.';
      case 401:
        return backendMessage || 'Oturum süreniz dolmuş veya giriş yapmalısınız!';
      case 403:
        return 'Bu işlem için yetkiniz bulunmuyor!';
      case 404:
        return 'Aradığınız kaynak bulunamadı.';
      case 429:
        return 'Çok fazla istek attınız! Lütfen 1 dakika bekleyin.';
      case 500:
        return backendMessage || 'Sunucu hatası oluştu. Lütfen daha sonra tekrar deneyin.';
      default:
        return backendMessage || `Beklenmedik bir hata oluştu (Kod: ${error.status})`;
    }
  }

  private extractBackendMessage(errorData: any): string | null {
    if (!errorData) return null;
    if (typeof errorData === 'string') return errorData;

    // 1. Standart alanlar
    if (errorData.detail) return errorData.detail;
    if (errorData.message) return errorData.message;

    // 2. .NET Validation Errors (ProblemDetails format)
    if (errorData.errors && typeof errorData.errors === 'object') {
      const firstErrorField = Object.values(errorData.errors).find(
        (value) => Array.isArray(value) && value.length > 0
      );
      if (firstErrorField && Array.isArray(firstErrorField)) {
        return firstErrorField[0]; // "Password must be at least 6 characters"
      }
    }

    // 3. Array formatındaki error'lar
    if (errorData.error && Array.isArray(errorData.error)) {
      return errorData.error[0];
    }

    // 4. Field bazlı direkt hatalar (Django Rest Framework)
    const firstFieldError = Object.values(errorData).find(
      (value) => Array.isArray(value) && value.length > 0
    );
    if (firstFieldError && Array.isArray(firstFieldError)) {
      return firstFieldError[0];
    }

    return null;
  }
}