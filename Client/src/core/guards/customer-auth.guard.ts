import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { CustomerAuthService } from '../services/customers/customer-auth.service';

export const customerAuthGuard: CanActivateFn = (_route, state) => {
  const customerAuth = inject(CustomerAuthService);
  const router = inject(Router);

  if (customerAuth.isAuthenticated()) {
    return true;
  }

  return router.createUrlTree(['/customer-auth/customer-login'], {
    queryParams: { returnUrl: state.url },
  });
};