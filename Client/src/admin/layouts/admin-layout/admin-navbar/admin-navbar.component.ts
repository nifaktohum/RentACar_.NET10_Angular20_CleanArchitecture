import { ChangeDetectionStrategy, Component, computed, effect, HostListener, inject, OnInit, signal } from '@angular/core';
import { AuthService } from '../../../../core/services/auth.service';
import { Router } from '@angular/router';
import { BraadcrumbComponent } from '../../../shared/components/braadcrumb/braadcrumb.component';


@Component({
  selector: 'app-admin-navbar',
  imports: [BraadcrumbComponent],
  templateUrl: './admin-navbar.component.html',
  styleUrl: './admin-navbar.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminNavbarComponent {
  readonly authService = inject(AuthService);
  private router = inject(Router);
  
  activeDropdown = signal<string | null>(null);
  readonly current = computed(() => this.authService.currentUser());
  
  // ============================>


  toggleDropdown(type: string, event: Event): void {
    event.stopPropagation(); // Tıklamanın dökümana yayılmasını engeller
    if (this.activeDropdown() === type) {
      this.activeDropdown.set(null); // Zaten açıksa kapat
    } else {
      this.activeDropdown.set(type); // Kapalıysa hedefi aç
    }
  }

  // Sayfada boş bir yere tıklandığında açık olan tüm dropdown'ları otomatik kapatır
  @HostListener('document:click')
  closeDropdowns(): void {
    this.activeDropdown.set(null);
  }

  logout() {
    this.authService.logout();
    this.router.navigateByUrl('/auth/login');
  }

  // 🔥 YENİ: Tüm cihazlardan güvenli çıkış lojiğini tetikliyoruz kanka
  logoutAllDevices() {
    this.authService.logoutAllDevices().subscribe({
      next: () => {
        this.router.navigateByUrl('/auth/login');
      },
      error: () => {
        // Hata interceptor'ın (errorInterceptor) zaten 401 veya diğer hataları yakalayacak
        // Ama akışın yarıda kalmaması için burayı da emniyete alıyoruz kanka
        this.authService.logout();
        this.router.navigateByUrl('/auth/login');
      }
    });
  }
}
