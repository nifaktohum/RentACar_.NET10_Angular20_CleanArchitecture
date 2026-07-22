import { ChangeDetectionStrategy, Component, effect, inject, input, OnInit, output, signal, ViewEncapsulation } from '@angular/core';
import { ProtectionPackage } from '../../../core/models/protection-package/protection-package.model';
import { MessageService } from 'primeng/api';
import { ProtectionPackageService } from '../../../core/services/protection-packages/protection-package.service';
import { ProtectionBenefitService } from '../../../core/services/protection-packages/protection-benefit.service';
import { ToggleSwitchModule } from 'primeng/toggleswitch';
import { FormsModule } from '@angular/forms';
import { DialogModule } from 'primeng/dialog';
import { CreateProtectionPackageRequest, UpdateProtectionPackageRequest } from '../../../core/models/protection-package/protection-package-request.model';
import { SelectModule } from 'primeng/select';
import { ProtectionBenefit } from '../../../core/models/protection-package/protection-benefit.model';
import { MultiSelectModule } from 'primeng/multiselect';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';

@Component({
  selector: 'app-package-dialog',
  imports: [
    ToggleSwitchModule,
    FormsModule,
    DialogModule,
    SelectModule,
    MultiSelectModule,
    ButtonModule,
    InputTextModule,
    TextareaModule
    
  ],
  templateUrl: './package-dialog.component.html',
  styleUrl: './package-dialog.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.Emulated
})
export class PackageDialogComponent implements OnInit{
  private messageService = inject(MessageService);
  private packageService = inject(ProtectionPackageService);
  private benefitService = inject(ProtectionBenefitService);


  // ==================== INPUT ====================
  // Input() yerine input()
  readonly visible = input<boolean>(false);
  readonly editData = input<ProtectionPackage | null>(null);
  readonly mode = input<'create' | 'edit'>('create');

  // ==================== OUTPUTS ====================
  // Output() yerine output()
  readonly visibleChange = output<boolean>();
  readonly saved = output<void>();

  isLoading = signal<boolean>(false);
  isSaving = signal<boolean>(false);
  selectedBenefitIds: string[] = [];
  benefits = signal<ProtectionBenefit[]>([]);
  // ===================================================

  // ==================== FORM MODEL ====================
  formData: CreateProtectionPackageRequest = {
    name: '',
    description: null,
    icon: null,
    displayOrder: 0,
    isRecommended: false,
    starRating: 3,
    protectionLevel: 0,
    deductibleType: 0,
    benefitIds: [],
    pricing: {
      dailyPrice: null,
      deductibleAmount: null,
      isDefault: true,
      validityStart: null,
      validityEnd: null
    },
    isActive: true
  };

  protectionLevels = [
    { label: 'Basic', value: 0 },
    { label: 'Standard', value: 1 },
    { label: 'Plus', value: 2 },
    { label: 'Premium', value: 3 },
    { label: 'Platinum', value: 4 }
  ];

  deductibleTypes = [
    { label: 'Muafiyetli', value: 0 },
    { label: 'Muafiyetsiz', value: 1 },
    { label: 'Düşük Muafiyetli', value: 2 }
  ];

  
  constructor() {
    // ✅ effect ile editData değişimini izle
    effect(() => {
      
      const data = this.editData();
      if (this.isEditMode && data) {
        this.populateForm(data);
      } else {
        this.resetForm();
      }

      
    });

    
  }

  ngOnInit(): void {
    this.loadBenefits();    
  }

  get dialogTitle(): string {
    return this.mode() === 'create' ? 'Yeni Koruma Paketi Ekle' : 'Koruma Paketi Düzenle';
  }

  get isEditMode(): boolean {
    return this.mode() === 'edit';
  }

  get showDailyPrice(): boolean {
    return this.formData.deductibleType === 1;
  }

  get showDeductibleAmount(): boolean {
    return this.formData.deductibleType === 0 || this.formData.deductibleType === 2;
  }

  getBenefitName(id: string): string {
    return this.benefits().find(b => b.id === id)?.name || '';
  }

