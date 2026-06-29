import { Route } from "@angular/router";

export const CATEGORIES_ROUTES: Route[] = [
  { path: '',
    loadComponent: () => import('./categories.component').then(m => m.CategoriesComponent)
  }
]