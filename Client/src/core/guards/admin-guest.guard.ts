import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { CustomerAuthService } from '../services/customers/customer-auth.service';

/**
 * Admin zaten giriş yapmışsa login sayfasına erişmesini engeller
 * ve /admin/dashboard paneline yönlendirir.
 */
export const adminGuestGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const customerAuth = inject(CustomerAuthService);
  const router = inject(Router);

  // 1) Admin / personel girişli → admin panele at
  if (auth.isAuthenticated()) {
    return router.createUrlTree(['/admin/dashboard']);
  }

  // 2) Müşteri girişli → ana sayfaya at
  if (customerAuth.isAuthenticated()) {
    return router.createUrlTree(['/']);
  }

  // 3) Hiçbiri yok → login sayfası göster
  return true;
};