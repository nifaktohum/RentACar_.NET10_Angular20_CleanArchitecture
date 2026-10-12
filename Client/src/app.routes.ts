import { Routes } from '@angular/router';
import { adminAuthGuard } from './core/guards/admin-auth.guard';
import { NotFoundComponent } from './shared/pages/not-foun/not-found.component';
import { customerAuthGuard } from './core/guards/customer-auth.guard';

export const routes: Routes = [
  // ==================== ANA SAYFA ====================
  {
    path: '',
    title: 'Ana Sayfa',
    loadComponent: () =>
      import('./features/home/home.component').then(m => m.HomeComponent),
  },

  // ==================== PERSONEL / ADMIN ====================
  {
    path: 'auth',
    loadChildren: () =>
      import('./features/auth/auth.routes').then(m => m.AUTH_ROUTES),
  },
  {
    path: 'admin',
    canActivate: [adminAuthGuard],   // 👈 admin panelinin girişi korumalı
    loadChildren: () =>
      import('./admin/admin.routes').then(m => m.ADMIN_ROUTES),
  },

  // ==================== MÜŞTERİ ====================
  {
    path: 'customer-auth',
    loadChildren: () =>
      import('./features/customer-auth/customer-auth.routes').then(
        m => m.CUSTOMER_AUTH_ROUTES
      ),
  }, 
  {
    path: 'customer',
    canActivate: [customerAuthGuard], // 👈 tutarlılık için burada da koyalım
    loadChildren: () =>
      import('./features/customer/customer.routes').then(m => m.CUSTOMER_ROUTES),
  },

  // ==================== FALLBACK ====================
  {
    path: '**',
    title: 'Sayfa Bulunamadı',
    component: NotFoundComponent,
  },
];