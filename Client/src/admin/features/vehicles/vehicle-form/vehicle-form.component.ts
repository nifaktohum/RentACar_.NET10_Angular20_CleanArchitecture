import { ChangeDetectionStrategy, Component, computed, DestroyRef, inject, OnInit, signal, ViewEncapsulation } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { VehicleService } from '../../../core/services/vehicle.service';
import { VehicleModelService } from '../../../core/services/vehicle-model.service';
import { VehicleImageService } from '../../../core/services/vehicle-image.service';
import { MessageService } from 'primeng/api';
import { BreadcrumbService } from '../../../core/services/breadcrumb.service';
import { CustomConfirmDialogService } from '../../../shared/services/custom-confirm-dialog.service';
import { VehicleModel } from '../../../core/models/vehicle/VehicleModel.model';
import { Vehicle } from '../../../core/models/vehicle/vehicle.model';
import { VehicleImage } from '../../../core/models/vehicle/vehicle-image.model';
import { BreadCrumbModel } from '../../../core/models/breadcrumb';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs';
import { CreateVehicleRequest } from '../../../core/models/vehicle/create-vehicle-request';
import { environment } from '../../../../environments/environment';
import { CardModule } from 'primeng/card';
import { SelectModule } from 'primeng/select';
import { TagModule } from 'primeng/tag';
import { InputNumber } from 'primeng/inputnumber';
import { Textarea } from 'primeng/textarea';
import { FileUploadModule } from 'primeng/fileupload';
import { ButtonModule } from 'primeng/button';
import { InputText } from 'primeng/inputtext';
import { UpdateVehicleRequest } from '../../../core/models/vehicle/UpdateVehicleRequest';

@Component({
  selector: 'app-vehicle-form',
  imports: [
    CardModule,
    FormsModule,
    ReactiveFormsModule,
    SelectModule,
    TagModule,
    InputNumber,
    Textarea,
    FileUploadModule,
    ButtonModule,
    InputText,

  ],
  templateUrl: './vehicle-form.component.html',
  styleUrl: './vehicle-form.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.Emulated
})

export class VehicleFormComponent implements OnInit {
  // ==================== INJECTS ====================
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private destroyRef = inject(DestroyRef);

  private vehicleService = inject(VehicleService);
  private vehicleModelService = inject(VehicleModelService);
  private vehicleImageService = inject(VehicleImageService);
  private messageService = inject(MessageService);
  private breadcrumbService = inject(BreadcrumbService);
  private customConfirmDialogService = inject(CustomConfirmDialogService);

  // ==================== SIGNALS ====================
  readonly vehicleModels = signal<VehicleModel[]>([]);
  readonly vehicle = signal<Vehicle | null>(null);
  readonly selectedModel = signal<VehicleModel | null>(null);

  readonly isEditMode = signal(false);
  readonly isLoading = signal(false);
  readonly isSubmitting = signal(false);
  readonly vehicleId = signal<string | null>(null);

  // Image signals
  readonly mainImageFile = signal<File | null>(null);
  readonly mainImagePreview = signal<string | null>(null);
  readonly additionalFiles = signal<File[]>([]);
  readonly existingImages = signal<VehicleImage[]>([]);
  readonly imagesToDelete = signal<string[]>([]);

  // Distinct options
  readonly colors = signal<string[]>([]);
  readonly fuelTypes = signal<string[]>([]);
  readonly transmissionTypes = signal<string[]>([]);

  // ==================== COMPUTED ====================
  readonly totalImageCount = computed(() => {
    return this.existingImages().length +
      (this.mainImageFile() ? 1 : 0) +
      this.additionalFiles().length;
  });

  readonly canAddMoreImages = computed(() => this.totalImageCount() < 11);

  readonly vehicleModelDisplay = computed(() => {
    const model = this.selectedModel();
    if (!model) return '';
    return `${model.brand} ${model.name} (${model.vehicleTypeName})`;
  });

