import { CanActivateFn, Router } from '@angular/router';
import { CustomerAuthService } from '../services/customers/customer-auth.service';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

/**
 * Giriş yapmış müşteriyi login/register sayfalarından uzak tutar
 * ve müşteri profiline yönlendirir.
 */
export const customerGuestGuard: CanActivateFn = () => {
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