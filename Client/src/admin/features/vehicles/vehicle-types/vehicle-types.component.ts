import { ChangeDetectionStrategy, Component, inject, signal, ViewEncapsulation } from '@angular/core';
import { MessageService } from 'primeng/api';
import { BreadcrumbService } from '../../../core/services/breadcrumb.service';
import { VehicleTypeService } from '../../../core/services/vehicle-type.service';
import { CustomConfirmDialogService } from '../../../shared/services/custom-confirm-dialog.service';
import { VehicleType } from '../../../core/models/vehicle/vehicle-type.model';
import { BreadCrumbModel } from '../../../core/models/breadcrumb';
import { TagModule } from 'primeng/tag';
import { NgClass } from '@angular/common';
import { TableModule } from 'primeng/table';
import { FormsModule } from '@angular/forms';
import { SelectModule } from 'primeng/select';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { VehicleTypeDialogComponent } from './vehicle-type-dialog/vehicle-type-dialog.component';

@Component({
  selector: 'app-vehicle-types',
  imports: [
    VehicleTypeDialogComponent,
    TagModule,
    NgClass,
    FormsModule,
    SelectModule,
    ButtonModule,
    TableModule,
    CardModule,
  ],
  templateUrl: './vehicle-types.component.html',
  styleUrl: './vehicle-types.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.Emulated
})
export class VehicleTypesComponent {
  private vehicleTypeService = inject(VehicleTypeService);
  private messageService = inject(MessageService);
  private customConfirmDialogService = inject(CustomConfirmDialogService);
  private breadcrumbService = inject(BreadcrumbService);

  // ==================== SIGNALS ====================
  readonly vehicleTypes = signal<VehicleType[]>([]);
  readonly totalCount = signal<number>(0);
  readonly isLoading = signal<boolean>(false);

  // Dialog state
  readonly dialogVisible = signal<boolean>(false);
  readonly dialogMode = signal<'create' | 'edit'>('create');
  readonly selectedVehicleTypeId = signal<string | null>(null);

  // Filtreler
  readonly filterSearch = signal<string>('');
  readonly filterStatus = signal<boolean | null>(null);

  // Sayfalama
  readonly pageIndex = signal<number>(1);
  readonly pageSize = signal<number>(5);

  // Sıralama
  readonly sortField = signal<string>('displayOrder');
  readonly sortOrder = signal<number>(-1);

  readonly statusOptions = [
    { label: 'Aktif', value: true },
    { label: 'Pasif', value: false }
  ];

  // Breadcrumb
  readonly breadcrumbs = signal<BreadCrumbModel[]>([
    {
      title: 'Araç Tipleri Listesi',
      url: '/admin/vehicle-types',
      icon: 'ri-caravan-line',
      isActive: true
    }
  ]);

  // ==================================================

  ngOnInit(): void {
    this.breadcrumbService.reset(this.breadcrumbs());
    this.loadVehicleTypes();
    console.log(this.selectedVehicleTypeId());
    
  }

  loadVehicleTypes(): void {
    this.isLoading.set(true);
    const direction = this.sortOrder() === 1 ? 'Asc' : 'Desc';
    const sortParam = this.sortField() ? `${this.sortField()}${direction}` : null;

    const params: any = {
      pageNumber: this.pageIndex(),
      pageSize: this.pageSize(),
      sort: sortParam
    };
  
    this.vehicleTypeService.getAllType(params).subscribe({
      next: (res) => {
        if (res.isSuccessful && res.data) {
          this.vehicleTypes.set(res.data.items);
          this.totalCount.set(res.data.totalCount);          
        }
        this.isLoading.set(false);
      },
      error: (err) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Hata',
          detail: 'Araç tipleri yüklenirken bir hata oluştu.'
        });
        console.error('Hata:', err?.error?.message || err?.message);
        this.isLoading.set(false);
      }
    });
  }

  refreshData(): void {
    this.loadVehicleTypes();
  }

  onFilterChange(): void {
    this.pageIndex.set(1);
    this.loadVehicleTypes();
  }

  onPageChange(event: any): void {
    this.pageIndex.set(event.first / event.rows + 1);
    this.pageSize.set(event.rows);
    this.loadVehicleTypes();
  }

  getVehicleTypes(): VehicleType[] {
    let items = this.vehicleTypes();

    // Durum filtresi
    if (this.filterStatus() !== null) {
      items = items.filter(vt => vt.isActive === this.filterStatus());
    }

    // Arama filtresi
    const search = this.filterSearch().toLowerCase().trim();
    if (search) {
      items = items.filter(vt =>
        vt.name.toLowerCase().includes(search) ||
        (vt.description && vt.description.toLowerCase().includes(search))
      );
    }

    return items;
  }

  // DIALOG METHODS
  openCreateDialog(): void {
    this.dialogMode.set('create');
    this.selectedVehicleTypeId.set(null);
    this.dialogVisible.set(true);
  }

  openEditDialog(vehicleType: VehicleType): void {
    this.dialogMode.set('edit');
    this.selectedVehicleTypeId.set(vehicleType.id);
    this.dialogVisible.set(true);
  }

  onDialogSaved(): void {
    this.loadVehicleTypes();
  }

  deleteVehicleType(id: string, name: string): void {
    this.customConfirmDialogService.showDeleteConfirm(
      name,
      () => {
        this.vehicleTypeService.delete(id).subscribe({
          next: (response) => {
            if (response.isSuccessful) {
              this.messageService.add({
                severity: 'success',
                summary: 'Başarılı',
                detail: `"${name}" araç tipi silindi.`,
                life: 3000
              });
              this.loadVehicleTypes();
            }
          },
          error: (err) => {
            this.messageService.add({
              severity: 'error',
              summary: 'Hata',
              detail: err?.error?.errorMessages?.[0] || 'Araç tipi silinirken bir hata oluştu.',
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

  toggleStatus(vehicleType: VehicleType): void {
    const newStatus = !vehicleType.isActive;

    this.customConfirmDialogService.showStatusChangeConfirm(
      vehicleType.name,
      newStatus,
      () => {
        this.vehicleTypeService.toggleStatus(vehicleType.id).subscribe({
          next: (response) => {
            if (response.isSuccessful) {
              this.messageService.add({
                severity: 'success',
                summary: 'Başarılı',
                detail: `"${vehicleType.name}" ${newStatus ? 'aktifleştirildi' : 'pasifleştirildi'}.`,
                life: 3000
              });
              this.loadVehicleTypes();
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
          detail: `Durum değişikliği iptal edildi: ${vehicleType.name}`,
          life: 3000
        });
      }
    );
  }

  onSort(event: any): void {
    if (!event.field) return;
console.log(event);

    this.sortField.set(event.field);
    this.sortOrder.set(event.order);
    this.pageIndex.set(1);
    this.loadVehicleTypes();  // ⭐ Backend'e istek at
  }
}
