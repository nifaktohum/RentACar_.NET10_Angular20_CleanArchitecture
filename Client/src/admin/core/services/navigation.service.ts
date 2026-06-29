import { computed, inject, Injectable } from '@angular/core';
import { PermissionService } from './permission.service';
import { navigations } from '../../../navigation';

@Injectable({
  providedIn: 'root',
})
export class NavigationService {
  private permissionService = inject(PermissionService);


  
  // Sadece yetkisi olan menüleri döndüren computed signal
  filteredNavigations = computed(() => {
    return this.filterNav(navigations);
  });
  // * PRİVATE ----------------------->
  
  private filterNav(items: any[]): any[] {
    return items
      .filter(item => !item.role || this.permissionService.hasPermission(item.role)) // Yetkisi yoksa filtrele
      .map(item => ({
        ...item,
        // Eğer alt menü varsa, onları da recursive filtrele
        subMenus: item.subMenus ? this.filterNav(item.subMenus) : undefined
      }))
      // Eğer bir menünün alt menüleri filtrelendikten sonra boş kaldıysa ve menü kendi başına bir link değilse gizle
      .filter(item => !item.haveSubMenu || (item.subMenus && item.subMenus.length > 0));
  }
}

