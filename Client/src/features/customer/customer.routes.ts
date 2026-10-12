import { Routes } from "@angular/router";
import { customerAuthGuard } from "../../core/guards/customer-auth.guard";
import { CustomerLayoutComponent } from "../../layouts/customer-auth-layout/customer-layout.component";

export const CUSTOMER_ROUTES: Routes = [
  {
    path: '',
    component: CustomerLayoutComponent,
    canActivate: [customerAuthGuard],
    children: [
      { path: '', redirectTo: 'profile', pathMatch: 'full' },
      {
        path: 'profile',
        loadComponent: () => import('./profile/profile.component').then(m => m.ProfileComponent)
      },
      {
        path: 'profile/edit',
        loadComponent: () => import('./profile-edit/profile-edit.component').then(m => m.ProfileEditComponent)
      },
      {
        path: 'verify-email',
        loadComponent: () => import('./verify-email/verify-email.component').then(m => m.VerifyEmailComponent)
      },
      {
        path: 'verify-phone',
        loadComponent: () => import('./verify-phone/verify-phone.component').then(m => m.VerifyPhoneComponent)
      }
    ]
   }
];