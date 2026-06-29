import { HttpInterceptorFn, HttpErrorResponse, HttpContextToken } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { Router } from '@angular/router';
import { MessageService } from 'primeng/api';
import { AuthService } from '../services/auth.service';

// 🚀 Interceptor'ı bypass etmek için kullanacağımız sihirli token'ı buraya tanımlıyoruz kanka.
// Servis dosyasında istek atarken bunu tetikleyeceğiz.
export const BYPASS_INTERCEPTOR = new HttpContextToken<boolean>(() => false);

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const messageServiceToast = inject(MessageService);
  const authService = inject(AuthService)
  const router = inject(Router);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {

      // 🔥 GÖZÜNÜN YAŞINA BAKMA KANKS: Eğer istek bypass edilmek istendiyse,
      // Interceptor hiçbir işleme girmeden hatayı fırlatıp aradan çekiliyor!
      if (req.context.get(BYPASS_INTERCEPTOR) === true) {
        return throwError(() => error);
      }

      let errorMessage = 'Beklenmedik bir hata oluştu!';

      if (error.error instanceof ErrorEvent) {
        errorMessage = `Bağlantı Hatası: ${error.error.message}`;
      } else {
        const backendMessage =
          error.error?.detail ||
          error.error?.message ||
          (error.error?.error && Array.isArray(error.error.error) ? error.error.error[0] : null);

        // Eğer hata 401 ise, nerede olursak olalım 
        // Arkadan gelen çoklu istekler hafızayı ezmesin diye İSTİSNASIZ temizliyoruz kanka!
        if (error.status === 401) {
          // ✅ SADECE oturumla ilgili olanları temizle
          // localStorage.clear(); // ❌ BUNU KALDIR

          // ✅ SADECE auth ile ilgili olanları temizle
          const rememberMe = localStorage.getItem('rememberedEmail');
          localStorage.removeItem('accessToken'); // Token'ı sil
          localStorage.removeItem('fullName');
          localStorage.removeItem('email');
          // localStorage.removeItem('refreshToken'); // Varsa

          // ✅ 'rememberedEmail' KULLANICI İSTEĞİYLE KORUNUR
          if (!rememberMe) {
            localStorage.removeItem('rememberedEmail');
          }

          // AuthService'deki state'leri de temizle
          authService.clearSession(); // AuthService'de bir metot oluştur

          if (error.error?.error === 'SecurityStampMismatch') {
            errorMessage = 'Güvenlik nedeniyle tüm oturumlarınız sonlandırıldı!';
          } else {
            errorMessage = backendMessage || 'Oturumunuz geçersiz veya şifre hatalı!';
          }

          // Login sayfasına yönlendir (sonsuz döngüyü engelle)
          if (!router.url.includes('/auth/login')) {
            router.navigate(['/auth/login']);
          }

          messageServiceToast.add({
            severity: 'error',
            summary: 'Hata',
            detail: errorMessage
          });

          return throwError(() => error);
        }
        switch (error.status) {
          case 400:
            // Şifre yanlış hataları genelde 401 veya 400 düşer, ikisinde de bu mesajı yakala kanka
            // .NET Identity'den gelen "E-posta veya şifre hatalı." mesajını doğrudan yakalayıp basıyoruz!
            errorMessage = backendMessage || 'Kullanıcı adı veya şifre hatalı kanka!';
            break;
          case 403:
            errorMessage = 'Bu işlem için yetkiniz bulunmuyor!';
            break;
          case 404:
            errorMessage = 'Sunucu hatası: :(';
            break;
          case 429:
            errorMessage = 'Çok fazla istek attınız! Lütfen biraz bekleyin.';
            break;
          case 500:
            errorMessage = backendMessage || 'Sunucu tarafında sistemsel bir hata oluştu!';
            break;
          default:
            errorMessage = backendMessage || `Bir hata oluştu (Kod: ${error.status})`;
        }
      }

      // Ekrana şık snackbar'ı basıyoruz
      messageServiceToast.add({
        severity: 'error',
        summary: 'Hata',
        detail: errorMessage
      });

      return throwError(() => error);
    })
  );
};