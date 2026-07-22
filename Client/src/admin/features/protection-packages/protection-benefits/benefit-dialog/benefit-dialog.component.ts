import { ChangeDetectionStrategy, ChangeDetectorRef, Component, effect, inject, input, OnInit, output, signal, ViewEncapsulation } from '@angular/core';
import { MessageService } from 'primeng/api';
import { ProtectionBenefitService } from '../../../../core/services/protection-packages/protection-benefit.service';
import { ProtectionBenefit } from '../../../../core/models/protection-package/protection-benefit.model';
import { CreateProtectionBenefitRequest, UpdateProtectionBenefitRequest } from '../../../../core/models/protection-package/protection-benefit-request.model';
import { SelectModule } from 'primeng/select';
import { FormsModule } from '@angular/forms';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { FloatLabelModule } from 'primeng/floatlabel';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { BenefitCategory } from '../../../../core/models/protection-package/benefit-category/benefit-category.model';
import { BenefitCategoryService } from '../../../../core/services/protection-packages/benefit.category.service';

@Component({
  selector: 'app-benefit-dialog',
  imports: [
    SelectModule,
    FormsModule,
    DialogModule,
    ButtonModule,
    FloatLabelModule,
    InputTextModule,
    TextareaModule

  ],
  templateUrl: './benefit-dialog.component.html',
  styleUrl: './benefit-dialog.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.Emulated
})
export class BenefitDialogComponent implements OnInit {
  private messageService = inject(MessageService);
  private protectionBenefitService = inject(ProtectionBenefitService);
  private benefitCategoryService = inject(BenefitCategoryService)

  // ==================== SIGNAL INPUTS ====================
  readonly visible = input<boolean>(false);
  readonly editData = input<ProtectionBenefit | null>(null);
  readonly mode = input<'create' | 'edit'>('create');

  // ==================== OUTPUTS ====================
  readonly visibleChange = output<boolean>();
  readonly saved = output<void>();

  // ==================== SIGNALS ====================
  isSaving = signal<boolean>(false);
  benefitCategories = signal<BenefitCategory[]>([]);

  // ==================== FORM MODEL ====================
  formData: CreateProtectionBenefitRequest = {
    name: '',
    description: null,
    icon: null,
    displayOrder: 0,
    categoryId: ''
  };


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

  ngOnInit(): void {
    // Başlangıçta form reset
    this.loadCategories();
    this.resetForm();

    // ✅ Create modunda otomatik sıralama ata
    if (!this.isEditMode) {
      this.setDefaultDisplayOrder();
    }
  }

  // ==================== COMPUTED ====================
  get dialogTitle(): string {
    return this.mode() === 'create' ? 'Yeni Kapsam Ekle' : 'Kapsam Düzenle';
  }

  get isEditMode(): boolean {
    return this.mode() === 'edit';
  }


  // ==================== METHODS ====================
  populateForm(benefit: ProtectionBenefit): void {

    this.formData = {
      name: benefit.name,
      description: benefit.description || null,
      icon: benefit.icon || null,
      displayOrder: benefit.displayOrder,
      categoryId: benefit.categoryId || ''
    };
  }

  saveBenefit(): void {
    // Validasyon
    if (!this.formData.name?.trim()) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Uyarı',
        detail: 'Kapsam adı zorunludur.'
      });
      return;
    }

    this.isSaving.set(true);

    if (this.isEditMode && this.editData()) {
      this.updateBenefit();
    } else {
      this.createBenefit();
    }
  }

  createBenefit(): void {
    this.protectionBenefitService.create(this.formData).subscribe({
      next: (response) => {
        this.isSaving.set(false);
        if (response.isSuccessful) {
          this.messageService.add({
            severity: 'success',
            summary: 'Başarılı',
            detail: `"${this.formData.name}" kapsamı oluşturuldu.`
          });
          this.closeDialog();
          this.saved.emit();
        } else {
          this.messageService.add({
            severity: 'error',
            summary: 'Hata',
            detail: response.errorMessages?.join(', ') || 'Kapsam oluşturulamadı.'
          });
        }
      },
      error: (err) => {
        this.isSaving.set(false);
        const errorMessage = err.error?.errorMessages?.[0] ||
          err.error?.message ||
          err.message ||
          'Kapsam oluşturulurken bir hata oluştu.';
        this.messageService.add({
          severity: 'error',
          summary: 'Hata',
          detail: errorMessage
        });
        console.error(err);
      }
    });
  }

  updateBenefit(): void {
    const editData = this.editData();
    if (!editData) return;

    const request: UpdateProtectionBenefitRequest = {
      id: editData.id,
      name: this.formData.name,
      description: this.formData.description,
      icon: this.formData.icon,
      displayOrder: this.formData.displayOrder,
      categoryId: this.formData.categoryId
    };

    this.protectionBenefitService.update(request).subscribe({
      next: (response) => {
        this.isSaving.set(false);
        if (response.isSuccessful) {
          this.messageService.add({
            severity: 'success',
            summary: 'Başarılı',
            detail: `"${this.formData.name}" kapsamı güncellendi.`
          });
          this.closeDialog();
          this.saved.emit();
        } else {
          this.messageService.add({
            severity: 'error',
            summary: 'Hata',
            detail: response.errorMessages?.join(', ') || 'Kapsam güncellenemedi.'
          });
        }
      },
      error: (err) => {
        this.isSaving.set(false);
        const errorMessage = err.error?.errorMessages?.[0] ||
          err.error?.message ||
          err.message ||
          'Kapsam güncellenirken bir hata oluştu.';
        this.messageService.add({
          severity: 'error',
          summary: 'Hata',
          detail: errorMessage
        });
        console.error(err);
      }
    });
  }

  loadCategories(): void {
    this.benefitCategoryService.getAll().subscribe({
      next: (response) => {
        if (response.isSuccessful && response.data) {
          this.benefitCategories.set(response.data);
        }
      },
      error: (err) => {
        console.error('Kategoriler yüklenirken hata:', err);
      }
    });
  }

  resetForm(): void {
    this.formData = {
      name: '',
      description: null,
      icon: null,
      displayOrder: 0,
      categoryId: ''
    };
  }

  clearCategory(): void {
    this.formData.categoryId = '';
    this.messageService.add({
      severity: 'info',
      summary: 'Bilgi',
      detail: 'Kategori bağlantısı kaldırıldı. Artık benefit\'i silebilirsiniz.'
    });
  }

  closeDialog(): void {
    this.visibleChange.emit(false);
    this.resetForm();
  }

  //  * Mevcut benefit'lerin en yüksek displayOrder'undan sonraki sırayı ata
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
