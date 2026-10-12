import { ChangeDetectionStrategy, Component, computed, DestroyRef, inject, OnInit, signal, ViewEncapsulation } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { DividerModule } from 'primeng/divider';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { SkeletonModule } from 'primeng/skeleton';
import { TableModule, TablePageEvent } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { PhoneFormatPipe } from '../../../shared/pipes/phone-format.pipe';
import { MessageService } from 'primeng/api';
import { BreadcrumbService } from '../../core/services/breadcrumb.service';
import { CustomerManagementService } from '../../core/services/customers/customer-management.service';
import { CustomerListDto } from '../../../core/models/customer.model';
import { BreadCrumbModel } from '../../core/models/breadcrumb';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { catchError, debounceTime, distinctUntilChanged, finalize, map, merge, of, Subject, switchMap, tap, withLatestFrom } from 'rxjs';
import { Result } from '../../../core/models/result.model';
import { NgClass } from '@angular/common';
import { CustomerStateService } from '../../core/services/customers/customer-state.service';
import { CustomerType } from '../../../shared/enums/customer-type';

@Component({
  selector: 'app-customers',
  imports: [
    FormsModule,
    CardModule,
    ButtonModule,
    TableModule,
    SelectModule,
    InputTextModule,
    TagModule,
    TooltipModule,
    SkeletonModule,
    DividerModule,
    PhoneFormatPipe,
    NgClass
  ],
  templateUrl: './customers.component.html',
  styleUrl: './customers.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.Emulated
})
export class CustomersComponent implements OnInit {
  private customerService = inject(CustomerManagementService);
  private stateService = inject(CustomerStateService);   // ⭐ EKLENDİ
  private breadcrumbService = inject(BreadcrumbService);
  private messageService = inject(MessageService);
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);

  // ==================== SIGNALS ====================
  customers = signal<CustomerListDto[]>([]);
  totalCount = signal<number>(0);
  isLoading = signal<boolean>(false);

  // ==================== 🎛️ STATE SERVICE KISAYOLLARI ====================
  // Template'in direkt erişebilmesi için public yapıyoruz
  readonly pageIndex = this.stateService.pageIndex;
  readonly pageSize = this.stateService.pageSize;
  readonly filterSearch = this.stateService.filterSearch;
  readonly filterType = this.stateService.selectedType;
  readonly filterIsBlacklisted = this.stateService.filterIsBlacklisted;
  readonly filterIsEmailVerified = this.stateService.filterIsEmailVerified;
  readonly filterIsPhoneVerified = this.stateService.filterIsPhoneVerified;
  readonly filterIsActive = this.stateService.filterIsActive;

  // ⭐ Manuel yenileme tetikleyicisi
  private refreshTrigger = new Subject<void>();

  // ==================== COMPUTED ====================
  // ⭐ hasActiveFilters artık state service'ten
  readonly hasActiveFilters = this.stateService.hasActiveFilters;

  // ⭐ Sort — template'te select box için computed
  readonly selectedSortOption = computed(() => {
    const dir = this.stateService.sortOrder() === 1 ? 'Asc' : 'Desc';
    return `${this.stateService.sortField()}${dir}`;
  });

  // ==================== 🎛️ OPTIONS ====================
  typeOptions = [
    { label: 'Tümü', value: null },
    { label: 'Bireysel', value: CustomerType.Individual },  // ⭐ enum
    { label: 'Kurumsal', value: CustomerType.Corporate }
  ];

  booleanOptions = [
    { label: 'Tümü', value: null },
    { label: 'Evet', value: true },
    { label: 'Hayır', value: false }
  ];

  sortOptions = [
    // Tarih
    { label: 'En Yeni', value: 'createdDesc' },
    { label: 'En Eski', value: 'createdAsc' },

    // Ad (Bireysel)
    { label: 'Ad (A-Z)', value: 'firstNameAsc' },
    { label: 'Ad (Z-A)', value: 'firstNameDesc' },

    // Soyad
    { label: 'Soyad (A-Z)', value: 'lastNameAsc' },
    { label: 'Soyad (Z-A)', value: 'lastNameDesc' },

    // Şirket
    { label: 'Şirket (A-Z)', value: 'companyNameAsc' },
    { label: 'Şirket (Z-A)', value: 'companyNameDesc' },

    // Email
    { label: 'Email (A-Z)', value: 'emailAsc' },
    { label: 'Email (Z-A)', value: 'emailDesc' },

    // Findeks
    { label: 'Findeks (Yüksek)', value: 'findeksDesc' },
    { label: 'Findeks (Düşük)', value: 'findeksAsc' },

    // Diğer
    { label: 'Şehir (A-Z)', value: 'cityAsc' },
    { label: 'Kara Liste Önce', value: 'blacklistDesc' },
  ];

  // ==================== 📡 RXJS AKIŞLARI (STREAMS) ====================
  // 1. Filtre değişimlerini dinleyen akış sınıf seviyesine taşındı
  private filterChanges$ = toObservable(this.stateService.filterParams).pipe(
    debounceTime(300),
    distinctUntilChanged((a, b) => JSON.stringify(a) === JSON.stringify(b))
  );

  // 2. Yenileme butonunu dinleyen akış sınıf seviyesine taşındı
  private refresh$ = this.refreshTrigger.pipe(
    map(() => this.stateService.filterParams())
  );


  // ==================== BREADCRUMB ====================
  breadcrumbs = signal<BreadCrumbModel[]>([
    {
      title: 'Müşteriler',
      url: '/admin/customers',
      icon: 'ri-group-line',
      isActive: true
    }
  ]);

  // ==================== 🚀 CONSTRUCTOR ====================
  constructor() {
    // Constructor artık sadece akışları birleştirip süreci başlatıyor! Tertemiz.
    merge(this.filterChanges$, this.refresh$)
      .pipe(
        tap(() => this.isLoading.set(true)),
        switchMap((params) => this.fetchCustomers(params)), // İstek fonksiyona gitti
        takeUntilDestroyed()
      )
      .subscribe((res) => this.updateState(res)); // Yanıt fonksiyona gitti
  }

  ngOnInit(): void {
    this.breadcrumbService.reset(this.breadcrumbs());
  }

  // ==================== 🎛️ FILTER HANDLER'LARI ====================
  onFilterChange(): void {
    this.stateService.resetPage();   // ⭐ sadece state güncelle, fetch otomatik
  }

  onResetFilters(): void {
    this.stateService.resetFilters();   // ⭐ hepsi sıfırlanır, fetch otomatik
  }

  // ==================== PAGINATION ====================
  // Template'ten `[(ngModel)]` ile bağlanıyorsan gerek yok ama
  // `(ngModelChange)` ile manuel bağlarsan:
  onSearchChange(value: string): void {
    this.stateService.filterSearch.set(value);
    this.stateService.resetPage();
  }

  onSortChange(value: string): void {
    // "createdAtDesc" → field: "createdAt", order: -1
    const match = value.match(/^(.+?)(Asc|Desc)$/);
    if (match) {
      const [, field, dir] = match;
      this.stateService.sortField.set(field);
      this.stateService.sortOrder.set(dir === 'Asc' ? 1 : -1);
      this.stateService.resetPage();
    }
  }

  // ==================== 📄 PAGINATION ====================
  onPageChange(event: TablePageEvent): void {
    const newPageSize = event.rows;
    const oldPageSize = this.pageSize();

    if (newPageSize !== oldPageSize) {
      this.stateService.pageSize.set(newPageSize);
      this.stateService.pageIndex.set(1);
    } else {
      this.stateService.pageIndex.set((event.first / event.rows) + 1);
    }
    // ⭐ fetch otomatik
  }

  // ==================== 🧭 NAVIGATION ====================
  viewDetail(customer: CustomerListDto): void {
    this.router.navigate(['/admin/customers/detail', customer.id]);
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

  getTypeSeverity(type: string): 'info' | 'success' | 'warn' | 'danger' | 'secondary' | 'contrast' {
    return type === 'Corporate' ? 'info' : 'success';
  }

  getFindeksSeverity(score: number | null): 'success' | 'info' | 'warn' | 'danger' | 'secondary' {
    if (score == null) return 'secondary';
    if (score >= 1500) return 'success';
    if (score >= 1200) return 'info';
    if (score >= 900) return 'warn';
    return 'danger';
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

  // ============================================================
  // 🔄 MANUEL YENİLE
  // ============================================================
  refreshData(): void {
    this.refreshTrigger.next();
  }

  // ==================== ⚙️ İŞ KODLARI (METOTLAR) ====================
  private fetchCustomers(params: any) {
    return this.customerService.getCustomers(params).pipe(
      catchError((err) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Hata',
          detail: err?.error?.errorMessages?.[0] || err?.error?.message || 'Müşteri listesi yüklenemedi.'
        });
        console.error('[fetchCustomers] error:', err);
        return of({ isSuccessful: false, data: null } as Result<any>);
      }),
      finalize(() => this.isLoading.set(false))
    );
  }

  private updateState(res: Result<any>): void {
    if (res?.isSuccessful && res?.data) {
      this.customers.set(res.data.items ?? []);
      this.totalCount.set(res.data.totalCount ?? 0);
    }
  }

}
