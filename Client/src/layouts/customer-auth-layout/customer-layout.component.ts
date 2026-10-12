import { ChangeDetectionStrategy, Component, computed, inject, Input, signal, ViewEncapsulation } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { MenuItem, MessageService } from 'primeng/api';
import { MenuModule } from 'primeng/menu';
import { AvatarModule } from 'primeng/avatar';
import { CustomerAuthService } from '../../core/services/customers/customer-auth.service';
import { DrawerModule } from 'primeng/drawer';
@Component({
  selector: 'app-customer-auth-layout',
  imports: [
    RouterOutlet,
    MenuModule,
    AvatarModule,
    RouterLink,
    RouterLinkActive,
DrawerModule
  ],
  templateUrl: './customer-layout.component.html',
  styleUrl: './customer-layout.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.Emulated
})
export class CustomerLayoutComponent {
  private customerAuth = inject(CustomerAuthService);
  private router = inject(Router);
  private messageService = inject(MessageService);

  // ==================== STATE ====================
  sidebarOpen = signal<boolean>(false);
  userMenuOpen = signal<boolean>(false);
  

  // ==================== COMPUTED ====================
  customerFullName = computed(() => {
    const customer = this.customerAuth.currentCustomer();
    const fullName = localStorage.getItem('customer_fullName');
    return fullName || 'Müşteri';
  });

  customerEmail = computed(() => {
    return this.customerAuth.currentCustomer()?.['Email'] || '';
  });

  customerType = computed(() => {
    return this.customerAuth.customerType() || 'Individual';
  });

  customerTypeLabel = computed(() => {
    const type = this.customerType();
    return type === 'Individual' ? 'Bireysel' : 'Kurumsal';
  });

  isEmailVerified = computed(() => this.customerAuth.isEmailVerified());
  isPhoneVerified = computed(() => this.customerAuth.isPhoneVerified());

  constructor() {
    console.log('verifed ' + this.isEmailVerified());
    
  }

  initials = computed(() => {
    const name = this.customerFullName();
    if (!name || name === 'Müşteri') return 'M';
    return name
      .split(' ')
      .map(n => n.charAt(0))
      .slice(0, 2)
      .join('')
      .toUpperCase();
  });

  // ==================== USER MENU ====================
  userMenuItems: MenuItem[] = [
    {
      label: 'Profilim',
      icon: 'ri-user-line',
      routerLink: '/customer/profile'
    },
    {
      label: 'Kiralamalarım',
      icon: 'ri-roadster-line',
      disabled: true    // Henüz yok
    },
    {
      separator: true
    },
    {
      label: 'Çıkış Yap',
      icon: 'ri-logout-box-r-line',
      command: () => this.logout()
    }
  ];

  // ==================== SIDEBAR TOGGLE ====================
  toggleSidebar(): void {
    this.sidebarOpen.update(v => !v);
  }

  // ==================== LOGOUT ====================
  logout(): void {
    this.customerAuth.logout().subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Çıkış Yapıldı',
          detail: 'Güvenli bir şekilde çıkış yaptınız.'
        });
        this.router.navigate(['/customer-auth/customer-login']);
      },
      error: () => {
        // Hata olsa bile client-side temizle
        this.customerAuth.clearSession();
        this.router.navigate(['/customer-auth/customer-login']);
      }
    });
  }
}
