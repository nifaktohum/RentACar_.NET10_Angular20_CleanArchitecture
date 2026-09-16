import { Injectable, signal } from '@angular/core';
import { BreadCrumbModel } from '../models/breadcrumb';

@Injectable({
  providedIn: 'root',
})
// breadcrumb.service.ts
export class BreadcrumbService {

  readonly breadCrumbData = signal<BreadCrumbModel[]>([]);

  reset(breadCrumbs: BreadCrumbModel[] = []) {
    const dashboard: BreadCrumbModel = {
      title: 'Dashboard',
      url: '/admin/dashboard',
      icon: 'ri-dashboard-3-line',
      isActive: breadCrumbs.length === 0
    };
    this.breadCrumbData.set([dashboard, ...breadCrumbs]);
  }

  // ✅ YENİ METOD: update()
  update(breadCrumbs: BreadCrumbModel[]) {
    const dashboard: BreadCrumbModel = {
      title: 'Dashboard',
      url: '/admin/dashboard',
      icon: 'ri-dashboard-3-line',
      isActive: breadCrumbs.length === 0
    };
    this.breadCrumbData.set([dashboard, ...breadCrumbs]);
  }

  addCrumb(crumb: BreadCrumbModel) {
    this.breadCrumbData.update(currentCrumbs => [...currentCrumbs, crumb]);
  }
}