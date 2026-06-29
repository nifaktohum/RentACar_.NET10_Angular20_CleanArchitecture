import { Injectable, signal } from '@angular/core';
import { BreadCrumbModel } from '../models/breadcrumb';

@Injectable({
  providedIn: 'root',
})
export class BreadcrumbService {

  readonly breadCrumbData = signal<BreadCrumbModel[]>([]);

  // 🎯 breadCrumbs parametresini boş gelebilir (optional) yaptık
  reset(breadCrumbs: BreadCrumbModel[] = []) {
    const dashboard: BreadCrumbModel = {
      title: 'Dashboard',
      url: '/admin/dashboard',
      icon: 'ri-dashboard-3-line',
      // Eğer arkasından başka sayfa gelmiyorsa Dashboard aktiftir, geliyorsa inaktiftir
      isActive: breadCrumbs.length === 0
    };

    // 🎯 DOĞRU BİRLEŞTİRME: Dizinin ilk elemanı dashboard, sonrakiler parametreden gelenler
    this.breadCrumbData.set([dashboard, ...breadCrumbs]);
  }

  addCrumb(crumb: BreadCrumbModel) {
    // Mevcut array'in sonuna yeni gelen kırıntıyı ekliyoruz
    this.breadCrumbData.update(currentCrumbs => [...currentCrumbs, crumb]);
  }
}