  // ==================== FORM ====================
  vehicleForm!: FormGroup;

  // ==================== BREADCRUMB ====================
  readonly breadcrumbs = signal<BreadCrumbModel[]>([
    { title: 'Araç Listesi', url: '/admin/vehicles', icon: 'ri-car-line', isActive: false },
    { title: 'Araç Ekle', url: '/admin/vehicles/create-vehicle', icon: 'ri-add-line', isActive: true }
  ]);

  // ==================== LIFECYCLE ====================
  ngOnInit(): void {
    this.breadcrumbService.reset(this.breadcrumbs());
    this.initForm();
    this.loadVehicleModels();
    this.loadDistinctOptions();
    this.checkEditMode();
  }

  // ==================== FORM INIT ====================
  private initForm(): void {
    this.vehicleForm = this.fb.group({
      vehicleModelId: ['', [Validators.required]],
      brand: [{ value: '', disabled: true }],
      model: [{ value: '', disabled: true }],
      year: ['', [Validators.required, Validators.pattern(/^\d{4}$/)]],
      plate: ['', [Validators.required, Validators.pattern(/^[A-Za-z0-9]{2,8}$/)]],
      color: ['', [Validators.required]],
      fuelType: ['', [Validators.required]],
      transmission: ['', [Validators.required]],
      seatCount: [4, [Validators.required, Validators.min(1), Validators.max(20)]],
      doorCount: [4, [Validators.required, Validators.min(2), Validators.max(6)]],
      minAge: [18, [Validators.min(18), Validators.max(99)]],
      dailyPrice: [0, [Validators.required, Validators.min(0)]],
      description: ['']
    });
  }

  // ==================== LOAD DATA ====================
  private loadVehicleModels(): void {
    this.vehicleModelService.getAll({
      pageNumber: 1,
      pageSize: 1000,
      isInStock: true
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (res) => {
          if (res.isSuccessful && res.data) {
            this.vehicleModels.set(res.data.items || []);
          }
        },
        error: () => {
          this.messageService.add({
            severity: 'error',
            summary: 'Hata',
            detail: 'Araç modelleri yüklenirken hata oluştu.'
          });
        }
      });
  }