  savePackage(): void {
    // Validasyon    
    if (!this.formData.name?.trim()) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Uyarı',
        detail: 'Paket adı zorunludur.'
      });
      return;
    }

    if (this.formData.benefitIds.length === 0) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Uyarı',
        detail: 'En az bir kapsam (benefit) seçmelisiniz.'
      });
      return;
    }

    this.isSaving.set(true);

    if (this.isEditMode && this.editData()) {
      this.updatePackage();
    } else {
      this.createPackage();
    }
  }

  createPackage(): void {
    const request: CreateProtectionPackageRequest = {
      ...this.formData,
      benefitIds: this.formData.benefitIds
    };

    this.packageService.create(request).subscribe({
      next: (response) => {
        this.isSaving.set(false);
        if (response.isSuccessful) {
          this.messageService.add({
            severity: 'success',
            summary: 'Başarılı',
            detail: `"${this.formData.name}" paketi oluşturuldu.`
          });
          this.closeDialog();
          this.saved.emit();
        } else {
          this.messageService.add({
            severity: 'error',
            summary: 'Hata',
            detail: response.errorMessages?.join(', ') || 'Paket oluşturulamadı.'
          });
        }
      },
      error: (err) => {
        this.isSaving.set(false);
        this.messageService.add({
          severity: 'error',
          summary: 'Hata',
          detail: err.error?.message || 'Paket oluşturulurken bir hata oluştu.'
        });
        console.error(err);
      }
    });
  }

  updatePackage(): void {
    const editData = this.editData();
    if (!editData) return;

    const request: UpdateProtectionPackageRequest = {
      id: editData.id,
      name: this.formData.name,
      description: this.formData.description,
      icon: this.formData.icon,
      displayOrder: this.formData.displayOrder,
      isRecommended: this.formData.isRecommended,
      starRating: this.formData.starRating,
      protectionLevel: this.formData.protectionLevel,
      deductibleType: this.formData.deductibleType,
      benefitIds: this.selectedBenefitIds,
      pricing: {
        pricingId: editData.pricing?.[0]?.id || null,
        dailyPrice: this.formData.pricing.dailyPrice,
        deductibleAmount: this.formData.pricing.deductibleAmount,
        isDefault: this.formData.pricing.isDefault || true,
        validityStart: this.formData.pricing.validityStart,
        validityEnd: this.formData.pricing.validityEnd,
        isActive: this.formData.isActive
      },
      isActive: this.formData.isActive
    };

    this.packageService.update(request).subscribe({
      next: (response) => {
        this.isSaving.set(false);
        if (response.isSuccessful) {
          this.messageService.add({
            severity: 'success',
            summary: 'Başarılı',
            detail: `"${this.formData.name}" paketi güncellendi.`
          });
          this.closeDialog();
          this.saved.emit();
        } else {
          this.messageService.add({
            severity: 'error',
            summary: 'Hata',
            detail: response.errorMessages?.join(', ') || 'Paket güncellenemedi.'
          });
        }
      },
      error: (err) => {
        this.isSaving.set(false);
        this.messageService.add({
          severity: 'error',
          summary: 'Hata',
          detail: err.error?.message || 'Paket güncellenirken bir hata oluştu.'
        });
        console.error(err);
      }
    });
  }

  loadBenefits(): void {
    this.benefitService.getAll().subscribe({
      next: (response) => {
        if (response.isSuccessful && response.data) {
          this.benefits.set(response.data);
        }
      },
      error: (err) => {
        console.error("Benefit'ler yüklenirken hata: ", err);
      }
    });
  }

  populateForm(pkg: ProtectionPackage): void {
    this.formData = {
      name: pkg.name,
      description: pkg.description || null,
      icon: pkg.icon || null,
      displayOrder: pkg.displayOrder,
      isRecommended: pkg.isRecommended,
      starRating: pkg.starRating,
      protectionLevel: pkg.protectionLevel,
      deductibleType: pkg.deductibleType,
      benefitIds: pkg.benefits?.map(b => b.id) || [],
      pricing: {
        dailyPrice: pkg.pricing?.[0]?.dailyPrice || null,
        deductibleAmount: pkg.pricing?.[0]?.deductibleAmount || null,
        isDefault: pkg.pricing?.[0]?.isDefault || true,
        validityStart: pkg.pricing?.[0]?.validityStart || null,
        validityEnd: pkg.pricing?.[0]?.validityEnd || null
      },
      isActive: pkg.isActive
    };
    
    this.selectedBenefitIds = this.formData.benefitIds;
  }

  closeDialog(): void {
    this.visibleChange.emit(false);
    this.resetForm();
  }

  resetForm(): void {
    this.formData = {
      name: '',
      description: null,
      icon: null,
      displayOrder: 0,
      isRecommended: false,
      starRating: 3,
      protectionLevel: 0,
      deductibleType: 0,
      benefitIds: [],
      pricing: {
        dailyPrice: null,
        deductibleAmount: null,
        isDefault: true,
        validityStart: null,
        validityEnd: null
      },
      isActive: true
    };
    this.selectedBenefitIds = [];
  }
}
