import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { TokenService } from '../services/token.service';
import { TOKEN_TYPE, TokenType } from './token-context';

export const httpInterceptor: HttpInterceptorFn = (req, next) => {
  const tokenService = inject(TokenService);

  // ⭐ Context'ten token tipini oku (varsayılan: User)
  const tokenType = req.context.get(TOKEN_TYPE);

  // ⭐ Anonim isteklerde token arama ve direkt devam et
  if (tokenType === TokenType.None) {
    return next(req);
  }

  const token = tokenType === TokenType.Customer
    ? tokenService.getCustomerToken()
    : tokenService.getUserToken();
  
  
  // Token varsa ekle, yoksa devam
  if (token) {
    return next(req.clone({
      setHeaders: { Authorization: `Bearer ${token}` }
    }));
  }

  return next(req);

}

/*

    // Customer endpoint'leri
    // const CUSTOMER_URL_PATTERNS = [
    //                         '/customersauth',
    //                         '/customersprofile',
    //                         '/customersmanagement'
    //                       ];

    export const httpInterceptor: HttpInterceptorFn = (req, next) => {
      const authService = inject(AuthService);
      // const token = authService.getToken();
      const _tokenService = inject(TokenService);
      const _url = req.url.toLowerCase();

      // URL Customer endpoint'i mi?
      // const isCustomerRequest = CUSTOMER_URL_PATTERNS.some(pattern => req.url.includes(pattern));
      const isCustomerRequest = _url.includes('customer');
      let token: string | null = null;
      console.log('isCustomerRequest: ', _url);

      if (isCustomerRequest) {
        token = _tokenService.getCustomerToken();
        console.log('Customer Token:', token); // <-- Kontrol et
      } else {
        token = _tokenService.getUserToken();
        console.log('User Token:', token); // <-- Kontrol et
      }

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
*/

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