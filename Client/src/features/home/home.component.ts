import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { MessageService } from 'primeng/api';
import { CustomerAuthService } from '../../core/services/customers/customer-auth.service';

@Component({
  selector: 'app-home',
  imports: [
    RouterLink
  ],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomeComponent {
  private customerAuth = inject(CustomerAuthService);
  private router = inject(Router);
  private messageService = inject(MessageService);






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
