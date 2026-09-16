import { ChangeDetectionStrategy, Component, inject, signal, ViewEncapsulation } from '@angular/core';
import { MessageService } from 'primeng/api';
import { BreadcrumbService } from '../../../core/services/breadcrumb.service';
import { VehicleModelService } from '../../../core/services/vehicle-model.service';
import { VehicleTypeService } from '../../../core/services/vehicle-type.service';
import { CustomConfirmDialogService } from '../../../shared/services/custom-confirm-dialog.service';
import { VehicleModel, VehicleModelFilterParams } from '../../../core/models/vehicle/VehicleModel.model';
import { VehicleType } from '../../../core/models/vehicle/vehicle-type.model';
import { BreadCrumbModel } from '../../../core/models/breadcrumb';
import { TagModule } from 'primeng/tag';
import { NgClass } from '@angular/common';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { CardModule } from 'primeng/card';
import { FormsModule } from '@angular/forms';
import { TooltipModule } from 'primeng/tooltip';
import { VehicleModelDialogComponent } from './vehicle-model-dialog/vehicle-model-dialog.component';

@Component({
  selector: 'app-vehicle-models',
  imports: [
    TagModule,
    NgClass,
    TableModule,
    ButtonModule,
    SelectModule,
    CardModule,
    FormsModule,
    TooltipModule,
    VehicleModelDialogComponent
],
  templateUrl: './vehicle-models.component.html',
  styleUrl: './vehicle-models.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.Emulated
})
export class VehicleModelsComponent {
  private vehicleModelService = inject(VehicleModelService);
  private vehicleTypeService = inject(VehicleTypeService);
  private messageService = inject(MessageService);
  private customConfirmDialogService = inject(CustomConfirmDialogService);
  private breadcrumbService = inject(BreadcrumbService);

  // ==================== SIGNALS ====================
  readonly vehicleModels = signal<VehicleModel[]>([]);
  readonly vehicleTypes = signal<VehicleType[]>([]);
  readonly totalCount = signal<number>(0);
  readonly isLoading = signal<boolean>(false);

  // Dialog state
  readonly dialogVisible = signal<boolean>(false);
  readonly dialogMode = signal<'create' | 'edit'>('create');
  readonly selectedVehicleModelId = signal<string | null>(null);

  // Filtreler
  readonly filterSearch = signal<string>('');
  readonly filterVehicleType = signal<string | null>(null);
  readonly filterStatus = signal<boolean | null>(null);
  readonly filterIsInStock = signal<boolean | null>(null);
  readonly filterMinStock = signal<number | null>(null);
  readonly filterMaxStock = signal<number | null>(null);

  // Sayfalama
  readonly pageIndex = signal<number>(1);
  readonly pageSize = signal<number>(5);

  // Sıralama
  readonly sortField = signal<string>('brand');
  readonly sortOrder = signal<number>(1);

  // Dropdown options
  readonly statusOptions = [
    { label: 'Aktif', value: true },
    { label: 'Pasif', value: false }
  ];

  readonly stockOptions = [
    { label: 'Stokta Var', value: true },
    { label: 'Stokta Yok', value: false }
  ];

  readonly selectedSort = signal<string>('');

  // Breadcrumb
  readonly breadcrumbs = signal<BreadCrumbModel[]>([
    {
      title: 'Araç Model Listesi',
      url: '/admin/vehicle-models',
      icon: 'ri-stack-line',
      isActive: true
    }
  ]);

  // ==================================================

  ngOnInit(): void {
    this.breadcrumbService.reset(this.breadcrumbs());
    this.loadVehicleTypes();
    this.loadVehicleModels();
  }

