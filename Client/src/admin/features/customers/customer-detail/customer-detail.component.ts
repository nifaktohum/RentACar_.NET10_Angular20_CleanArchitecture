import { ChangeDetectionStrategy, Component, computed, inject, OnInit, signal, ViewEncapsulation } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { DividerModule } from 'primeng/divider';
import { SkeletonModule } from 'primeng/skeleton';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { PhoneFormatPipe } from '../../../../shared/pipes/phone-format.pipe';
import { MessageService } from 'primeng/api';
import { BreadcrumbService } from '../../../core/services/breadcrumb.service';
import { CustomerManagementService } from '../../../core/services/customers/customer-management.service';
import { CustomConfirmDialogService } from '../../../shared/services/custom-confirm-dialog.service';
import { CustomerDetailDto } from '../../../../core/models/customer.model';
import { BreadCrumbModel } from '../../../core/models/breadcrumb';
import { Result } from '../../../../core/models/result.model';
import { finalize } from 'rxjs';
import { BlacklistDialogComponent } from './blacklist-dialog/blacklist-dialog.component';
import { FindeksDialogComponent } from './findeks-dialog/findeks-dialog.component';

@Component({
  selector: 'app-customer-detail',
  imports: [
    RouterLink,
    CardModule,
    ButtonModule,
    TagModule,
    TooltipModule,
    SkeletonModule,
    DividerModule,
    PhoneFormatPipe,
    // Component
    BlacklistDialogComponent,
    FindeksDialogComponent     
  ],
  templateUrl: './customer-detail.component.html',
  styleUrl: './customer-detail.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.Emulated
})
export class CustomerDetailComponent implements OnInit {
  private customerService = inject(CustomerManagementService);
  private breadcrumbService = inject(BreadcrumbService);
  private messageService = inject(MessageService);
  private confirmDialog = inject(CustomConfirmDialogService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  // ==================== STATE ====================
  customerId = signal<string | null>(null);
  customer = signal<CustomerDetailDto | null>(null);
  isLoading = signal<boolean>(true);

  blacklistDialogVisible = signal<boolean>(false);
  findeksDialogVisible = signal<boolean>(false); 

  // ==================== COMPUTED ====================
  fullName = computed(() => {
    const c = this.customer();
    if (!c) return '';
    if (c.type === 'Individual') {
      return `${c.firstName || ''} ${c.lastName || ''}`.trim() || 'İsimsiz';
    }
    return c.companyName || 'Kurumsal Müşteri';
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
    return this.customer()?.type === 'Corporate' ? 'Kurumsal' : 'Bireysel';
  });

  customerTypeSeverity = computed<'info' | 'success'>(() => {
    return this.customer()?.type === 'Corporate' ? 'info' : 'success';
  });

  genderLabel = computed(() => {
    const g = this.customer()?.gender;
    if (!g) return '-';
    switch (g) {
      case 'Male': return 'Erkek';
      case 'Female': return 'Kadın';
      default: return g;
    }
  });

  findeksStatus = computed(() => {
    const score = this.customer()?.findeksScore;
    if (score == null) return { label: 'Belirsiz', severity: 'secondary' as const };

    if (score >= 1500) return { label: 'Çok İyi', severity: 'success' as const };
    if (score >= 1200) return { label: 'İyi', severity: 'info' as const };
    if (score >= 900) return { label: 'Orta', severity: 'warn' as const };
    return { label: 'Düşük', severity: 'danger' as const };
  });

  findeksPercent = computed(() => {
    const score = this.customer()?.findeksScore;
    if (score == null) return 0;
    return Math.round((score / 1900) * 100);
  });

  // ==================== BREADCRUMB ====================
  breadcrumbs = signal<BreadCrumbModel[]>([
    {
      title: 'Müşteriler',
      url: '/admin/customers',
      icon: 'ri-group-line',
      isActive: false
    }
  ]);

  // ==================== LIFECYCLE ====================
  ngOnInit(): void {
    this.breadcrumbService.reset(this.breadcrumbs());
    this.route.params.subscribe(params => {
      const id = params['id'];
      if (id) {
        this.customerId.set(id);
        this.loadCustomer(id);
      } else {
        this.router.navigate(['/admin/customers']);
      }
    });
  }

  // ==================== LOAD ====================
  loadCustomer(id: string): void {
    this.isLoading.set(true);

    this.customerService.getCustomerById(id)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (res: Result<CustomerDetailDto>) => {
          if (res.isSuccessful && res.data) {
            this.customer.set(res.data);
            this.updateBreadcrumb(res.data);
          } else {
            this.messageService.add({
              severity: 'error',
              summary: 'Hata',
              detail: res.errorMessages?.[0] || 'Müşteri bulunamadı.'
            });
            this.router.navigate(['/admin/customers']);
          }
        },
        error: (err) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Hata',
            detail: err?.error?.errorMessages?.[0]
              || err?.error?.message
              || 'Müşteri bilgileri yüklenemedi.'
          });
          console.error(err);
          this.router.navigate(['/admin/customers']);
        }
      });
  }

  private updateBreadcrumb(customer: CustomerDetailDto): void {
    const name = customer.type === 'Individual'
      ? `${customer.firstName || ''} ${customer.lastName || ''}`.trim()
      : customer.companyName || 'Detay';

    this.breadcrumbService.reset([
      {
        title: 'Müşteriler',
        url: '/admin/customers',
        icon: 'ri-group-line',
        isActive: false
      },
      {
        title: name || 'Detay',
        url: `/admin/customers/detail/${customer.id}`,
        icon: 'ri-user-line',
        isActive: true
      }
    ]);
  }

  removeFromBlacklist(): void {
    const id = this.customerId();
    const name = this.fullName();

    if (!id) return;

    this.confirmDialog.showConfirm(
      `${name}`,
      "Müşteriyi karaliste'den çıkar.",
      () => {
        this.customerService.removeFromBlacklist(id).subscribe({
          next: (res: Result<string>) => {
            if (res.isSuccessful) {
              this.messageService.add({
                severity: 'success',
                summary: name, // İsmi kalın harflerle üstte gösterir (Örn: Ahmet Yılmaz)
                detail: 'Kara listeden çıkarıldı.' // İşlemi altta gösterir
              });
              this.loadCustomer(id);   // ⭐ Detayı yeniden yükle
            } else {
              this.messageService.add({
                severity: 'warn',
                summary: 'Uyarı',
                detail: res.errorMessages?.[0] || 'İşlem başarısız.'
              });
            }
          },
          error: (err) => {
            this.messageService.add({
              severity: 'error',
              summary: 'Hata',
              detail: err?.error?.errorMessages?.[0]
                || err?.error?.message
                || 'Sunucu hatası.'
            });
          }
        });
      }
    );
  }


  // ==================== HELPERS ====================
  formatDate(dateStr: string | null): string {
    if (!dateStr) return '-';
    try {
      return new Date(dateStr).toLocaleDateString('tr-TR', {
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
      return new Date(dateStr).toLocaleDateString('tr-TR', {
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

  // ==================== ACTIONS ====================
  openBlacklistDialog(): void {
    this.blacklistDialogVisible.set(true);
  }

  openFindeksDialog(): void {
    this.findeksDialogVisible.set(true);   // ⭐ Dialog aç
  }

  onBlacklistSuccess(): void {
    const id = this.customerId();
    if (id) this.loadCustomer(id);
  }

  onFindeksSuccess(): void {
    const id = this.customerId();
    if (id) this.loadCustomer(id);   // ⭐ Detayı yenile
  }














}
