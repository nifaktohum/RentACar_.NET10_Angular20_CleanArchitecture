import { ChangeDetectionStrategy, Component, computed, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router } from '@angular/router';
import { MessageService } from 'primeng/api';
import { finalize } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { BreadCrumbModel } from '../../../core/models/breadcrumb';
import { VehicleImage } from '../../../core/models/vehicle/vehicle-image.model';
import { Vehicle } from '../../../core/models/vehicle/vehicle.model';
import { BreadcrumbService } from '../../../core/services/breadcrumb.service';
import { VehicleImageService } from '../../../core/services/vehicle-image.service';
import { VehicleService } from '../../../core/services/vehicle.service';
import { CustomConfirmDialogService } from '../../../shared/services/custom-confirm-dialog.service';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { DividerModule } from 'primeng/divider';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { CardModule } from 'primeng/card';

@Component({
  selector: 'app-vehicle-detail',
  imports: [
    DatePipe,
    CurrencyPipe,
    DividerModule,
    ButtonModule,
    TagModule,
    CardModule
  ],
  templateUrl: './vehicle-detail.component.html',
  styleUrl: './vehicle-detail.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class VehicleDetailComponent implements OnInit {
  // ==================== INJECTS ====================
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);

  private vehicleService = inject(VehicleService);
  private vehicleImageService = inject(VehicleImageService);
  private messageService = inject(MessageService);
  private breadcrumbService = inject(BreadcrumbService);
  private customConfirmDialogService = inject(CustomConfirmDialogService);

  // ==================== SIGNALS ====================
  readonly vehicle = signal<Vehicle | null>(null);
  readonly vehicleImages = signal<VehicleImage[]>([]);
  readonly isLoading = signal<boolean>(false);
  readonly vehicleId = signal<string | null>(null);
  readonly activeImageIndex = signal<number>(0);

  // ⭐ Galleria için
  readonly galleriaImages = computed(() => {
    return this.vehicleImages().map(img => ({
      itemImageSrc: this.getImageUrl(img.imageUrl),
      thumbnailImageSrc: this.getImageUrl(img.imageUrl),
      alt: 'Araç resmi',
      title: img.isMain ? 'Vitrin Resmi' : 'Araç Resmi'
    }));
  });

  readonly mainImage = computed(() => {
    const images = this.vehicleImages();
        
    if (images.length === 0) return null;
    const main = images.find(img => img.isMain);
    return main ? this.getImageUrl(main.imageUrl) : this.getImageUrl(images[0].imageUrl);
  });

  // ==================== BREADCRUMB ====================
  readonly breadcrumbs = signal<BreadCrumbModel[]>([
    { title: 'Araç Listesi', url: '/admin/vehicles', icon: 'ri-car-line', isActive: false },
    { title: 'Araç Detay', url: '/admin/vehicles/detail-vehicle', icon: 'ri-eye-line', isActive: true }
  ]);

  // ==================== LIFECYCLE ====================
  ngOnInit(): void {
    this.breadcrumbService.reset(this.breadcrumbs());

    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.messageService.add({
        severity: 'error',
        summary: 'Hata',
        detail: 'Araç ID bulunamadı.'
      });
      this.router.navigate(['/admin/vehicles']);
      return;
    }

    this.vehicleId.set(id);
    this.loadVehicle(id);
    this.loadVehicleImages(id);
  }

  // ==================== LOAD VEHICLE ====================
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
            this.updateBreadcrumb(res.data);
          }
        },
        error: (err) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Hata',
            detail: 'Araç bilgileri yüklenirken hata oluştu.'
          });
          console.error(err);
        }
      });
  }

  // ==================== LOAD IMAGES ====================
  private loadVehicleImages(id: string): void {
    this.vehicleImageService.getImagesByVehicle(id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (res) => {
          if (res.isSuccessful && res.data) {
            this.vehicleImages.set(res.data);
          }
        },
        error: (err) => {
          console.error('Resimler yüklenemedi:', err);
        }
      });
  }

  private updateBreadcrumb(vehicle: Vehicle): void {
    const breadcrumbs = this.breadcrumbs();
    breadcrumbs[breadcrumbs.length - 1].title = `${vehicle.brand} ${vehicle.model}`;
    this.breadcrumbService.update(breadcrumbs);
  }

  // ==================== ACTIONS ====================
  editVehicle(): void {
    const id = this.vehicleId();
    if (id) {
      this.router.navigate(['/admin/vehicles/edit-vehicle', id]);
    }
  }

  goBack(): void {
    this.router.navigate(['/admin/vehicles']);
  }

  setMainImage(image: VehicleImage): void {
    const vehicleId = this.vehicleId();
    if (!vehicleId) return;

    if (image.isMain) {
      this.messageService.add({
        severity: 'info',
        summary: 'Bilgi',
        detail: 'Bu resim zaten vitrin resmi.'
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
                this.vehicleImages.update(images =>
                  images.map(img => ({
                    ...img,
                    isMain: img.id === image.id
                  }))
                );
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

  toggleStatus(): void {
    const vehicle = this.vehicle();
    if (!vehicle) return;

    const newStatus = !vehicle.isActive;

    this.customConfirmDialogService.showStatusChangeConfirm(
      `${vehicle.brand} ${vehicle.model}`,
      newStatus,
      () => {
        this.vehicleService.toggleStatus(vehicle.id)
          .pipe(takeUntilDestroyed(this.destroyRef))
          .subscribe({
            next: (response) => {
              if (response.isSuccessful) {
                this.vehicle.update(v => v ? { ...v, isActive: newStatus } : v);
                this.messageService.add({
                  severity: 'success',
                  summary: 'Başarılı',
                  detail: `Araç ${newStatus ? 'aktifleştirildi' : 'pasifleştirildi'}.`
                });
              }
            },
            error: (err) => {
              this.messageService.add({
                severity: 'error',
                summary: 'Hata',
                detail: err?.error?.errorMessages?.[0] || 'Durum değiştirilemedi.'
              });
            }
          });
      }
    );
  }

  deleteVehicle(): void {
    const vehicle = this.vehicle();
    if (!vehicle) return;

    this.customConfirmDialogService.showDeleteConfirm(
      `${vehicle.brand} ${vehicle.model}`,
      () => {
        this.vehicleService.delete(vehicle.id)
          .pipe(takeUntilDestroyed(this.destroyRef))
          .subscribe({
            next: (response) => {
              if (response.isSuccessful) {
                this.messageService.add({
                  severity: 'success',
                  summary: 'Başarılı',
                  detail: 'Araç silindi.'
                });
                this.router.navigate(['/admin/vehicles']);
              }
            },
            error: (err) => {
              this.messageService.add({
                severity: 'error',
                summary: 'Hata',
                detail: err?.error?.errorMessages?.[0] || 'Araç silinemedi.'
              });
            }
          });
      }
    );
  }

  // ==================== HELPERS ====================
  getImageUrl(imageUrl: string | null | undefined): string {
    if (!imageUrl) return 'assets/no-image.png';
    if (imageUrl.startsWith('http://') || imageUrl.startsWith('https://')) return imageUrl;

    const baseUrl = environment.apiUrl.replace(/\/api\/?$/, '');
    const path = imageUrl.startsWith('/') ? imageUrl : `/${imageUrl}`;

    return `${baseUrl}${path}`;
  }

  getStatusSeverity(isActive: boolean): 'success' | 'danger' {
    return isActive ? 'success' : 'danger';
  }

  getStatusLabel(isActive: boolean): string {
    return isActive ? 'Aktif' : 'Pasif';
  }

  getAvailabilitySeverity(isAvailable: boolean): 'success' | 'danger' {
    return isAvailable ? 'success' : 'danger';
  }

  getAvailabilityLabel(isAvailable: boolean): string {
    return isAvailable ? 'Müsait' : 'Dolu';
  }
}