  loadVehicleTypes(): void {
    const params: any = {
      pageNumber: 1,
      pageSize: 100  
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

  loadVehicleModels(): void {
    this.isLoading.set(true);

    const direction = this.sortOrder() === 1 ? 'Asc' : 'Desc';
    const sortParam = this.sortField() ? `${this.sortField()}${direction}` : null;
    


    const params: VehicleModelFilterParams = {
      pageNumber: this.pageIndex(),
      pageSize: this.pageSize(),
      searchTerm: this.filterSearch() || undefined,
      vehicleTypeIds: this.filterVehicleType() ? [this.filterVehicleType()!] : undefined,
      isInStock: this.filterIsInStock(),
      minStock: this.filterMinStock(),
      maxStock: this.filterMaxStock(),
      sort: sortParam
    };

    this.vehicleModelService.getAll(params).subscribe({
      next: (res) => {
        if (res.isSuccessful && res.data) {
          this.vehicleModels.set(res.data.items);
          this.totalCount.set(res.data.totalCount);
        }
        this.isLoading.set(false);
      },
      error: (err) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Hata',
          detail: 'Araç modelleri yüklenirken bir hata oluştu.'
        });
        console.error('Hata:', err?.error?.message || err?.message);
        this.isLoading.set(false);
      }
    });
  }

  refreshData(): void {
    this.loadVehicleModels();
  }

  onFilterChange(): void {
    this.pageIndex.set(1);
    this.loadVehicleModels();
  }

  clearFilters(): void {
    this.filterSearch.set('');
    this.filterVehicleType.set(null);
    this.filterStatus.set(null);
    this.filterIsInStock.set(null);
    this.filterMinStock.set(null);
    this.filterMaxStock.set(null);
    this.selectedSort.set('brandAsc');
    this.pageIndex.set(1);
    this.loadVehicleModels();
  }

  onPageChange(event: any): void {
    this.pageIndex.set(event.first / event.rows + 1);
    this.pageSize.set(event.rows);
    this.loadVehicleModels();
  }


  
  onSort(event: any): void {    
    if (!event.field) return;
      this.sortField.set(event.field);
      this.sortOrder.set(event.order);
      this.pageIndex.set(1); 
      this.loadVehicleModels();

  }

  openCreateDialog(): void {
    this.dialogMode.set('create');
    this.selectedVehicleModelId.set(null);
    this.dialogVisible.set(true);
  }

  onDialogSaved(): void {
    this.loadVehicleModels();
  }

  openEditDialog(model: VehicleModel): void {
    this.dialogMode.set('edit');
    this.selectedVehicleModelId.set(model.id);
    this.dialogVisible.set(true);
  }

  deleteVehicleModel(id: string, name: string): void {
    this.customConfirmDialogService.showDeleteConfirm(
      name,
      () => {
        this.vehicleModelService.delete(id).subscribe({
          next: (response) => {
            if (response.isSuccessful) {
              this.messageService.add({
                severity: 'success',
                summary: 'Başarılı',
                detail: `"${name}" araç modeli silindi.`,
                life: 3000
              });
              this.loadVehicleModels();
            }
          },
          error: (err) => {
            this.messageService.add({
              severity: 'error',
              summary: 'Hata',
              detail: err?.error?.errorMessages?.[0] || 'Araç modeli silinirken bir hata oluştu.',
              life: 3000
            });
            console.error(err);
          }
        });
      },
      () => {
        this.messageService.add({
          severity: 'info',
          summary: 'İptal Edildi',
          detail: `Silme işlemi iptal edildi: ${name}`,
          life: 3000
        });
      }
    );
  }

  toggleStatus(model: VehicleModel): void {
    const newStatus = !model.isActive;

    this.customConfirmDialogService.showStatusChangeConfirm(
      model.name,
      newStatus,
      () => {
        this.vehicleModelService.toggleStatus(model.id).subscribe({
          next: (response) => {
            if (response.isSuccessful) {
              this.messageService.add({
                severity: 'success',
                summary: 'Başarılı',
                detail: `"${model.name}" ${newStatus ? 'aktifleştirildi' : 'pasifleştirildi'}.`,
                life: 3000
              });
              this.loadVehicleModels();
            }
          },
          error: (err) => {
            this.messageService.add({
              severity: 'error',
              summary: 'Hata',
              detail: err?.error?.errorMessages?.[0] || 'Durum değiştirilemedi.',
              life: 3000
            });
            console.error(err);
          }
        });
      },
      () => {
        this.messageService.add({
          severity: 'error',
          summary: 'Hata',
          detail: `Durum değişikliği iptal edildi: ${model.name}`,
          life: 3000
        });
      }
    );
  }


  getStockSeverity(stock: number): 'success' | 'warn' | 'danger' {
    if (stock === 0) return 'danger';
    if (stock <= 3) return 'warn';
    return 'success';
  }

}
