import { ChangeDetectionStrategy, Component, computed, DestroyRef, effect, EventEmitter, inject, Input, Output, signal, ViewEncapsulation } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { TextareaModule } from 'primeng/textarea';
import { CustomerManagementService } from '../../../core/services/customers/customer-management.service';
import { finalize } from 'rxjs';
import { Result } from '../../../../core/models/result.model';
import { BreadcrumbService } from '../../../core/services/breadcrumb.service';
import { CustomConfirmDialogService } from '../../../shared/services/custom-confirm-dialog.service';
import { Router } from '@angular/router';
import { CustomerListDto } from '../../../../core/models/customer.model';
import { BreadCrumbModel } from '../../../core/models/breadcrumb';
import { CustomerSpecParams } from '../../../core/models/customers/customer-spec-params';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { TableModule, TablePageEvent } from 'primeng/table';
import { CardModule } from 'primeng/card';
import { SelectModule } from 'primeng/select';
import { PhoneFormatPipe } from '../../../../shared/pipes/phone-format.pipe';
import { TagModule } from 'primeng/tag';
import { CustomerStateService } from '../../../core/services/customers/customer-state.service';
import { CustomerType } from '../../../../shared/enums/customer-type';

@Component({
  selector: 'app-black-list-form',
  imports: [
    CardModule,
    ButtonModule,
    FormsModule,
    SelectModule,
    TableModule,
    PhoneFormatPipe,
    TagModule,

  ],
  templateUrl: './blacklist.component.html',
  styleUrl: './blacklist.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.Emulated
})
export class BlacklistComponent {
  private customerService = inject(CustomerManagementService);
  readonly state = inject(CustomerStateService);
  private breadcrumbService = inject(BreadcrumbService);
  private messageService = inject(MessageService);
  private confirmDialog = inject(CustomConfirmDialogService);
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);

  // ==================== 🎛️ STATE SERVICE KISAYOLLARI ====================
  readonly filterType = this.state.selectedType;

  // ==================== SIGNALS ====================
  customers = signal<CustomerListDto[]>([]);
  totalCount = signal<number>(0);
  isLoading = signal<boolean>(false);

  // Filtreler (kara liste sayfasında sadece arama ve sıralama)
  filterSearch = signal<string>('');
  filterSort = signal<string>('createdDesc');

  // Sayfalama
  pageIndex = signal<number>(1);
  pageSize = signal<number>(10);

  // ==================== COMPUTED ====================
  hasActiveFilters = computed(() => !!this.filterSearch());


  sortOptions = [
    { label: 'En Yeni', field: 'created', order: -1 },
    { label: 'En Eski', field: 'created', order: 1 },
    { label: 'Email (A-Z)', field: 'email', order: 1 },
    { label: 'Email (Z-A)', field: 'email', order: -1 },
    { label: 'Ad (A-Z)', field: 'firstName', order: 1 },
    { label: 'Ad (Z-A)', field: 'firstName', order: -1 },
    { label: 'Şirket (A-Z)', field: 'companyName', order: 1 },
    { label: 'Şirket (Z-A)', field: 'companyName', order: -1 },
    { label: 'Findeks (Yüksek)', field: 'findeksScore', order: -1 },
    { label: 'Findeks (Düşük)', field: 'findeksScore', order: 1 }
  ];


  typeOptions = [
    { label: 'Tümü', value: null },
    { label: 'Bireysel', value: CustomerType.Individual },  // ⭐ enum
    { label: 'Kurumsal', value: CustomerType.Corporate }
  ];



  // ⭐ Hangi sort seçili?
  selectedSortOption = computed(() => {
    const field = this.state.sortField();
    const order = this.state.sortOrder();

    return this.sortOptions.find(o => o.field === field && o.order === order)
      ?? this.sortOptions[0];
  });


  // ==================== BREADCRUMB ====================
  breadcrumbs = signal<BreadCrumbModel[]>([
    {
      title: 'Müşteriler',
      url: '/admin/customers',
      icon: 'ri-group-line',
      isActive: false
    },
    {
      title: 'Kara Liste',
      url: '/admin/customers/blacklist',
      icon: 'ri-forbid-2-line',
      isActive: true
    }
  ]);

  constructor() {
    effect(() => {
      const sort = this.state.sortField();
      console.log('🔍 sortField changed:', sort);
      // otomatik API çağrısı varsa burada
    });
  }

  // ==================== LIFECYCLE ====================
  ngOnInit(): void {
    this.breadcrumbService.reset(this.breadcrumbs());

    // ⭐ Kara liste için state hazırla
    this.state.resetFilters();
    this.state.filterIsBlacklisted.set(true);
    this.state.sortField.set('created');
    this.state.sortOrder.set(-1);

    this.loadBlacklistedCustomers();
  }

  ngOnDestroy(): void {
    // ⭐ Sayfadan çıkınca state temizle
    this.state.reset();
  }

  // ==================== LOAD ====================
  loadBlacklistedCustomers(): void {
    this.isLoading.set(true);
    const specParams = this.state.filterParams();

    this.customerService.getCustomers(specParams)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.isLoading.set(false))
      )
      .subscribe({
        next: (res) => {
          if (res.isSuccessful && res.data) {
            console.log(res.data);

            this.customers.set(res.data.items || []);
            this.totalCount.set(res.data.totalCount || 0);
          }
        },
        error: (err) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Hata',
            detail: err?.error?.errorMessages?.[0]
              || err?.error?.message
              || 'Kara liste yüklenemedi.'
          });
          console.error(err);
        }
      });
  }

  // ==================== FILTERS ====================
  onFilterChange(): void {
    this.state.resetPage();
    this.loadBlacklistedCustomers();
  }

  onResetFilters(): void {
    this.state.resetFilters();
    this.state.filterIsBlacklisted.set(true);
    this.loadBlacklistedCustomers();
  }

  // ==================== PAGINATION ====================
  onPageChange(event: TablePageEvent): void {
    const newPageSize = event.rows;
    const oldPageSize = this.pageSize();

    if (newPageSize !== oldPageSize) {
      this.pageSize.set(newPageSize);
      this.pageIndex.set(1);
    } else {
      this.pageIndex.set((event.first / event.rows) + 1);
    }

    this.loadBlacklistedCustomers();
  }

  onSortChange(selected: { field: string; order: number }): void {
    if (selected?.field) {
      this.state.sortField.set(selected.field);
      this.state.sortOrder.set(selected.order ?? -1);
      this.state.resetPage();
    }
    this.onFilterChange();
  }

  // onSortChange(selected: { field: string; order: number }): void {
  //   if (!selected?.field) return;

  //   const direction = selected.order === -1 ? 'desc' : 'asc';

  //   // ⭐ field'ı temizle: olası "asc"/"desc" son eklerini kaldır
  //   const cleanField = selected.field
  //     .toLowerCase()
  //     .replace(/(asc|desc)$/i, '');   // sadece SONDAKİ asc/desc'i kaldır

  //   const normalizedSort = `${cleanField}${direction}`;
  //   // companyname + desc → "companynamedesc" ✅

  //   this.state.sortField.set(normalizedSort);
  //   this.state.sortOrder.set(selected.order);
  //   this.state.resetPage();

  //   this.onFilterChange();
  // }

  // ==================== NAVIGATION ====================
  viewDetail(customer: CustomerListDto): void {
    this.router.navigate(['/admin/customers/detail', customer.id]);
  }

  // ==================== KARA LISTEDEN ÇIKAR ====================
  removeFromBlacklist(customer: CustomerListDto): void {
    const name = this.getFullName(customer);

    this.confirmDialog.showConfirm(
      'Kara Listeden Çıkar',
      `"${name}" müşterisini kara listeden çıkarmak istediğinize emin misiniz?`,
      () => {
        this.customerService.removeFromBlacklist(customer.id).subscribe({
          next: (res: Result<string>) => {
            if (res.isSuccessful) {
              this.messageService.add({
                severity: 'success',
                summary: 'Başarılı',
                detail: `${name} kara listeden çıkarıldı.`,
                life: 4000
              });
              this.loadBlacklistedCustomers();   // ⭐ Listeyi yenile
            } else {
              this.messageService.add({
                severity: 'warn',
                summary: 'Uyarı',
                detail: res.errorMessages?.[0] || 'İşlem başarısız.'
              });
            }
          },
          error: (err) => {
            this.messageService.add({
              severity: 'error',
              summary: 'Hata',
              detail: err?.error?.errorMessages?.[0]
                || err?.error?.message
                || 'Sunucu hatası.'
            });
          }
        });
      }
    );
  }

  // ==================== HELPERS ====================
  getFullName(customer: CustomerListDto): string {
    if (customer.type === 'Corporate') {
      return customer.companyName || 'Kurumsal Müşteri';
    }
    return `${customer.firstName || ''} ${customer.lastName || ''}`.trim() || 'İsimsiz';
  }

  getInitials(customer: CustomerListDto): string {
    const name = this.getFullName(customer);
    if (!name) return 'M';
    return name
      .split(' ')
      .filter(n => n.length > 0)
      .map(n => n.charAt(0))
      .slice(0, 2)
      .join('')
      .toUpperCase();
  }

  getTypeLabel(type: string): string {
    return type === 'Corporate' ? 'Kurumsal' : 'Bireysel';
  }

  formatDate(dateStr: string | null): string {
    if (!dateStr) return '-';
    try {
      return new Date(dateStr).toLocaleDateString('tr-TR', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric'
      });
    } catch {
      return '-';
    }
  }

}
