import { Route } from '@angular/router';

export const RESERVATION_ROUTES: Route[] = [
  {
    path: '',
    loadComponent: () => import('./reservations.component').then(m => m.ReservationsComponent)
  },
  // İleride eklenecek:
  // {
  //   path: 'add',
  //   loadComponent: () => import('./reservation-form/reservation-form.component').then(m => m.ReservationFormComponent)
  // },
  // {
  //   path: 'detail/:id',
  //   loadComponent: () => import('./reservation-detail/reservation-detail.component').then(m => m.ReservationDetailComponent)
  // }
];