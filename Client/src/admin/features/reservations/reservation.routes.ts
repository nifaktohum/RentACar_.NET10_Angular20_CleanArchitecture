import { Route } from '@angular/router';

export const RESERVATION_ROUTES: Route[] = [
  {
    path: '',
    loadComponent: () => import('./reservations.component').then(m => m.ReservationsComponent)
  },
  {
    path: 'active',                                                      // 🆕 YENİ
    loadComponent: () => import('./reservations-active/reservations-active.component').then(m => m.ReservationsActiveComponent)
  },
  {
    path: 'detail/:id',
    loadComponent: () => import('./reservation-detail/reservation-detail.component').then(m => m.ReservationDetailComponent)
  },
  {
    path: 'add',
    loadComponent: () => import('./reservation-form/reservation-form.component').then(m => m.ReservationFormComponent)
  }
];