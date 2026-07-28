import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { ExtraService } from '../../core/services/extra.service';
import { MessageService } from 'primeng/api';
import { BreadcrumbService } from '../../core/services/breadcrumb.service';
import { ExtraSummary } from '../../core/models/extra/extraSummary';
import { CategoryMap, ExtraCategoryOptions, ExtraPriceTypeOptions, PriceTypeMap } from '../../core/models/extra/enum/extra-enums.model';
import { BreadCrumbModel } from '../../core/models/breadcrumb';
import { CustomConfirmDialogService } from '../../shared/services/custom-confirm-dialog.service';
import { ExtraDialogComponent } from './extra-dialog/extra-dialog.component';
import { CurrencyPipe, NgClass } from '@angular/common';
import { TagModule } from 'primeng/tag';
import { TableModule } from 'primeng/table';
import { SelectModule } from 'primeng/select';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';


@Component({
  selector: 'app-extras',
  imports: [
    //Component
    ExtraDialogComponent,
    //core
    NgClass,
    CurrencyPipe,
    FormsModule,
    //PrimeNG
    TagModule,
    TableModule,
    SelectModule,
    ButtonModule,
    CardModule
  ],
  templateUrl: './extras.component.html',
  styleUrl: './extras.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ExtrasComponent {
  private extraService = inject(ExtraService);
  private messageService = inject(MessageService);
    private customConfirmDialogService = inject(CustomConfirmDialogService);
  private breadcrumbService = inject(BreadcrumbService);

  // ==================== SIGNALS ====================
  readonly extras = signal<ExtraSummary[]>([]);
  readonly totalCount = signal<number>(0);
  readonly isLoading = signal<boolean>(false);

  // Dialog state
  readonly dialogVisible = signal<boolean>(false);
  readonly dialogMode = signal<'create' | 'edit'>('create');
  readonly selectedExtraId = signal<string | null>(null);

  // Filtreler
  readonly filterCategory = signal<number | null>(null);
  readonly filterPriceType = signal<number | null>(null);
  readonly filterSearch = signal<string>('');

  // Kategori seçenekleri
  readonly categoryOptions = ExtraCategoryOptions;
  readonly priceTypeOptions = ExtraPriceTypeOptions;

  // Sıralama 
  readonly sortField = signal<string>('displayOrder');
  readonly sortOrder = signal<number>(1);

  // Breadcrumb
  readonly breadcrumbs = signal<BreadCrumbModel[]>([
    {
      title: 'Ekstra Hizmetler',
      url: '/admin/extras',
      icon: 'ri-service-line',
      isActive: true
    }
  ]);

  // ==================================================

  ngOnInit(): void {
    this.breadcrumbService.reset(this.breadcrumbs());
    this.loadExtras();    
  }

  loadExtras(): void {
    this.isLoading.set(true);

    this.extraService.getAll().subscribe({
      next: (_res) => {
        if (_res.isSuccessful && _res.data) {
          this.extras.set(_res.data);
          this.totalCount.set(_res.data.length);
        }
        this.isLoading.set(false);
      },
      error: (err) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Hata',
          detail: 'Ekstra hizmetler yüklenirken bir hata oluştu.'
        });
        console.error('Hata:', err?.error?.message || err?.message);
        this.isLoading.set(false);
      }
    });
  }

  refreshData(): void {
    this.loadExtras();
  }

  onFilterChange(): void {
    this.loadExtras();
    // TODO: Filtreleri backend'e göndermek için API'ye filter parametresi eklenebilir
    // Şimdilik frontend'de filtreleme yapabiliriz
  }

  getFilteredExtras(): ExtraSummary[] {
    let items = this.extras();

    // Kategori filtresi
    if (this.filterCategory() !== null) {
      const category = CategoryMap[this.filterCategory()!];
      items = items.filter(e => e.category === category);
    }


    if (this.filterPriceType() !== null) {
      const priceType = PriceTypeMap[this.filterPriceType()!];
      items = items.filter(e => e.priceType === priceType);
    }

    // Arama filtresi
    const search = this.filterSearch().toLowerCase().trim();
    if (search) {
      items = items.filter(e =>
        e.name.toLowerCase().includes(search)
      );
    }

    return items;
  }

  // DIALOG METHODS
  openCreateDialog(): void {
    this.dialogMode.set('create');
    this.selectedExtraId.set(null);
    this.dialogVisible.set(true);
  }

  openEditDialog(extra: ExtraSummary): void {
    this.dialogMode.set('edit');
    this.selectedExtraId.set(extra.id);
    this.dialogVisible.set(true);
  }

  onDialogSaved(): void {
    this.loadExtras();
  }

  deleteExtra(id: string, name: string): void {
    this.customConfirmDialogService.showDeleteConfirm(
      name,
      () => {
        this.extraService.delete(id).subscribe({
          next: (response) => {
            if (response.isSuccessful) {
              this.messageService.add({
                severity: 'success',
                summary: 'Başarılı',
                detail: `"${name}" ekstra hizmeti silindi.`,
                life: 3000
              });
              this.loadExtras();
            }
          },
          error: (err) => {
            this.messageService.add({
              severity: 'error',
              summary: 'Hata',
              detail: err?.error?.message || 'Ekstra hizmet silinirken bir hata oluştu.',
              life: 3000
            });
            console.error(err);
          }
        })
      },
      () => {
        this.messageService.add({
          severity: 'info',
          summary: 'İptal Edildi',
          detail: `Silme işlemi iptal edildi: ${name}`,
          life: 3000
        });
      }
    )
  }

  toggleStatus(extra: ExtraSummary): void {
    const newStatus = !extra.isActive;

    this.customConfirmDialogService.showStatusChangeConfirm(
      extra.name,
      newStatus,
      () => {
        this.extraService.toggleStatus(extra.id).subscribe({
          next: (response) => {
            if (response.isSuccessful) {
              this.messageService.add({
                severity: 'success',
                summary: 'Başarılı',
                detail: `"${extra.name}" ${newStatus ? 'aktifleştirildi' : 'pasifleştirildi'}.`,
                life: 3000
              });
              this.loadExtras();
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
      },
      () => {
        this.messageService.add({
          severity: 'error',
          summary: 'Hata',
          detail: `Durum değişikliği iptal edildi: ${extra.name}`,
          life: 3000
        });
      }
    );
  }

  getCategoryLabel(category: string): string {
    const map: Record<string, string> = {
      'Guarantee': 'Güvence',
      'Driver': 'Sürücü',
      'Seat': 'Koltuk',
      'Other': 'Diğer'
    };
    return map[category] || category;
  }

  getCategorySeverity(category: string): 'success' | 'info' | 'warn' | 'danger' | 'secondary' | 'contrast' {
    const map: Record<string, any> = {
      'Guarantee': 'info',
      'Driver': 'warn',
      'Seat': 'success',
      'Other': 'secondary'
    };
    return map[category] || 'secondary';
  }

  getPriceTypeLabel(priceType: string): string {
    const map: Record<string, string> = {
      'Daily': 'Günlük',
      'Rental': 'Kiralama Başı'
    };
    return map[priceType] || priceType;
  }

  getStockDisplay(stockLimit: number | null): string {
    if (stockLimit === null) return 'Sınırsız';
    if (stockLimit === 0) return 'Stokta yok';
    return `${stockLimit}`;
  }

  onSort(event: any): void {
    this.sortField.set(event.field);
    this.sortOrder.set(event.order);
  }


}
