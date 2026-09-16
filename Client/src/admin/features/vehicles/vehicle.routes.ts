import { Route } from '@angular/router';

export const VEHICLE_ROUTES: Route[] = [
  {
    path: '',
    loadComponent: () => import('./vehicles.component').then(m => m.VehiclesComponent)
  },
  {
    path: 'create-vehicle',
    loadComponent: () => import('./vehicle-form/vehicle-form.component').then(m => m.VehicleFormComponent)
  },
  {
    path: 'edit-vehicle/:id',
    loadComponent: () => import('./vehicle-form/vehicle-form.component').then(m => m.VehicleFormComponent)
  },
  {
    path: 'detail-vehicle/:id',
    loadComponent: () => import('./vehicle-detail/vehicle-detail.component').then(m => m.VehicleDetailComponent)
  },
  {
    path: 'vehicle-types',
    loadComponent: () => import('./vehicle-types/vehicle-types.component').then(m => m.VehicleTypesComponent)
  },
  {
    path: 'vehicle-models',
    loadComponent: () => import('./vehicle-models/vehicle-models.component').then(m => m.VehicleModelsComponent)
  }

]