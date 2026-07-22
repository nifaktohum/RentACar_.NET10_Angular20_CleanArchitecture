import { ChangeDetectionStrategy, Component, effect, inject, input, OnInit, output, signal, ViewEncapsulation } from '@angular/core';
import { BreadCrumbModel } from '../../core/models/breadcrumb';
import { BreadcrumbService } from '../../core/services/breadcrumb.service';
import { ProtectionPackage } from '../../core/models/protection-package/protection-package.model';
import { ProtectionPackageService } from '../../core/services/protection-packages/protection-package.service';
import { CustomConfirmDialogService } from '../../shared/services/custom-confirm-dialog.service';
import { ConfirmationService, MessageService } from 'primeng/api';
import { CurrencyPipe, NgClass } from '@angular/common';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { PackageDialogComponent } from './package-dialog/package-dialog.component';

@Component({
  selector: 'app-protection-packages',
  imports: [
    CurrencyPipe,
    NgClass,
    TableModule,
    ButtonModule,
    CardModule,
    PackageDialogComponent
  ],
  templateUrl: './protection-packages.component.html',
  styleUrl: './protection-packages.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.Emulated
})
export class ProtectionPackagesComponent implements OnInit {
  private braadCrumbService = inject(BreadcrumbService);
  private protectionPackageService = inject(ProtectionPackageService);
  private customConfirmDialogService = inject(CustomConfirmDialogService);
  private messageServiceToast = inject(MessageService);
  private confirmationService = inject(ConfirmationService);

  // Signals
  readonly packages = signal<ProtectionPackage[]>([]);
  readonly totalCount = signal<number>(0);
  readonly isLoading = signal<boolean>(false);

  // Dialog state'leri
  readonly dialogVisible = signal<boolean>(false);
  readonly dialogMode = signal<'create' | 'edit'>('create');
  readonly selectedPackage = signal<ProtectionPackage | null>(null);


  // Breadcrumb
  breadcrumbs = signal<BreadCrumbModel[]>([
    {
      title: 'Koruma Paketleri',
      url: '/admin/protection-packages',
      icon: 'ri-shield-star-line',
      isActive: true
    }
  ]);


  // ==============================================>

  ngOnInit(): void {
    this.braadCrumbService.reset(this.breadcrumbs())
    this.loadPackages();
  };

  // Dialog açma metodları
  openCreateDialog(): void {
    this.dialogMode.set('create');
    this.selectedPackage.set(null);
    this.dialogVisible.set(true);
  }

  openEditDialog(pkg: ProtectionPackage): void {
    this.dialogMode.set('edit');
    this.selectedPackage.set(pkg);    
    this.dialogVisible.set(true);
  }

  //  * Paket listesini yükler
  loadPackages(page: number = 1, pageSize: number = 10) {
    this.isLoading.set(true);

    this.protectionPackageService.getAll().subscribe({
      next: (response) => {
        if (response.isSuccessful && response.data) {
          this.packages.set(response.data ?? []);
          this.totalCount.set(response.data.length);
        }
        this.isLoading.set(false);
      },
      error: (err) => {
        this.messageServiceToast.add({
          severity: 'error',
          summary: 'Hata',
          detail: 'Paketler yüklenirken bir hata oluştu.'
        });
        console.error('Hata:', err?.error?.message || err?.message);
        this.isLoading.set(false);
      }
    });
  }

  //  * Listeyi yeniler
  refreshData() {
    this.loadPackages();
  }

  //  * Paketi siler (Soft Delete)
  deletePackage(id: string, name: string) {
    this.customConfirmDialogService.showDeleteConfirm(
      name,
      () => {
        this.protectionPackageService.delete(id).subscribe({
          next: () => {
            this.messageServiceToast.add({
              severity: 'success',
              summary: 'Silme başarılı',
              detail: `"${name}" paketi silindi.`,
              life: 3000
            });
            this.loadPackages();
          },
          error: (err) => {
            console.error('Silme işlemi esnasında hata oluştu:', err);
            this.messageServiceToast.add({
              severity: 'error',
              summary: 'Hata',
              detail: err?.error?.message || 'Paket silinirken bir hata meydana geldi.',
              life: 3000
            });
          }
        });
      },
      () => {
        this.messageServiceToast.add({
          severity: 'info',
          summary: 'İptal Edildi',
          detail: `Silme işlemi iptal edildi: ${name}`,
          life: 3000
        });
      }
    );
  }

  // * Paketi aktif/pasif yapar
  // ✅ Durum değişikliği (Aktif/Pasif)
  toggleStatus(pkg: ProtectionPackage): void {
    const newStatus = !pkg.isActive;

    this.customConfirmDialogService.showStatusChangeConfirm(
      pkg.name,
      newStatus,
      () => {
        this.protectionPackageService.toggleStatus(pkg.id, newStatus).subscribe({
          next: (response) => {
            if (response.isSuccessful) {
              this.messageServiceToast.add({
                severity: 'success',
                summary: 'Başarılı',
                detail: `"${pkg.name}" ${newStatus ? 'aktifleştirildi' : 'pasifleştirildi'}.`,
                life: 3000
              });
              this.loadPackages();
            }
          },
          error: (err) => {
            this.messageServiceToast.add({
              severity: 'error',
              summary: 'Hata',
              detail: err?.error?.message || 'Durum değiştirilemedi.',
              life: 3000
            });
          }
        });
      },
      () => {
        this.messageServiceToast.add({
          severity: 'info',
          summary: 'İptal Edildi',
          detail: `Durum değişikliği iptal edildi: ${pkg.name}`,
          life: 3000
        });
      }
    );
  }

  //  * Yıldız sayısını dizi olarak döner (UI'da göstermek için)
  getStars(rating: number): number[] {
    return Array(rating).fill(0);
  }

  //  * Koruma seviyesi label'ı
  getProtectionLevelLabel(level: number): string {
    const levels = ['Basic', 'Standard', 'Plus', 'Premium', 'Platinum'];
    return levels[level] || 'Bilinmiyor';
  }

  //  * Muafiyet tipi label'ı
  getDeductibleTypeLabel(type: number): string {
    const types = ['Muafiyetli', 'Muafiyetsiz', 'Düşük Muafiyetli'];
    return types[type] || 'Bilinmiyor';
  }

  //  * Tablo lazy load event'i
  onLazyLoad(event: any) {
    const first = event.first ?? 0;
    const rows = event.rows ?? 10;
    const pageIndex = (first / rows) + 1;
    this.loadPackages(pageIndex, rows);
  }

}
