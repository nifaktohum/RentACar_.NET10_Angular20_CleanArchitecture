import { ChangeDetectionStrategy, ChangeDetectorRef, Component, effect, inject, input, model, OnInit, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { BenefitCategoryService } from '../../../../../core/services/protection-packages/benefit.category.service';
import { BenefitCategory } from '../../../../../core/models/protection-package/benefit-category/benefit-category.model';
import { CreateBenefitCategoryRequest } from '../../../../../core/models/protection-package/benefit-category/CreateBenefitCategoryRequest';
import { UpdateBenefitCategoryRequest } from '../../../../../core/models/protection-package/benefit-category/UpdateBenefitCategoryRequest';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';

@Component({
  selector: 'app-benefit-category-dialog',
  imports: [
    DialogModule,
    ButtonModule,
    FormsModule,
    InputTextModule,
    TextareaModule,
  ],
  templateUrl: './benefit-category-dialog.component.html',
  styleUrl: './benefit-category-dialog.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BenefitCategoryDialogComponent  {
  private messageService = inject(MessageService);
  private benefitCategoryService = inject(BenefitCategoryService);

  // ==================== INPUTS ====================
  readonly visible = input<boolean>(false);
  readonly editData = input<BenefitCategory | null>(null);
  readonly mode = input<'create' | 'edit'>('create');

  // ==================== OUTPUTS ====================
  readonly visibleChange = output<boolean>();
  readonly saved = output<void>();

  // ==================== SIGNALS ====================
  isSaving = signal<boolean>(false);

  // ==================== FORM MODEL ====================
  formData: CreateBenefitCategoryRequest = {
    name: '',
    slug: '',
    description: null,
    icon: null,
    displayOrder: 0
  };

  get dialogTitle(): string {
    return this.mode() === 'create' ? 'Yeni Kapsam Kategorisi Ekle' : 'Kapsam Kategorisi Düzenle';
  }

  get isEditMode(): boolean {
    return this.mode() === 'edit';
  }

  constructor() {
    effect(() => {
      const isVisible = this.visible(); // Dialog görünürlüğü takip ediliyor
      const data = this.editData();

      if (isVisible) {
        if (this.isEditMode && data) {
          this.populateForm(data);
        } else {
          this.resetForm();
          this.setDefaultDisplayOrder(); // Her açıldığında güncel sırayı çeker
        }
      }
    });
  }


  // ==================== FORM METHODS ====================
  populateForm(category: BenefitCategory): void {
    this.formData = {
      name: category.name,
      slug: category.slug,
      description: category.description || null,
      icon: category.icon || 'ri-price-tag-3-lin',
      displayOrder: category.displayOrder
    };
  }

  resetForm(): void {
    this.formData = {
      name: '',
      slug: '',
      description: null,
      icon: null,
      displayOrder: 0
    };
  }

  generateSlug(): void {
    if (this.formData.name) {
      this.formData.slug = this.formData.name
        .toLowerCase()
        .replace(/\s+/g, '-')
        .replace(/[^a-z0-9-]/g, '');
    }
  }

  // ==================== DIALOG METHODS ====================
  closeDialog(): void {
    this.visibleChange.emit(false);
    this.resetForm();
  }

  // ==================== CRUD METHODS ====================
  saveCategory(): void {
    // Validasyon
    if (!this.formData.name?.trim()) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Uyarı',
        detail: 'Kategori adı zorunludur.'
      });
      return;
    }

    if (!this.formData.slug?.trim()) {
      this.generateSlug();
    }

    this.isSaving.set(true);

    if (this.isEditMode && this.editData()) {
      this.updateCategory();
    } else {
      this.createCategory();
    }
  }

  // ======================================> 

  createCategory(): void {
    this.benefitCategoryService.create(this.formData).subscribe({
      next: (response) => {
        this.isSaving.set(false);
        if (response.isSuccessful) {
          this.messageService.add({
            severity: 'success',
            summary: 'Başarılı 🎉',
            detail: `"${this.formData.name}" kategorisi oluşturuldu.`
          });
          this.closeDialog();
          this.saved.emit();
        } else {
          this.messageService.add({
            severity: 'error',
            summary: 'Hata',
            detail: response.errorMessages?.join(', ') || 'Kategori oluşturulamadı.'
          });
        }
      },
      error: (err) => {
        this.isSaving.set(false);
        const errorMessage = err.error?.errorMessages?.[0] ||
          err.error?.message ||
          err.message ||
          'Kategori oluşturulurken bir hata oluştu.';
        this.messageService.add({
          severity: 'error',
          summary: 'Hata',
          detail: errorMessage
        });
        console.error('Create Category Error:', err);
      }
    });
  }

  updateCategory(): void {
    const editData = this.editData();
    if (!editData) return;

    const request: UpdateBenefitCategoryRequest = {
      id: editData.id,
      name: this.formData.name,
      slug: this.formData.slug,
      description: this.formData.description,
      icon: this.formData.icon,
      displayOrder: this.formData.displayOrder
    };

    this.benefitCategoryService.update(request).subscribe({
      next: (response) => {
        this.isSaving.set(false);
        if (response.isSuccessful) {
          this.messageService.add({
            severity: 'success',
            summary: 'Başarılı 🎉',
            detail: `"${this.formData.name}" kategorisi güncellendi.`
          });
          this.closeDialog();
          this.saved.emit();
        } else {
          this.messageService.add({
            severity: 'error',
            summary: 'Hata',
            detail: response.errorMessages?.join(', ') || 'Kategori güncellenemedi.'
          });
        }
      },
      error: (err) => {
        this.isSaving.set(false);
        const errorMessage = err.error?.errorMessages?.[0] ||
          err.error?.message ||
          err.message ||
          'Kategori güncellenirken bir hata oluştu.';
        this.messageService.add({
          severity: 'error',
          summary: 'Hata',
          detail: errorMessage
        });
        console.error('Update Category Error:', err);
      }
    });
  }

  private cdr = inject(ChangeDetectorRef);
  //  * Mevcut benefit'lerin en yüksek displayOrder'undan sonraki sırayı ata
  setDefaultDisplayOrder(): void {
    this.benefitCategoryService.getAll().subscribe({
      next: (response) => {
        if (response.isSuccessful && response.data) {
          const maxOrder = response.data.reduce((max, item) => {
            return item.displayOrder > max ? item.displayOrder : max;
          }, 0);

          this.formData.displayOrder = maxOrder + 1;

          // 🚀 Değişimin ekrana hemen yansıması için tetikliyoruz
          this.cdr.detectChanges();
        }
      },
      error: (err) => {
        console.error('Sıralama hesaplanırken hata:', err);
        this.formData.displayOrder = 1;
        this.cdr.detectChanges();
      }
    });
  }
}
