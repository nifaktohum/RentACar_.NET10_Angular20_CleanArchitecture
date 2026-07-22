import { ChangeDetectionStrategy, Component, inject, signal, ViewEncapsulation } from '@angular/core';
import { BenefitCategoryDialogComponent } from './benefit-category-dialog/benefit-category-dialog.component';
import { BreadcrumbService } from '../../../../core/services/breadcrumb.service';
import { BenefitCategoryService } from '../../../../core/services/protection-packages/benefit.category.service';
import { CustomConfirmDialogService } from '../../../../shared/services/custom-confirm-dialog.service';
import { ConfirmationService, MessageService } from 'primeng/api';
import { BenefitCategory } from '../../../../core/models/protection-package/benefit-category/benefit-category.model';
import { BreadCrumbModel } from '../../../../core/models/breadcrumb';
import { NgClass } from '@angular/common';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';

@Component({
  selector: 'app-benefit-categories',
  imports: [
    BenefitCategoryDialogComponent,
    NgClass,
    TableModule,
    ButtonModule,
    CardModule,
  ],
  templateUrl: './benefit-categories.component.html',
  styleUrl: './benefit-categories.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.Emulated
})
export class BenefitCategoriesComponent {
  private breadcrumbService = inject(BreadcrumbService);
  private benefitCategoryService = inject(BenefitCategoryService);
  private customConfirmDialogService = inject(CustomConfirmDialogService);
  private messageServiceToast = inject(MessageService);
  private confirmationService = inject(ConfirmationService);

  // Signals
  readonly categories = signal<BenefitCategory[]>([]);
  readonly totalCount = signal<number>(0);
  readonly isLoading = signal<boolean>(false);

  // Dialog state'leri
  readonly dialogVisible = signal<boolean>(false);
  readonly dialogMode = signal<'create' | 'edit'>('create');
  readonly selectedCategory = signal<BenefitCategory | null>(null);

  // Breadcrumb
  breadcrumbs = signal<BreadCrumbModel[]>([
    {
      title: 'Kapsam Kategorileri',
      url: '/admin/benefit-categories',
      icon: 'ri-price-tag-3-line',
      isActive: true
    }
  ]);

  //=======================================> 
  ngOnInit(): void {
    this.breadcrumbService.reset(this.breadcrumbs());
    this.loadCategories();
  }


  // ==================== DIALOG METHODS ====================
  openCreateDialog(): void {
    this.dialogMode.set('create');
    this.selectedCategory.set(null);
    this.dialogVisible.set(true);
  }

  openEditDialog(category: BenefitCategory): void {
    this.dialogMode.set('edit');
    this.selectedCategory.set(category);
    this.dialogVisible.set(true);
  }


  // ==================== LOAD METHODS ====================
  loadCategories(): void {
    this.isLoading.set(true);
    this.benefitCategoryService.getAll().subscribe({
      next: (response) => {
        if (response.isSuccessful && response.data) {
          this.categories.set(response.data);
          this.totalCount.set(response.data.length);
        }
        this.isLoading.set(false);
      },
      error: (err) => {
        this.messageServiceToast.add({
          severity: 'error',
          summary: 'Hata',
          detail: 'Kategoriler yüklenirken bir hata oluştu.'
        });
        this.isLoading.set(false);
        console.error(err);
      }
    });
  }

  refreshData(): void {
    this.loadCategories();
  }

  // ==================== CRUD METHODS ====================
  deleteCategory(id: string, name: string): void {
    this.customConfirmDialogService.showDeleteConfirm(
      name,
      () => {
        this.benefitCategoryService.delete(id).subscribe({
          next: () => {
            this.messageServiceToast.add({
              severity: 'success',
              summary: 'Başarılı',
              detail: `"${name}" kategorisi silindi.`,
              life: 3000
            });
            this.loadCategories();
          },
          error: (err) => {
            this.messageServiceToast.add({
              severity: 'error',
              summary: 'Hata',
              detail: err?.error?.message || 'Kategori silinirken bir hata oluştu.',
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

  toggleStatus(category: BenefitCategory): void {
    const newStatus = !category.isActive;
    const action = newStatus ? 'aktifleştirilecek' : 'pasifleştirilecek';

    this.confirmationService.confirm({
      message: `"${category.name}" kategorisini ${action}. Devam etmek istiyor musunuz?`,
      header: 'Durum Değişikliği',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        const updateData = {
          id: category.id,
          name: category.name,
          slug: category.slug,
          description: category.description,
          icon: category.icon,
          displayOrder: category.displayOrder
        };
        this.benefitCategoryService.update(updateData).subscribe({
          next: (response) => {
            if (response.isSuccessful) {
              this.messageServiceToast.add({
                severity: 'success',
                summary: 'Başarılı',
                detail: `"${category.name}" ${newStatus ? 'aktifleştirildi' : 'pasifleştirildi'}.`,
                life: 3000
              });
              this.loadCategories();
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
      }
    });
  }

  getBenefitCountLabel(count: number): string {
    if (count === 0) return 'Henüz benefit yok';
    if (count === 1) return '1 benefit';
    return `${count} benefit`;
  }
}
