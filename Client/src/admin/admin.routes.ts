import { Routes } from "@angular/router";
import { AdminLayoutComponent } from "./layouts/admin-layout/admin-layout.component";
import { permissionAuthGuard } from "../core/guards/permissionAuthGuard.guard";

export const ADMIN_ROUTES: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  { path: '',
    component: AdminLayoutComponent,
    canActivate: [permissionAuthGuard],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },

      { path: 'dashboard',
        loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
      },
      // --- BRANCHES (ŞUBELER) ROTASI ---
      {
        path: 'branches',
        loadChildren: () => import('./features/branches/branches.routes').then(m => m.BRANCH_ROUTES),
        canActivate: [permissionAuthGuard], // Yetki kontrolü ekledik
        data: { requiredPermission: 'Branches.Read' } // İzin kodu
      },
      // --- ROLES (ROLLER) ROTASI ---
      {
        path: 'roles',
        loadChildren: () => import('./features/roles/roles.routes').then(m => m.ROLES_ROUTES),
        canActivate: [permissionAuthGuard], // Yetki kontrolü ekledik
        data: { requiredPermission: 'Roles.Read' } // İzin kodu
      },
      // --- USERS (ROLLER) ROTASI ---
      {
        path: 'users',
        loadChildren: () => import('./features/users/users.routes').then(m => m.USERS_ROUTES),
        canActivate: [permissionAuthGuard], // Yetki kontrolü ekledik
        data: { requiredPermission: 'Users.Read' } // İzin kodu
      },
      // --- CATEGORİES (ROLLER) ROTASI ---
      {
        path: 'categories',
        loadChildren: () => import('./features/categories/categories.routes').then(m => m.CATEGORIES_ROUTES),
        canActivate: [permissionAuthGuard], // Yetki kontrolü ekledik
        data: { requiredPermission: 'Categories.Read' } // İzin kodu
      },
      // --- PROTECTİON PACKAGE (KORUMA PAKETLERI) ROTASI ---
      {
        path: 'protection-packages',
        loadChildren: () => import('./features/protection-packages/protection-packages.routes').then(m => m.PROTECTION_PACKAGE_ROUTES),
        canActivate: [permissionAuthGuard], // Yetki kontrolü ekledik
        data: { requiredPermission: 'ProtectionPackage.Read' } // İzin kodu
      }
    ]
  },
];


/*
export const routes: Routes = [
    
    {path: "admin", component: LayoutComponent, children: [
                {path:'', redirectTo: 'dasboard', pathMatch: 'full'},
                {path: 'dasboard', component: DashboardComponent, canActivate: [authGuard] },
                {path: 'customers', component: CustomerComponent, canActivate: [authGuard] },
                {path: 'orders', component: OrdersComponent, canActivate: [authGuard] },
                {path: 'products', component: ProductsComponent, canActivate: [authGuard] },
            ], canActivate: [authGuard] 
    },
    {path: '', component: HomeComponent},
    {path: '', redirectTo: 'home', pathMatch: 'full'},
    {path: 'home', component: HomeComponent},
    {path: 'products-user', component: ProductsUserComponent},
    {path: 'basket', component: BasketComponent},
    {path: 'register', component: RegisterComponent},
    {path: 'login', component: LoginComponent},
];



*/