import { Route } from "@angular/router";
import { CategoriesComponent } from "./categories.component";

export const CATEGORIES_ROUTES: Route[] = [
  { path: '',
    loadComponent: () => import('./categories.component').then(m => m.CategoriesComponent),
  },
  {
    path: 'category-create',
    component: CategoriesComponent, // Aynı component
    data: { mode: 'page' }           // ÖNEMLİ: Bu veri gönderiliyor mu?
  }
]