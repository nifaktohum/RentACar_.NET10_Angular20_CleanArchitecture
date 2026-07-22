import { Route } from '@angular/router';

export const PROTECTION_PACKAGE_ROUTES: Route[] = [
  {
    path: '', // localhost:4200/admin/branches -> Liste Sayfası
    loadComponent: () => import('./protection-packages.component').then(m => m.ProtectionPackagesComponent) // Kendi list component path'in
  },
  {
    path: 'benefits', // localhost:4200/admin/branches -> Liste Sayfası
    loadComponent: () => import('./protection-benefits/protection-benefits.component').then(m => m.ProtectionBenefitsComponent) // Kendi list component path'in
  },
  {
    path: 'benefit-categories',
    loadComponent: () => import('./protection-benefits/benefit-categories/benefit-categories.component').then(m => m.BenefitCategoriesComponent)
  }

];