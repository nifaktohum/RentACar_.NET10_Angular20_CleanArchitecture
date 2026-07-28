import { Route } from '@angular/router';

export const EXTRA_ROUTES: Route[] = [
  {
    path: '', // localhost:4200/admin/branches -> Liste Sayfası
    loadComponent: () => import('./extras.component').then(m => m.ExtrasComponent) // Kendi list component path'in
  },
 

];