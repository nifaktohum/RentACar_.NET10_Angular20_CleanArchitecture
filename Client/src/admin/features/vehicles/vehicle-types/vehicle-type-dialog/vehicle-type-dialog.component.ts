import { ChangeDetectionStrategy, ChangeDetectorRef, Component, effect, inject, input, output, signal, ViewEncapsulation } from '@angular/core';
import { VehicleTypeService } from '../../../../core/services/vehicle-type.service';
import { MessageService } from 'primeng/api';
import { DialogModule } from 'primeng/dialog';
import { CreateVehicleTypeRequest, UpdateVehicleTypeRequest, VehicleType, VehicleTypeDetail } from '../../../../core/models/vehicle/vehicle-type.model';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { ToggleSwitchModule } from 'primeng/toggleswitch';

@Component({
  selector: 'app-vehicle-type-dialog',
  imports: [
    DialogModule,
    FormsModule,
    ButtonModule,
    DialogModule,
    SelectModule,
    InputTextModule,
    TextareaModule,
    ToggleSwitchModule
  ],
  templateUrl: './vehicle-type-dialog.component.html',
  styleUrl: './vehicle-type-dialog.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.Emulated
})
export class VehicleTypeDialogComponent {
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

  // ==================== FORM DATA ====================
  formData: CreateVehicleTypeRequest = {
    name: '',
    description: null,
    icon: null,
    displayOrder: 0
  };

  // ==================== COMPUTED ====================
  get dialogTitle(): string {
    return this.mode() === 'create' ? 'Yeni Araç Tipi Ekle' : 'Araç Tipi Düzenle';
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
        if (this.isEditMode && id) {
          this.loadVehicleType(id);      
        } else {
          this.resetForm();
          this.setDefaultDisplayOrder();
        }
      }
    });
    
  }


  // ==================== LOAD METHODS ====================
  loadVehicleType(id: string): void {
    this.isLoading.set(true);

    this.vehicleTypeService.getById(id).subscribe({
      next: (response) => {
        if (response.isSuccessful && response.data) {
          this.populateForm(response.data);
          console.log(response.data);
          
        }
        this.isLoading.set(false);
      },
      error: (err) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Hata',
          detail: 'Araç tipi yüklenirken bir hata oluştu.'
        });
        console.error(err);
        this.isLoading.set(false);
        this.closeDialog();
      }
    });
  }

  populateForm(vehicleType: VehicleTypeDetail): void {
    this.formData = {
      name: vehicleType.name,
      description: vehicleType.description,
      icon: vehicleType.icon,
      displayOrder: vehicleType.displayOrder
    };
  }

  resetForm(): void {
    this.formData = {
      name: '',
      description: null,
      icon: null,
      displayOrder: 0
    };
  }


  saveVehicleType(): void {
    // Validasyon
    if (!this.formData.name?.trim()) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Uyarı',
        detail: 'Araç tipi adı zorunludur.'
      });
      return;
    }

    this.isSaving.set(true);

    if (this.isEditMode && this.editId()) {
      this.updateVehicleType();
    } else {
      this.createVehicleType();
    }
  }

  createVehicleType(): void {
    const request: CreateVehicleTypeRequest = this.formData;

    this.vehicleTypeService.create(request).subscribe({
      next: (response) => {
        this.isSaving.set(false);
        if (response.isSuccessful) {
          this.messageService.add({
            severity: 'success',
            summary: 'Başarılı',
            detail: `"${this.formData.name}" araç tipi oluşturuldu.`
          });
          this.closeDialog();
          this.saved.emit();
        } else {
          this.messageService.add({
            severity: 'error',
            summary: 'Hata',
            detail: response.errorMessages?.join(', ') || 'Araç tipi oluşturulamadı.'
          });
        }
      },
      error: (err) => {
        this.isSaving.set(false);
        const errorMessage = err.error?.errorMessages?.[0] ||
          err.error?.message ||
          err.message ||
          'Araç tipi oluşturulurken bir hata oluştu.';
        this.messageService.add({
          severity: 'error',
          summary: 'Hata',
          detail: errorMessage
        });
        console.error(err);
      }
    });
  }

  updateVehicleType(): void {
    const id = this.editId();
    if (!id) return;

    const request: UpdateVehicleTypeRequest = {
      id: id,
      name: this.formData.name,
      description: this.formData.description,
      icon: this.formData.icon,
      displayOrder: this.formData.displayOrder
    };

    this.vehicleTypeService.update(request).subscribe({
      next: (response) => {
        this.isSaving.set(false);
        if (response.isSuccessful) {
          this.messageService.add({
            severity: 'success',
            summary: 'Başarılı',
            detail: `"${this.formData.name}" araç tipi güncellendi.`
          });
          this.closeDialog();
          this.saved.emit();
        } else {
          this.messageService.add({
            severity: 'error',
            summary: 'Hata',
            detail: response.errorMessages?.join(', ') || 'Araç tipi güncellenemedi.'
          });
        }
      },
      error: (err) => {
        this.isSaving.set(false);
        const errorMessage = err.error?.errorMessages?.[0] ||
          err.error?.message ||
          err.message ||
          'Araç tipi güncellenirken bir hata oluştu.';
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


  // ==================== HELPERS ====================
  setDefaultDisplayOrder(): void {
    const params: any = {
      pageNumber: 1,
      pageSize: 100
    };
    this.vehicleTypeService.getAllType(params).subscribe({
      next: (response) => {
        if (response.isSuccessful && response.data) {
          const maxOrder = response.data.items.reduce((max, item) => {
            return item.displayOrder > max ? item.displayOrder : max;
          }, 0);

          this.formData.displayOrder = maxOrder + 1;
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
