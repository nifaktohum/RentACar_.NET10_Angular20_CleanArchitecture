import { Route } from '@angular/router';

export const EXTRA_ROUTES: Route[] = [
  {
    path: '', // localhost:4200/admin/branches -> Liste Sayfası
    loadComponent: () => import('./extras.component').then(m => m.ExtrasComponent) // Kendi list component path'in
  },
  {
    path: 'extra-selector', // localhost:4200/admin/branches -> Liste Sayfası
    loadComponent: () => import('./extra-selector/extra-selector.component').then(m => m.ExtraSelectorComponent) // Kendi list component path'in
  },
 

];