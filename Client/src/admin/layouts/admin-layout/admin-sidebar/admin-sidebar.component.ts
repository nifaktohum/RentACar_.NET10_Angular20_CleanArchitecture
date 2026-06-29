import { ChangeDetectionStrategy, Component, computed, effect, inject, input, model, signal } from '@angular/core';
import { MatExpansionModule } from '@angular/material/expansion';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { NavigationModel, navigations } from '../../../../navigation';
import { NgClass } from '@angular/common';
import { AuthService } from '../../../../core/services/auth.service';
import { NavigationService } from '../../../core/services/navigation.service';

@Component({
  selector: 'app-admin-sidebar',
  imports: [
    MatExpansionModule,
    RouterLink,
    RouterLinkActive,
    NgClass
],
  templateUrl: './admin-sidebar.component.html',
  styleUrl: './admin-sidebar.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminSidebarComponent {
  readonly authService = inject(AuthService);
  private navService = inject(NavigationService);
  
  readonly current = computed(() => this.authService.currentUser());
  
  // Eski =>  navigatios = signal<NavigationModel[]>(navigations);
  navigatios = this.navService.filteredNavigations; // <== Yeni
  
  isSidebarCollapsed = signal<boolean>(true);
  isCollapsed = model<boolean>(false);
  // Hangi alt menünün açık olduğunu takip eden bir signal nesnesi tutuyoruz
  // Örn: { cars: true, reservations: false }
  openSubmenus = signal<{ [key: string]: boolean }>({});

  // 2. Navigation değiştiğinde signal'i güncelleyen bir effect
  constructor() {
    effect(() => {
      // filteredNavigations her değiştiğinde, openSubmenus state'ini yeniden kuruyoruz
      const navs = this.navService.filteredNavigations();
      this.openSubmenus.set(this.initializeSubmenus(navs));
    });
  }

  
  // Alt menüleri açıp kapatan toggle metodu
  toggleSubmenu(menuKey: string, event: Event): void {
    event.preventDefault();
    if (this.isCollapsed()) this.isCollapsed.set(false);

    this.openSubmenus.update(state => ({
      ...state,
      [menuKey]: !state[menuKey]
    }));
  }

  private initializeSubmenus(navs: any[]): { [key: string]: boolean } {
    const state: { [key: string]: boolean } = {};
    navs.forEach(item => {
      if (item.haveSubMenu) {
        state[item.title.toLowerCase()] = false;
      }
    });
    return state;
  }

  


  visible() {
    this.isCollapsed.set(true)
  }




}
