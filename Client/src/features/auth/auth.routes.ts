import { Routes } from "@angular/router";
import { adminGuestGuard } from "../../core/guards/admin-guest.guard";

export const AUTH_ROUTES: Routes = [
  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full',
  },
  {
    path: 'login',
    title: 'Admin Girişi',
    canActivate: [adminGuestGuard],
    loadComponent: () =>
      import('./login/login.component').then(m => m.LoginComponent),
  },
  {
    path: 'register',
    title: 'Admin Kaydı',
    canActivate: [adminGuestGuard],
    loadComponent: () =>
      import('./register/register.component').then(m => m.RegisterComponent),
  },
  {
    path: '**',
    redirectTo: 'login',
  },
];