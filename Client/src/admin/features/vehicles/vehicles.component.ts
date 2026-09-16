import { ChangeDetectionStrategy, Component, inject, OnInit, signal, ViewEncapsulation } from '@angular/core';
import { Vehicle } from '../../core/models/vehicle/vehicle.model';
import { VehicleType } from '../../core/models/vehicle/vehicle-type.model';
import { VehicleService } from '../../core/services/vehicle.service';
import { MessageService } from 'primeng/api';
import { CustomConfirmDialogService } from '../../shared/services/custom-confirm-dialog.service';
import { BreadcrumbService } from '../../core/services/breadcrumb.service';
import { Router } from '@angular/router';
import { BreadCrumbModel } from '../../core/models/breadcrumb';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { FormsModule } from '@angular/forms';
import { SelectModule } from 'primeng/select';
import { TableModule, TablePageEvent } from 'primeng/table';
import { CurrencyPipe, NgClass } from '@angular/common';
import { TagModule } from 'primeng/tag';
import { VehicleImageService } from '../../core/services/vehicle-image.service';
import { environment } from '../../../environments/environment';
import { forkJoin } from 'rxjs';
import { VehicleTypeService } from '../../core/services/vehicle-type.service';
import { VehicleStateService } from '../../core/services/state-services/vehicle-state.service';

@Component({
  selector: 'app-vehicles',
  imports: [
    CardModule,
    ButtonModule,
    FormsModule,
    TableModule,
    TagModule,
    NgClass,
    CurrencyPipe,
    SelectModule
  ],
  templateUrl: './vehicles.component.html',
  styleUrl: './vehicles.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.Emulated
})
export class VehiclesComponent implements OnInit {
  public vehicleService = inject(VehicleService);
  private vehicleTypeService = inject(VehicleTypeService);
  private vehicleStateService = inject(VehicleStateService);
  private messageService = inject(MessageService);
  private customConfirmDialogService = inject(CustomConfirmDialogService);
  private breadcrumbService = inject(BreadcrumbService);
  private router = inject(Router);

  // ==================== SIGNALS ====================
  readonly vehicles = signal<Vehicle[]>([]);
  readonly totalCount = signal<number>(0);
  readonly isLoading = signal<boolean>(false);
  // readonly vehicleTypes = signal<VehicleType[]>([]);
  readonly vehicleTypes = signal<VehicleType[]>([]);

  // Filtreler
  readonly filterBrand = signal<string>('');
  readonly filterModel = signal<string>('');
  readonly filterSearch = this.vehicleStateService.filterSearch;
  readonly selectedBrand = this.vehicleStateService.selectedBrand;
  readonly selectedModel = this.vehicleStateService.selectedModel;
  readonly selectedType = this.vehicleStateService.selectedType;
  readonly filterIsAvailable = this.vehicleStateService.filterIsAvailable;
  readonly vehicleBrandsDistinct = signal<string[]>([]);
  readonly vehicleModelsDistinct = signal<string[]>([]);
  readonly vehicleTypesDistinct = signal<string[]>([]);



  // Sıralama
  readonly sortField = this.vehicleStateService.sortField;
  readonly sortOrder = this.vehicleStateService.sortOrder;

  // Sayfalama
  readonly pageIndex = this.vehicleStateService.pageIndex;
  readonly pageSize = this.vehicleStateService.pageSize;

  // Breadcrumb
  readonly breadcrumbs = signal<BreadCrumbModel[]>([
    {
      title: 'Araç Listesi',
      url: '/admin/vehicles',
      icon: 'ri-car-line',
      isActive: true
    }
  ]);

  // ==================== LIFECYCLE ====================
  ngOnInit(): void {
    this.breadcrumbService.reset(this.breadcrumbs());
    this.loadVehicleTypes();
    this.loadVehicles();
    // Birden fazla observable'ı birleştirir
    forkJoin({
      brands: this.vehicleService.getBrandsDistinct(),
      models: this.vehicleService.getModelsDistinct(),
      types: this.vehicleService.getTypesDistinct(),
    }).subscribe({
      next: ({ brands, models, types }) => {
        if (brands.isSuccessful && brands.data) this.vehicleBrandsDistinct.set(brands.data);
        if (models.isSuccessful && models.data) this.vehicleModelsDistinct.set(models.data);
        if (models.isSuccessful && types.data) this.vehicleTypesDistinct.set(types.data);
      }
    });

    
  }

  // ==================== LOAD METHODS ====================
  loadVehicleTypes(): void {
    this.vehicleTypeService.getAllType().subscribe({
      next: (res) => {
        if (res.isSuccessful && res.data) {
          this.vehicleTypes.set(res.data.items);
        }
      },
      error: () => {
        this.messageService.add({
          severity: 'error',
          summary: 'Hata',
          detail: 'Araç tipleri yüklenirken hata oluştu.'
        });
      }
    });
  }



