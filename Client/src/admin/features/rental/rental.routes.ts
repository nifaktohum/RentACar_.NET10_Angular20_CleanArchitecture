import { Route } from '@angular/router';

export const RENTAL_ROUTES: Route[] = [
  {
    path: '',
    loadComponent: () => import('./rental-extra-selector/rental-extra-selector.component').then(m => m.RentalExtraSelectorComponent)
  }
  // {
  //   path: '',
  //   loadComponent: () => import('./pages/rental-create/rental-create.component').then(m => m.RentalCreateComponent)
  // },
  // {
  //   path: 'create',
  //   loadComponent: () => import('./pages/rental-create/rental-create.component').then(m => m.RentalCreateComponent)
  // },
  // {
  //   path: 'list',
  //   loadComponent: () => import('./pages/rental-list/rental-list.component').then(m => m.RentalListComponent)
  // }
];