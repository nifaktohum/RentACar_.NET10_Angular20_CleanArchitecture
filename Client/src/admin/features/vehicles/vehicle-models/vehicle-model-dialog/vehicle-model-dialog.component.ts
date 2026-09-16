import { ChangeDetectionStrategy, ChangeDetectorRef, Component, effect, inject, input, output, signal, ViewEncapsulation } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DialogModule } from 'primeng/dialog';
import { InputNumber, InputNumberModule } from 'primeng/inputnumber';
import { SelectModule } from 'primeng/select';
import { VehicleModelService } from '../../../../core/services/vehicle-model.service';
import { VehicleTypeService } from '../../../../core/services/vehicle-type.service';
import { MessageService } from 'primeng/api';
import { VehicleType } from '../../../../core/models/vehicle/vehicle-type.model';
import { CreateVehicleModelRequest, UpdateVehicleModelRequest, VehicleModelDetail } from '../../../../core/models/vehicle/VehicleModel.model';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';

@Component({
  selector: 'app-vehicle-model-dialog',
  imports: [
    DialogModule,
    SelectModule,
    InputNumber,
    FormsModule,
    ButtonModule,
    InputTextModule,
    InputNumberModule,
    TextareaModule
  ],
  templateUrl: './vehicle-model-dialog.component.html',
  styleUrl: './vehicle-model-dialog.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.Emulated
})
export class VehicleModelDialogComponent {
  private vehicleModelService = inject(VehicleModelService);
  private vehicleTypeService = inject(VehicleTypeService);
  private messageService = inject(MessageService);
  private cdr = inject(ChangeDetectorRef);

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
  readonly vehicleTypes = signal<VehicleType[]>([]);

  // ==================== FORM DATA ====================
  formData: CreateVehicleModelRequest = {
    brand: '',
    name: '',
    description: null,
    stock: 0,
    vehicleTypeId: ''
  };

  // ==================== COMPUTED ====================
  get dialogTitle(): string {
    return this.mode() === 'create' ? 'Yeni Araç Modeli Ekle' : 'Araç Modeli Düzenle';
  }

  get isEditMode(): boolean {
    return this.mode() === 'edit';
  }

  // ==================== CONSTRUCTOR ====================
  constructor() {
    effect(() => {
      const isVisible = this.visible();
      const id = this.editId();

      if (isVisible) {
        this.loadVehicleTypes();

        if (this.isEditMode && id) {
          this.loadVehicleModel(id);
        } else {
          this.resetForm();
        }
      }
    });
  }

  // ==================== LOAD METHODS ====================
  loadVehicleTypes(): void {
    const params: any = {
      pageNumber: 1,
      pageSize: 100,
      isActive: true
    };
    this.vehicleTypeService.getAllType(params).subscribe({
      next: (res) => {
        if (res.isSuccessful && res.data) {
          this.vehicleTypes.set(res.data.items);
        }
      },
      error: (err) => {
        console.error('Araç tipleri yüklenemedi:', err);
      }
    });
  }

  loadVehicleModel(id: string): void {
    this.isLoading.set(true);

    this.vehicleModelService.getById(id).subscribe({
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
          detail: 'Araç modeli yüklenirken bir hata oluştu.'
        });
        console.error(err);
        this.isLoading.set(false);
        this.closeDialog();
      }
    });
  }

  populateForm(model: VehicleModelDetail): void {
    this.formData = {
      brand: model.brand,
      name: model.name,
      description: model.description,
      stock: model.stock,
      vehicleTypeId: model.vehicleTypeId
    };
  }

  resetForm(): void {
    this.formData = {
      brand: '',
      name: '',
      description: null,
      stock: 0,
      vehicleTypeId: ''
    };
  }

  // ==================== CRUD METHODS ====================
  saveVehicleModel(): void {
    // Validasyon
    if (!this.formData.brand?.trim()) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Uyarı',
        detail: 'Marka zorunludur.'
      });
      return;
    }

    if (!this.formData.name?.trim()) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Uyarı',
        detail: 'Model adı zorunludur.'
      });
      return;
    }

    if (!this.formData.vehicleTypeId) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Uyarı',
        detail: 'Araç tipi seçimi zorunludur.'
      });
      return;
    }

    if (this.formData.stock < 0) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Uyarı',
        detail: 'Stok miktarı negatif olamaz.'
      });
      return;
    }

    this.isSaving.set(true);

    if (this.isEditMode && this.editId()) {
      this.updateVehicleModel();
    } else {
      this.createVehicleModel();
    }
  }

  createVehicleModel(): void {
    this.vehicleModelService.create(this.formData).subscribe({
      next: (response) => {
        this.isSaving.set(false);
        if (response.isSuccessful) {
          this.messageService.add({
            severity: 'success',
            summary: 'Başarılı',
            detail: `"${this.formData.brand} ${this.formData.name}" araç modeli oluşturuldu.`
          });
          this.closeDialog();
          this.saved.emit();
        } else {
          this.messageService.add({
            severity: 'error',
            summary: 'Hata',
            detail: response.errorMessages?.join(', ') || 'Araç modeli oluşturulamadı.'
          });
        }
      },
      error: (err) => {
        this.isSaving.set(false);
        this.messageService.add({
          severity: 'error',
          summary: 'Hata',
          detail: err.error?.errorMessages?.[0] || 'Araç modeli oluşturulurken bir hata oluştu.'
        });
        console.error(err);
      }
    });
  }

  updateVehicleModel(): void {
    const id = this.editId();
    if (!id) return;

    const request: UpdateVehicleModelRequest = {
      id: id,
      ...this.formData
    };

    this.vehicleModelService.update(request).subscribe({
      next: (response) => {
        this.isSaving.set(false);
        if (response.isSuccessful) {
          this.messageService.add({
            severity: 'success',
            summary: 'Başarılı',
            detail: `"${this.formData.brand} ${this.formData.name}" araç modeli güncellendi.`
          });
          this.closeDialog();
          this.saved.emit();
        } else {
          this.messageService.add({
            severity: 'error',
            summary: 'Hata',
            detail: response.errorMessages?.join(', ') || 'Araç modeli güncellenemedi.'
          });
        }
      },
      error: (err) => {
        this.isSaving.set(false);
        this.messageService.add({
          severity: 'error',
          summary: 'Hata',
          detail: err.error?.errorMessages?.[0] || 'Araç modeli güncellenirken bir hata oluştu.'
        });
        console.error(err);
      }
    });
  }

  closeDialog(): void {
    this.visibleChange.emit(false);
    this.resetForm();
  }

}
