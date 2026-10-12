import { Route } from '@angular/router';


// ===> admin/customers/[....]
export const CUSTOMER_ROUTES: Route[] = [
  {
    path: '',
    loadComponent: () => import('./customers.component').then(m => m.CustomersComponent)
  },
  {
    path: 'detail/:id',
    loadComponent: () => import('./customer-detail/customer-detail.component').then(m => m.CustomerDetailComponent)
  }, 
  {
    path: 'blacklist',
    loadComponent: () => import('./black-list/blacklist.component').then(m => m.BlacklistComponent)
  },
  {
    path: 'blacklist/:id',
    loadComponent: () => import('./black-list/blacklist.component').then(m => m.BlacklistComponent)
  }
];