  loadVehicles(): void {
    this.isLoading.set(true);


    const params = this.vehicleStateService.filterParams();
    console.log(params.sort);
    
    this.vehicleService.getAll(params).subscribe({
      next: (response) => {
        if (response.isSuccessful && response.data) {
          this.vehicles.set(response.data.items || []);
          // this.totalCount.payl?.set(response.data.totalCount || 0); // totalCount setleme
          this.totalCount.set(response.data.totalCount || 0);
        }
      },
      error: (err) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Hata',
          detail: 'Araç listesi yüklenirken hata oluştu.'
        });
        console.error('API Hatası:', err);
      },
      complete: () => {
        this.isLoading.set(false);
      }
    })
    
  }

  refreshData(): void {
    this.loadVehicles();
  }

  // ==================== FILTER METHODS ====================
  onFilterChange(): void {
    this.vehicleStateService.resetPage();
    this.loadVehicles();
  }

  onResetFilters(): void {
    this.vehicleStateService.resetFilters();
    this.loadVehicles();
  }

  // ==================== PAGINATION ====================
  onPageChange(event: TablePageEvent): void {
    
    const newPageSize = event.rows;
    const oldPageSize = this.pageSize();

    if (newPageSize !== oldPageSize) {
      this.pageSize.set(newPageSize);
      this.pageIndex.set(1);
    } else {
      this.pageIndex.set(event.first / event.rows + 1);
    }
    this.loadVehicles();
  }

  onSort(event: any): void {

    
    if (event.field) {
      this.sortField.set(event.field);
      this.sortOrder.set(event.order);
      this.vehicleStateService.resetPage();
      this.loadVehicles();
    }
  }

  // ==================== NAVIGATION ====================
  navigateToAdd(): void {
    this.router.navigate(['/admin/vehicles/create-vehicle']);
  }

  navigateToEdit(vehicle: Vehicle): void {
    this.router.navigate(['/admin/vehicles/edit-vehicle', vehicle.id]);
  }

  navigateToDetail(vehicle: Vehicle): void {
    this.router.navigate(['/admin/vehicles/detail-vehicle', vehicle.id]);
  }

  // ==================== ACTIONS ====================
  deleteVehicle(vehicle: Vehicle): void {
    this.customConfirmDialogService.showDeleteConfirm(
      `${vehicle.brand} ${vehicle.model}`,
      () => {
        this.vehicleService.delete(vehicle.id).subscribe({
          next: (response) => {
            if (response.isSuccessful) {
              this.messageService.add({
                severity: 'success',
                summary: 'Başarılı',
                detail: `"${vehicle.brand} ${vehicle.model}" aracı silindi.`,
                life: 3000
              });
              // // this.loadVehicles();
            }
          },
          error: (err) => {
            this.messageService.add({
              severity: 'error',
              summary: 'Hata',
              detail: err?.error?.message || 'Araç silinirken hata oluştu.',
              life: 3000
            });
            console.error(err);
          }
        });
      }
    );
  }


  toggleStatus(vehicle: Vehicle): void {
    const newStatus = !vehicle.isActive;

    this.customConfirmDialogService.showStatusChangeConfirm(
      `${vehicle.brand} ${vehicle.model}`,
      newStatus,
      () => {
        this.vehicleService.toggleStatus(vehicle.id).subscribe({
          next: (response) => {
            if (response.isSuccessful) {
              this.messageService.add({
                severity: 'success',
                summary: 'Başarılı',
                detail: `"${vehicle.brand} ${vehicle.model}" ${newStatus ? 'aktifleştirildi' : 'pasifleştirildi'}.`,
                life: 3000
              });
              this.loadVehicles();
            }
          },
          error: (err) => {
            this.messageService.add({
              severity: 'error',
              summary: 'Hata',
              detail: err?.error?.message || 'Durum değiştirilemedi.',
              life: 3000
            });
            console.error(err);
          }
        });
      }
    );
  }

  // ==================== HELPERS ====================
  getStatusSeverity(isActive: boolean): 'success' | 'danger' | 'warn' | 'info' | 'secondary' | 'contrast' {
    return isActive ? 'success' : 'danger';
  }

  getStatusLabel(isActive: boolean): string {
    return isActive ? 'Aktif' : 'Pasif';
  }

  getAvailabilitySeverity(isAvailable: boolean): 'success' | 'danger' | 'warn' | 'info' | 'secondary' | 'contrast' {
    return isAvailable ? 'success' : 'danger';
  }

  getAvailabilityLabel(isAvailable: boolean): string {
    return isAvailable ? 'Müsait' : 'Dolu';
  }

  getVehicleTypeName(typeId: string): string {
    const type = this.vehicleTypes().find(t => t.id === typeId);
    return type?.name || '-';
  }

  getMainImageUrl(vehicle: Vehicle): string | null {
    let imagePath: string | null = null;

    // 1. Önce vehicle.images dizisinden isMain: true olan resmi bul
    if (vehicle.images && vehicle.images.length > 0) {
      const mainImage = vehicle.images.find(img => img.isMain);
      if (mainImage) {
        imagePath = mainImage.imageUrl;
      }
    }

    // 2. Eğer images dizisinde yoksa vehicle.mainImageUrl değerine bak
    if (!imagePath && vehicle.imageUrl) {
      imagePath = vehicle.imageUrl;
    }

    if (!imagePath) return null;

    // 3. Eğer yol zaten 'http' ile başlıyorsa direkt döndür
    if (imagePath.startsWith('http')) {
      return imagePath;
    }

    // 4. environment.apiUrl içindeki '/api' ifadesini temizleyip anahost adresini alıyoruz
    // Örn: "https://localhost:7200/api" -> "https://localhost:7200"
    const baseUrl = environment.apiUrl.replace('/api', '');

    // Backend'in static dosyaları sunacağı tam URL'i oluşturuyoruz
    return `${baseUrl}${imagePath}`;
  }

}
