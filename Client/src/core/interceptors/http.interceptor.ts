import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

export const httpInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getToken();

  // Eğer token varsa request'i clone'layıp header ekliyoruz
  if (token) {
    const cloned = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
    return next(cloned);
  }

  // Token yoksa (örn: login/register istekleri), isteği hiç bozmadan aynen devam ettir
  return next(req);
};


/*
      export const httpInterceptor: HttpInterceptorFn = (req, next) => {
        const authService = inject(AuthService);
        const token = authService.getToken();
        
        const url = req.url;
        const endpoint = 'https://localhost:5001/';
        
        // 1. Ortak bir headers nesnesi hazırlıyoruz
        const headers: { [key: string]: string } = {};
        if (token) {
          headers['Authorization'] = `Bearer ${token}`;
        }

        // 2. TEK SEFERDE CLONE: Hem URL'i değiştiriyoruz hem de varsa header'ı basıyoruz kanka
        const cloned = req.clone({
          url: url.replace('/rent/', endpoint),
          setHeaders: headers
        });
             
        
        return next(cloned);
      };
*/