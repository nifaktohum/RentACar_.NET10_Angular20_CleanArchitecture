import { Route } from '@angular/router';

export const BRANCH_ROUTES: Route[] = [
  {
    path: '', // localhost:4200/admin/branches -> Liste Sayfası
    loadComponent: () => import('./branches.component').then(m => m.BranchesComponent) // Kendi list component path'in
  },
  {
    path: 'branch-create', // localhost:4200/admin/branches/branch-create -> Ekleme Sayfası
    loadComponent: () => import('./branch-create/branch-create.component').then(m => m.BranchCreateComponent)
  },
  {
    path: 'branch-edit/:id', // localhost:4200/admin/branches/branch-edit/5 -> Güncelleme Sayfası
    loadComponent: () => import('./branch-create/branch-create.component').then(m => m.BranchCreateComponent)
  }
];