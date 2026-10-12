import { ChangeDetectionStrategy, Component, computed, inject, OnInit, signal, ViewEncapsulation } from '@angular/core';
import { MessageService } from 'primeng/api';
import { CustomerProfileService } from '../../../core/services/customers/customer-profile.service';
import { CustomerProfileModel } from '../../../core/models/customer-profile.model';
import { finalize } from 'rxjs';
import { Result } from '../../../core/models/result.model';
import { TagModule } from 'primeng/tag';
import { SkeletonModule } from 'primeng/skeleton';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-profile',
  imports: [
    TagModule,
    SkeletonModule,
    RouterLink
  ],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.Emulated
})
export class ProfileComponent implements OnInit {
  private profileService = inject(CustomerProfileService);
  private messageService = inject(MessageService);

  // ==================== STATE ====================
  profile = signal<CustomerProfileModel | null>(null);
  isLoading = signal<boolean>(true);

  // ==================== COMPUTED ====================
  fullName = computed(() => {
    const p = this.profile();
    if (!p) return '';

    if (p.type === 'Individual') {
      return `${p.firstName || ''} ${p.lastName || ''}`.trim();
    }
    return p.companyName || 'Kurumsal Müşteri';
  });

  initials = computed(() => {
    const name = this.fullName();
    if (!name) return 'M';
    return name
      .split(' ')
      .filter(n => n.length > 0)
      .map(n => n.charAt(0))
      .slice(0, 2)
      .join('')
      .toUpperCase();
  });

  customerTypeLabel = computed(() => {
    return this.profile()?.type === 'Corporate' ? 'Kurumsal' : 'Bireysel';
  });

  genderLabel = computed(() => {
    const g = this.profile()?.gender;
    if (!g) return '-';
    switch (g) {
      case 'Male': return 'Erkek';
      case 'Female': return 'Kadın';
      default: return g;
    }
  });

  findeksStatus = computed(() => {
    const score = this.profile()?.findeksScore;
    if (score == null) return { label: 'Belirsiz', severity: 'secondary' as const };

    if (score >= 1500) return { label: 'Çok İyi', severity: 'success' as const };
    if (score >= 1200) return { label: 'İyi', severity: 'info' as const };
    if (score >= 900) return { label: 'Orta', severity: 'warn' as const };
    return { label: 'Düşük', severity: 'danger' as const };
  });

  // ==================== LIFECYCLE ====================
  ngOnInit(): void {
    this.loadProfile();
  }


  // ==================== LOAD ====================
  loadProfile(): void {
    this.isLoading.set(true);

    this.profileService.getProfile()
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (res: Result<CustomerProfileModel>) => {
          if (res.isSuccessful && res.data) {
            this.profile.set(res.data);
          } else {
            this.messageService.add({
              severity: 'error',
              summary: 'Hata',
              detail: res.errorMessages?.[0] || 'Profil yüklenemedi.'
            });
          }
        },
        error: (err) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Hata',
            detail: err?.error?.errorMessages?.[0]
              || err?.error?.message
              || 'Sunucu hatası, profil yüklenemedi.'
          });
          console.error(err);
        }
      });
  }

  // ==================== HELPERS ====================
  formatDate(dateStr: string | null): string {
    if (!dateStr) return '-';
    try {
      const date = new Date(dateStr);
      return date.toLocaleDateString('tr-TR', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric'
      });
    } catch {
      return '-';
    }
  }

  formatDateTime(dateStr: string | null): string {
    if (!dateStr) return '-';
    try {
      const date = new Date(dateStr);
      return date.toLocaleDateString('tr-TR', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
      });
    } catch {
      return '-';
    }
  }
}
