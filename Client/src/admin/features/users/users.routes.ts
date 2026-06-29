import { Route } from '@angular/router';

export const USERS_ROUTES: Route[] = [
  {
    path: '', // localhost:4200/admin/branches -> Liste Sayfası
    loadComponent: () => import('./users.component').then(m => m.UsersComponent) // Kendi list component path'in
  },

];