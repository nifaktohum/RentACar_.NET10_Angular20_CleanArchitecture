import { Route } from '@angular/router';

export const ROLES_ROUTES: Route[] = [
  {
    path: '', // localhost:4200/admin/branches -> Liste Sayfası
    loadComponent: () => import('./roles.component').then(m => m.RolesComponent) // Kendi list component path'in
  },

];