import { Routes } from '@angular/router';
import { customerGuestGuard } from '../../core/guards/customer-guest.guard';

export const CUSTOMER_AUTH_ROUTES: Routes = [
  {
    path: '',
    redirectTo: 'customer-login',
    pathMatch: 'full',
  },
  {
    path: 'customer-login',
    title: 'Müşteri Girişi',
    canActivate: [customerGuestGuard],
    loadComponent: () =>
      import('./login/login.component').then(m => m.LoginComponent),
  },
  {
    path: 'register-individual',
    title: 'Bireysel Kayıt',
    canActivate: [customerGuestGuard],
    loadComponent: () =>
      import('./register-individual/register-individual.component').then(
        m => m.RegisterIndividualComponent
      ),
  },
  {
    path: 'register-corporate',
    title: 'Kurumsal Kayıt',
    canActivate: [customerGuestGuard],
    loadComponent: () =>
      import('./register-corporate/register-corporate.component').then(
        m => m.RegisterCorporateComponent
      ),
  },
  {
    path: 'forgot-password',
    title: 'Şifremi Unuttum',
    canActivate: [customerGuestGuard],
    loadComponent: () =>
      import('./forgot-password/forgot-password.component').then(
        m => m.ForgotPasswordComponent
      ),
  },
  {
    path: '**',
    redirectTo: 'customer-login',
  },
];