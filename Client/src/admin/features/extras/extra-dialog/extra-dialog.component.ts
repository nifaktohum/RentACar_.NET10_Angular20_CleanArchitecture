import { ChangeDetectionStrategy, ChangeDetectorRef, Component, effect, inject, input, output, signal } from '@angular/core';
import { ExtraService } from '../../../core/services/extra.service';
import { MessageService } from 'primeng/api';
import { FormsModule } from '@angular/forms';
import { DialogModule } from 'primeng/dialog';
import { CreateExtraRequest } from '../../../core/models/extra/createExtraRequest';
import { ExtraCategoryOptions, ExtraCategoryValues, ExtraPriceTypeOptions, PriceTypeValues } from '../../../core/models/extra/enum/extra-enums.model';
import { Extra } from '../../../core/models/extra/extra';
import { UpdateExtraRequest } from '../../../core/models/extra/updateExtraRequest';
import { SelectModule } from 'primeng/select';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { ToggleSwitchModule } from 'primeng/toggleswitch';

@Component({
  selector: 'app-extra-dialog',
  imports: [
    FormsModule,
    DialogModule,
    SelectModule,
    ButtonModule,
    InputTextModule,
    TextareaModule,
    ToggleSwitchModule
  ],
  templateUrl: './extra-dialog.component.html',
  styleUrl: './extra-dialog.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ExtraDialogComponent {
  private extraService = inject(ExtraService);
  private messageService = inject(MessageService);

  // ==================== INPUTS ====================
  readonly visible = input<boolean>(false);
  readonly editId = input<string | null>(null);
  readonly mode = input<'create' | 'edit'>('create');

  // ==================== OUTPUTS ====================
  readonly visibleChange = output<boolean>();
  readonly saved = output<void>();

  // ==================== SIGNALS ====================
  readonly isSaving = signal<boolean>(false);
  readonly isLoading = signal<boolean>(false);

  // ==================== FORM DATA ====================
  formData: CreateExtraRequest = {
    name: '',
    description: null,
    icon: null,
    price: 0,
    priceType: PriceTypeValues.Daily,
    category: ExtraCategoryValues.Guarantee,
    displayOrder: 0,
    isRecommended: false,
    minAge: null,
    ageRange: null,
    stockLimit: null,
    isActive: true
  };

  // ==================== OPTIONS ====================
  readonly categoryOptions = ExtraCategoryOptions;
  readonly priceTypeOptions = ExtraPriceTypeOptions;

  // ==================== COMPUTED ====================
  get dialogTitle(): string {
    return this.mode() === 'create' ? 'Yeni Ekstra Hizmet Ekle' : 'Ekstra Hizmet Düzenle';
  }

  get isEditMode(): boolean {
    return this.mode() === 'edit';
  }

  // ==================== CONSTRUCTOR ====================
  constructor() {
    // Dialog açıldığında veya editId değiştiğinde formu doldur
    effect(() => {
      const isVisible = this.visible();
      const id = this.editId();

      if (isVisible) {
        if (this.isEditMode && id) {
          this.loadExtra(id);
        } else {
          this.resetForm();
          this.setDefaultDisplayOrder(); // Her açıldığında güncel sırayı çeker
        }
      }
    });

    console.log(this.formData);
    
  }

  // ==================== LOAD METHODS ====================
  loadExtra(id: string): void {
    this.isLoading.set(true);

    this.extraService.getById(id).subscribe({
      next: (response) => {
        if (response.isSuccessful && response.data) {
          this.populateForm(response.data);
        }
        this.isLoading.set(false);
      },
      error: (err) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Hata',
          detail: 'Ekstra hizmet yüklenirken bir hata oluştu.'
        });
        console.error(err);
        this.isLoading.set(false);
        this.closeDialog();
      }
    });
  }

  populateForm(extra: Extra): void {
    // Enum string'leri number'a çevir
    const priceTypeMap: Record<string, number> = {
      'Daily': PriceTypeValues.Daily,
      'Rental': PriceTypeValues.Rental
    };

    const categoryMap: Record<string, number> = {
      'Guarantee': ExtraCategoryValues.Guarantee,
      'Driver': ExtraCategoryValues.Driver,
      'Seat': ExtraCategoryValues.Seat,
      'Other': ExtraCategoryValues.Other
    };

    this.formData = {
      name: extra.name,
      description: extra.description,
      icon: extra.icon,
      price: extra.price,
      priceType: priceTypeMap[extra.priceType] || PriceTypeValues.Daily,
      category: categoryMap[extra.category] || ExtraCategoryValues.Guarantee,
      displayOrder: extra.displayOrder,
      isRecommended: extra.isRecommended,
      minAge: extra.minAge,
      ageRange: extra.ageRange,
      stockLimit: extra.stockLimit,
      isActive: extra.isActive
    };
  }

  resetForm(): void {
    this.formData = {
      name: '',
      description: null,
      icon: null,
      price: 0,
      priceType: PriceTypeValues.Daily,
      category: ExtraCategoryValues.Guarantee,
      displayOrder: 0,
      isRecommended: false,
      minAge: null,
      ageRange: null,
      stockLimit: null,
      isActive: true
    };
  }

  // ==================== CRUD METHODS ====================
  saveExtra(): void {
    // Validasyon
    if (!this.formData.name?.trim()) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Uyarı',
        detail: 'Hizmet adı zorunludur.'
      });
      return;
    }

    if (this.formData.price <= 0) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Uyarı',
        detail: 'Fiyat 0\'dan büyük olmalıdır.'
      });
      return;
    }

    this.isSaving.set(true);

    if (this.isEditMode && this.editId()) {
      this.updateExtra();
    } else {
      this.createExtra();
    }
  }


  createExtra(): void {
    const request: CreateExtraRequest = this.formData;

    this.extraService.create(request).subscribe({
      next: (response) => {
        this.isSaving.set(false);
        if (response.isSuccessful) {
          this.messageService.add({
            severity: 'success',
            summary: 'Başarılı',
            detail: `"${this.formData.name}" ekstra hizmeti oluşturuldu.`
          });
          this.closeDialog();
          this.saved.emit();
        } else {
          this.messageService.add({
            severity: 'error',
            summary: 'Hata',
            detail: response.errorMessages?.join(', ') || 'Ekstra hizmet oluşturulamadı.'
          });
        }
      },
      error: (err) => {
        this.isSaving.set(false);
        const errorMessage = err.error?.errorMessages?.[0] ||
          err.error?.message ||
          err.message ||
          'Ekstra hizmet oluşturulurken bir hata oluştu.';
        this.messageService.add({
          severity: 'error',
          summary: 'Hata',
          detail: errorMessage
        });
        console.error(err);
      }
    });
  }

  updateExtra(): void {
    const id = this.editId();
    if (!id) return;

    const request: UpdateExtraRequest = {
      id: id,
      name: this.formData.name,
      description: this.formData.description,
      icon: this.formData.icon,
      price: this.formData.price,
      priceType: this.formData.priceType,
      category: this.formData.category,
      displayOrder: this.formData.displayOrder,
      isRecommended: this.formData.isRecommended,
      minAge: this.formData.minAge,
      ageRange: this.formData.ageRange,
      stockLimit: this.formData.stockLimit,
      isActive: this.formData.isActive
    };

    this.extraService.update(request).subscribe({
      next: (response) => {
        this.isSaving.set(false);
        if (response.isSuccessful) {
          this.messageService.add({
            severity: 'success',
            summary: 'Başarılı',
            detail: `"${this.formData.name}" ekstra hizmeti güncellendi.`
          });
          this.closeDialog();
          this.saved.emit();
        } else {
          this.messageService.add({
            severity: 'error',
            summary: 'Hata',
            detail: response.errorMessages?.join(', ') || 'Ekstra hizmet güncellenemedi.'
          });
        }
      },
      error: (err) => {
        this.isSaving.set(false);
        const errorMessage = err.error?.errorMessages?.[0] ||
          err.error?.message ||
          err.message ||
          'Ekstra hizmet güncellenirken bir hata oluştu.';
        this.messageService.add({
          severity: 'error',
          summary: 'Hata',
          detail: errorMessage
        });
        console.error(err);
      }
    });
  }

  closeDialog(): void {
    this.visibleChange.emit(false);
    this.resetForm();
  }


  //  * Mevcut benefit'lerin en yüksek displayOrder'undan sonraki sırayı ata
  private cdr = inject(ChangeDetectorRef);
  setDefaultDisplayOrder(): void {
    this.extraService.getAll().subscribe({
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


  