  private loadDistinctOptions(): void {
    this.vehicleService.getColorDistinct()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (res) => {
          if (res.isSuccessful && res.data) this.colors.set(res.data);
        }
      });

    this.vehicleService.getFuelTypeDistinct()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (res) => {
          if (res.isSuccessful && res.data) this.fuelTypes.set(res.data);
        }
      });

    this.vehicleService.getTransmissionDistinct()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (res) => {
          if (res.isSuccessful && res.data) this.transmissionTypes.set(res.data);
        }
      });
  }

  // ==================== MODEL SELECTION ====================
  onVehicleModelChange(modelId: string): void {
    const model = this.vehicleModels().find(m => m.id === modelId);

    if (model) {
      this.selectedModel.set(model);
      this.vehicleForm.patchValue({
        brand: model.brand,
        model: model.name
      });
    } else {
      this.selectedModel.set(null);
      this.vehicleForm.patchValue({ brand: '', model: '' });
    }
  }

  // ==================== EDIT MODE ====================
  private checkEditMode(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEditMode.set(true);
      this.vehicleId.set(id);
      this.loadVehicle(id);
      this.updateBreadcrumb('Düzenle');
    }
  }

  private loadVehicle(id: string): void {
    this.isLoading.set(true);
    this.vehicleService.getById(id)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.isLoading.set(false))
      )
      .subscribe({
        next: (res) => {
          if (res.isSuccessful && res.data) {
            this.vehicle.set(res.data);
            this.patchForm(res.data);
            this.loadExistingImages(id);
          }
        },
        error: () => {
          this.messageService.add({
            severity: 'error',
            summary: 'Hata',
            detail: 'Araç bilgileri yüklenirken hata oluştu.'
          });
        }
      });
  }

  private patchForm(vehicle: Vehicle): void {
    this.vehicleForm.patchValue({
      vehicleModelId: vehicle.vehicleModelId,
      brand: vehicle.brand || '',
      model: vehicle.model || '',
      year: vehicle.year,
      plate: vehicle.plate,
      color: vehicle.color,
      fuelType: vehicle.fuelType,
      transmission: vehicle.transmission,
      seatCount: vehicle.seatCount,
      doorCount: vehicle.doorCount,
      minAge: vehicle.minAge,
      dailyPrice: vehicle.dailyPrice,
      description: vehicle.description
    });

    this.onVehicleModelChange(vehicle.vehicleModelId);
  }

  private loadExistingImages(vehicleId: string): void {
    this.vehicleImageService.getImagesByVehicle(vehicleId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (res) => {
          if (res.isSuccessful && res.data) {
            this.existingImages.set(res.data);
            const mainImage = res.data.find(img => img.isMain);
            if (mainImage) {
              this.mainImagePreview.set(this.getImageUrl(mainImage.imageUrl));
            }
          }
        }
      });
  }

  private updateBreadcrumb(title: string): void {
    const breadcrumbs = this.breadcrumbs();
    breadcrumbs[breadcrumbs.length - 1].title = `Araç ${title}`;
    this.breadcrumbService.update(breadcrumbs);
  }

  // ==================== IMAGE HANDLING ====================
  onMainImageSelected(event: any): void {
    const files = event.files;
    if (!files || files.length === 0) return;

    const file = files[0];

    if (file.size > 5 * 1024 * 1024) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Uyarı',
        detail: 'Resim 5MB\'dan büyük olamaz.'
      });
      return;
    }

    this.mainImageFile.set(file);
    this.mainImagePreview.set(URL.createObjectURL(file));
  }

  removeMainImage(): void {
    this.mainImageFile.set(null);
    this.mainImagePreview.set(null);
  }

  onAdditionalImagesSelected(event: any): void {
    const files = event.files;
    if (!files || files.length === 0) return;

    const fileList = Array.from(files) as File[];

    if (this.totalImageCount() + fileList.length > 11) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Uyarı',
        detail: 'En fazla 11 resim yükleyebilirsiniz.'
      });
      return;
    }

    this.additionalFiles.update(current => [...current, ...fileList]);
  }

  removeAdditionalFile(index: number): void {
    this.additionalFiles.update(files => files.filter((_, i) => i !== index));
  }

  removeExistingImage(image: VehicleImage, event: Event): void {
    event.stopPropagation();

    if (image.isMain) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Uyarı',
        detail: 'Vitrin resmini silemezsiniz. Önce başka bir resmi vitrin yapın.'
      });
      return;
    }

    this.customConfirmDialogService.showDeleteConfirm(
      'Bu resmi silmek istediğinize emin misiniz?',
      () => {
        this.imagesToDelete.update(ids => [...ids, image.id]);
        this.existingImages.update(images => images.filter(img => img.id !== image.id));
        this.messageService.add({
          severity: 'success',
          summary: 'Başarılı',
          detail: 'Resim silinecekler listesine eklendi.'
        });
      }
    );
  }

  onImageClick(image: VehicleImage): void {
    if (image.isMain) return;

    const vehicleId = this.vehicleId();
    if (!vehicleId) {
      this.messageService.add({
        severity: 'error',
        summary: 'Hata',
        detail: 'Araç ID bulunamadı.'
      });
      return;
    }

    this.customConfirmDialogService.showConfirm(
      'Vitrin Resmini Değiştir',
      'Bu resmi vitrin resmi yapmak istediğinize emin misiniz?',
      () => {
        this.vehicleImageService.setMainImage(vehicleId, image.id)
          .pipe(takeUntilDestroyed(this.destroyRef))
          .subscribe({
            next: (response) => {
              if (response.isSuccessful) {
                this.existingImages.update(images =>
                  images.map(img => ({
                    ...img,
                    isMain: img.id === image.id
                  }))
                );
                this.mainImagePreview.set(this.getImageUrl(image.imageUrl));
                this.messageService.add({
                  severity: 'success',
                  summary: 'Başarılı',
                  detail: 'Vitrin resmi güncellendi.'
                });
              }
            },
            error: (err) => {
              this.messageService.add({
                severity: 'error',
                summary: 'Hata',
                detail: err?.error?.errorMessages?.[0] || 'Vitrin resmi güncellenemedi.'
              });
            }
          });
      }
    );
  }

  // ==================== SUBMIT ====================
  async onSubmit(): Promise<void> {
    if (this.vehicleForm.invalid) {
      this.markAllFieldsAsTouched();
      this.messageService.add({
        severity: 'warn',
        summary: 'Uyarı',
        detail: 'Lütfen tüm zorunlu alanları doldurun.'
      });
      return;
    }

    this.isSubmitting.set(true);

    if (this.isEditMode()) {
      this.updateVehicle();
    } else {
      this.createVehicle();
    }
  }

  // ==================== CREATE ====================
  private createVehicle(): void {
    const request: CreateVehicleRequest = {
      vehicleModelId: this.vehicleForm.getRawValue().vehicleModelId,
      brand: this.vehicleForm.getRawValue().brand,
      model: this.vehicleForm.getRawValue().model,
      year: this.vehicleForm.value.year,
      plate: this.vehicleForm.value.plate,
      color: this.vehicleForm.value.color,
      fuelType: this.vehicleForm.value.fuelType,
      transmission: this.vehicleForm.value.transmission,
      seatCount: this.vehicleForm.value.seatCount,
      doorCount: this.vehicleForm.value.doorCount,
      minAge: this.vehicleForm.value.minAge || null,
      dailyPrice: this.vehicleForm.value.dailyPrice,
      description: this.vehicleForm.value.description || null,
      imageFile: this.mainImageFile()  // ⭐ TEK resim
    };

    this.vehicleService.create(request)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.isSubmitting.set(false))
      )
      .subscribe({
        next: (res) => {
          if (res.isSuccessful && res.data) {
            const newVehicleId = res.data.id;

            // ⭐ Ek resimler varsa yükle
            if (this.additionalFiles().length > 0) {
              this.uploadAdditionalImages(newVehicleId);
            } else {
              this.onSuccess('Araç başarıyla oluşturuldu.');
            }
          }
        },
        error: (err) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Hata',
            detail: err?.error?.errorMessages?.[0] || 'Araç oluşturulamadı.'
          });
        }
      });
  }

  // ==================== UPDATE ====================
  private updateVehicle(): void {
    const id = this.vehicleId();
    if (!id) return;

    const request: UpdateVehicleRequest = {
      id: id,
      vehicleModelId: this.vehicleForm.getRawValue().vehicleModelId,
      brand: this.vehicleForm.getRawValue().brand,
      model: this.vehicleForm.getRawValue().model,
      year: this.vehicleForm.value.year,
      plate: this.vehicleForm.value.plate,
      color: this.vehicleForm.value.color,
      fuelType: this.vehicleForm.value.fuelType,
      transmission: this.vehicleForm.value.transmission,
      seatCount: this.vehicleForm.value.seatCount,
      doorCount: this.vehicleForm.value.doorCount,
      minAge: this.vehicleForm.value.minAge || null,
      dailyPrice: this.vehicleForm.value.dailyPrice,
      description: this.vehicleForm.value.description || null,
      isActive: this.vehicle()?.isActive ?? true,
    };

    this.vehicleService.update(request)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.isSubmitting.set(false))
      )
      .subscribe({
        next: (res) => {
          if (res.isSuccessful) {
            // ⭐ Yeni resimler varsa yükle
            if (this.additionalFiles().length > 0) {
              this.uploadAdditionalImages(id);
            } else {
              this.handlePostUpdate(id);
            }
          }
        },
        error: (err) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Hata',
            detail: err?.error?.errorMessages?.[0] || 'Araç güncellenemedi.'
          });
        }
      });
  }

  // ==================== EK RESİM YÜKLE ====================
  private uploadAdditionalImages(vehicleId: string): void {
    const files = this.additionalFiles();
    if (files.length === 0) {
      this.handlePostUpdate(vehicleId);
      return;
    }

    this.vehicleImageService.uploadImages(vehicleId, files, false)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => this.handlePostUpdate(vehicleId),
        error: () => {
          this.messageService.add({
            severity: 'warn',
            summary: 'Uyarı',
            detail: 'Araç kaydedildi ancak ek resimler yüklenemedi.'
          });
          this.handlePostUpdate(vehicleId);
        }
      });
  }

  // ==================== SİLİNECEK RESİMLERİ SİL ====================
  private handlePostUpdate(vehicleId: string): void {
    const toDelete = this.imagesToDelete();
    if (toDelete.length > 0) {
      this.deleteImages(toDelete, () => {
        this.onSuccess(this.isEditMode() ? 'Araç güncellendi.' : 'Araç oluşturuldu.');
      });
    } else {
      this.onSuccess(this.isEditMode() ? 'Araç güncellendi.' : 'Araç oluşturuldu.');
    }
  }

  private deleteImages(imageIds: string[], onComplete: () => void): void {
    let completed = 0;
    const total = imageIds.length;

    imageIds.forEach(id => {
      this.vehicleImageService.deleteImage(id)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: () => { if (++completed === total) onComplete(); },
          error: () => { if (++completed === total) onComplete(); }
        });
    });
  }

  // ==================== SUCCESS ====================
  private onSuccess(message: string): void {
    this.messageService.add({
      severity: 'success',
      summary: 'Başarılı',
      detail: message
    });
    this.router.navigate(['/admin/vehicles']);
  }

  // ==================== HELPERS ====================
  private markAllFieldsAsTouched(): void {
    Object.keys(this.vehicleForm.controls).forEach(key => {
      this.vehicleForm.get(key)?.markAsTouched();
    });
  }

  isFieldInvalid(fieldName: string): boolean {
    const control = this.vehicleForm.get(fieldName);
    return !!(control && control.invalid && (control.dirty || control.touched));
  }

  getFieldError(fieldName: string): string {
    const control = this.vehicleForm.get(fieldName);
    if (!control) return '';

    if (control.hasError('required')) return 'Bu alan zorunludur.';
    if (control.hasError('pattern')) {
      if (fieldName === 'year') return 'Geçerli bir yıl giriniz (örn: 2024).';
      if (fieldName === 'plate') return 'Geçerli bir plaka giriniz (örn: 34ABC123).';
    }
    if (control.hasError('min')) {
      if (fieldName === 'dailyPrice') return 'Fiyat 0\'dan büyük olmalıdır.';
    }
    return 'Geçersiz değer.';
  }

  getImageUrl(imageUrl: string | null | undefined): string | null {
    if (!imageUrl || imageUrl.trim() === '') return null;
    if (imageUrl.startsWith('http://') || imageUrl.startsWith('https://')) return imageUrl;

    const baseUrl = environment.apiUrl.replace(/\/api\/?$/, '');
    const path = imageUrl.startsWith('/') ? imageUrl : `/${imageUrl}`;

    return `${baseUrl}${path}`;
  }

  getFileUrl(file: File): string {
    return URL.createObjectURL(file);
  }

  goBack(): void {
    const hasChanges = this.mainImageFile() ||
      this.additionalFiles().length > 0 ||
      this.vehicleForm.dirty ||
      this.imagesToDelete().length > 0;

    if (hasChanges) {
      this.customConfirmDialogService.showConfirm(
        'Değişiklikler Kaydedilmedi',
        'Bu sayfadan ayrılmak istediğinize emin misiniz?',
        () => this.router.navigate(['/admin/vehicles'])
      );
    } else {
      this.router.navigate(['/admin/vehicles']);
    }
  }
}
