import { ChangeDetectionStrategy, Component, effect, inject, OnInit, signal, ViewEncapsulation } from '@angular/core';
import { BreadcrumbService } from '../../../core/services/breadcrumb.service';
import { ProtectionBenefitService } from '../../../core/services/protection-packages/protection-benefit.service';
import { CustomConfirmDialogService } from '../../../shared/services/custom-confirm-dialog.service';
import { MessageService } from 'primeng/api';
import { ProtectionBenefit } from '../../../core/models/protection-package/protection-benefit.model';
import { BreadCrumbModel } from '../../../core/models/breadcrumb';
import { NgClass } from '@angular/common';
import { TagModule } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { CardModule } from 'primeng/card';
import { BenefitDialogComponent } from './benefit-dialog/benefit-dialog.component';
import { BenefitCategoryService } from '../../../core/services/protection-packages/benefit.category.service';
import { BenefitCategory } from '../../../core/models/protection-package/benefit-category/benefit-category.model';

@Component({
  selector: 'app-protection-benefits',
  imports: [
    NgClass,
    TagModule,
    ButtonModule,
    TableModule,
    CardModule,
    BenefitDialogComponent
  ],
  templateUrl: './protection-benefits.component.html',
  styleUrl: './protection-benefits.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.Emulated
})
export class ProtectionBenefitsComponent implements OnInit {
  private breadcrumbService = inject(BreadcrumbService);
  private protectionBenefitService = inject(ProtectionBenefitService);
  private benefitCategoryService = inject(BenefitCategoryService);
  private customConfirmDialogService = inject(CustomConfirmDialogService);
  private messageServiceToast = inject(MessageService);

  // Signals
  readonly benefits = signal<ProtectionBenefit[]>([]);
  readonly totalCount = signal<number>(0);
  readonly isLoading = signal<boolean>(false);
  readonly categories = signal<BenefitCategory[]>([]);

  // Dialog state'leri
  readonly dialogVisible = signal<boolean>(false);
  readonly dialogMode = signal<'create' | 'edit'>('create');
  readonly selectedBenefit = signal<ProtectionBenefit | null>(null);

  // Breadcrumb
  breadcrumbs = signal<BreadCrumbModel[]>([
    {
      title: 'Kapsamlar (Benefits)',
      url: '/admin/protection-benefits',
      icon: 'ri-shield-check-line',
      isActive: true
    }
  ]);

  // ==============================================>
  // Dialog açma metodları
  openCreateDialog(): void {
    this.dialogMode.set('create');
    this.selectedBenefit.set(null);
    this.dialogVisible.set(true);
  }

  openEditDialog(benefit: ProtectionBenefit): void {
    this.dialogMode.set('edit');
    this.selectedBenefit.set(benefit);
    this.dialogVisible.set(true);
  }

  ngOnInit(): void {
    this.breadcrumbService.reset(this.breadcrumbs());
    this.loadBenefits();
    this.loadCategories();
    
  }


  // * Benefit listesini yükler
  loadBenefits(): void {
    this.isLoading.set(true);

    this.protectionBenefitService.getAll().subscribe({
      next: (response) => {
        if (response.isSuccessful && response.data) {
          this.benefits.set(response.data);
          this.totalCount.set(response.data.length);
        }
        this.isLoading.set(false);
      },
      error: (err) => {
        this.messageServiceToast.add({
          severity: 'error',
          summary: 'Hata',
          detail: 'Kapsamlar yüklenirken bir hata oluştu.'
        });
        console.error('Hata:', err?.error?.message || err?.message);
        this.isLoading.set(false);
      }
    });
  }

  // ✅ Kategorileri yükle
  loadCategories(): void {
    this.benefitCategoryService.getAll().subscribe({
      next: (response) => {
        if (response.isSuccessful && response.data) {
          this.categories.set(response.data);
        }
      },
      error: (err) => {
        console.error('Kategoriler yüklenirken hata:', err);
      }
    });
  }
  //  * Listeyi yeniler
  refreshData(): void {
    this.loadBenefits();  
  }

  //  * Kategori ID'sine göre kategori adını getirir
  getCategoryNameById(categoryId: string): string {
    const category = this.categories().find(c => c.id === categoryId);
    return category?.name || 'Bilinmiyor';
  }

  //  * Kategori ID'sine göre kategori severity'ini getirir
  getCategorySeverityById(categoryId: string): 'success' | 'info' | 'warn' | 'danger' | 'secondary' | 'contrast' {
    const severities: Record<string, 'success' | 'info' | 'warn' | 'danger' | 'secondary' | 'contrast'> = {
      // Kategori ID'lerine göre renkler
    };
    return severities[categoryId] || 'secondary';
  }



  //  * Benefit'i siler
  deleteBenefit(id: string, name: string): void {
    this.customConfirmDialogService.showDeleteConfirm(
      name,
      () => {
        this.protectionBenefitService.delete(id).subscribe({
          next: () => {
            this.messageServiceToast.add({
              severity: 'success',
              summary: 'Başarılı',
              detail: `"${name}" kapsamı silindi.`,
              life: 3000
            });
            this.loadBenefits();
          },
          error: (err) => {
            this.messageServiceToast.add({
              severity: 'error',
              summary: 'Hata',
              detail: err?.error?.errorMessages || 'Kapsam silinirken bir hata oluştu.',
              life: 3000
            });
            console.log(err);
            
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

  //  * Benefit aktif/pasif durumunu değiştirir
  toggleStatus(benefit: ProtectionBenefit): void {
    // Basitçe aktif/pasif değiştir - backend'de bu endpoint yoksa sadece mesaj göster
    this.messageServiceToast.add({
      severity: 'info',
      summary: 'Bilgi',
      detail: `${benefit.name} durumu değiştirilemedi.`,
      life: 3000
    });
  }

  getBenefitCountLabel(count: number): string {
    if (count === 0) return 'Henüz benefit yok';
    if (count === 1) return '1 benefit';
    return `${count} benefit`;
  }


}
