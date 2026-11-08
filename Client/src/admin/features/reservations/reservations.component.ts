// reservations.component.ts
import { ChangeDetectionStrategy, Component, inject, OnInit, signal, ViewEncapsulation } from '@angular/core';
import { Router } from '@angular/router';
import { MessageService } from 'primeng/api';
import { BreadcrumbService } from '../../core/services/breadcrumb.service';
import { ReservationService } from '../../core/services/reservation.service';
import { ReservationStateService } from '../../core/services/state-services/reservation-state.service';
import { CustomConfirmDialogService } from '../../shared/services/custom-confirm-dialog.service';
import { Reservation } from '../../core/models/reservations/reservation.model';
import { BreadCrumbModel } from '../../core/models/breadcrumb';
import { TableModule, TablePageEvent } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { CurrencyPipe, DatePipe, NgIf } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { FormsModule } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { TooltipModule } from 'primeng/tooltip';

@Component({
  selector: 'app-reservations',
  imports: [
    TagModule,
    CurrencyPipe,
    DatePipe,
    TableModule,
    ButtonModule,
    SelectModule,
    FormsModule,
    CardModule,
    InputTextModule,
    TooltipModule,
  ],
  templateUrl: './reservations.component.html',
  styleUrl: './reservations.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.Emulated
})
export class ReservationsComponent implements OnInit {
  private reservationService = inject(ReservationService);
  private stateService = inject(ReservationStateService);
  private messageService = inject(MessageService);
  private confirmDialog = inject(CustomConfirmDialogService);
  private breadcrumbService = inject(BreadcrumbService);
  private router = inject(Router);

  // Signals
  readonly reservations = signal<Reservation[]>([]);
  readonly totalCount = signal<number>(0);
  readonly isLoading = signal<boolean>(false);

  // State Signals
  readonly filterSearch = this.stateService.filterSearch;
  readonly selectedStatus = this.stateService.selectedStatus;
  readonly pageIndex = this.stateService.pageIndex;
  readonly pageSize = this.stateService.pageSize;
  readonly sortField = this.stateService.sortField;
  readonly sortOrder = this.stateService.sortOrder;

  // ✅ Tip belirtildi
  readonly statusOptions: { label: string; value: string | null }[] = [
    { label: 'Tümü', value: null },
    { label: 'Ödeme Bekliyor', value: 'PendingPayment' },
    { label: 'Onaylandı', value: 'Confirmed' },
    { label: 'Aktif (Teslim Edildi)', value: 'Active' },
    { label: 'Tamamlandı', value: 'Completed' },
    { label: 'Süresi Doldu', value: 'Expired' },
    { label: 'Gelmedi (NoShow)', value: 'NoShow' },
    { label: 'İptal Edildi', value: 'Cancelled' }
  ];

  readonly breadcrumbs = signal<BreadCrumbModel[]>([
    { title: 'Rezervasyonlar', url: '/admin/reservations', icon: 'ri-calendar-check-line', isActive: true }
  ]);

  ngOnInit(): void {
    this.breadcrumbService.reset(this.breadcrumbs());
    this.loadReservations();
  }

  loadReservations(): void {
    this.isLoading.set(true);
    const params = this.stateService.filterParams();

    this.reservationService.getAll(params).subscribe({
      next: (res) => {
        if (res.isSuccessful && res.data) {
          this.reservations.set(res.data.items || []);
          this.totalCount.set(res.data.totalCount || 0);
        }
      },
      error: (err) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Hata',
          detail: 'Rezervasyonlar yüklenemedi.'
        });
        console.error(err);
      },
      complete: () => this.isLoading.set(false)
    });
  }

  onFilterChange(): void {
    this.stateService.resetPage();
    this.loadReservations();
  }

  onResetFilters(): void {
    this.stateService.resetFilters();
    this.loadReservations();
  }

  onPageChange(event: TablePageEvent): void {
    const newPageSize = event.rows;
    if (newPageSize !== this.pageSize()) {
      this.pageSize.set(newPageSize);
      this.pageIndex.set(1);
    } else {
      this.pageIndex.set(event.first / event.rows + 1);
    }
    this.loadReservations();
  }

  onSort(event: any): void {
    if (event.field) {
      this.sortField.set(event.field);
      this.sortOrder.set(event.order);
      this.stateService.resetPage();
      this.loadReservations();
    }
  }

  getStatusSeverity(status: string): 'success' | 'info' | 'warn' | 'danger' | 'secondary' | 'contrast' {
    switch (status) {
      case 'PendingPayment': return 'warn';
      case 'Confirmed': return 'info';
      case 'Active': return 'success';
      case 'Completed': return 'success';
      case 'Expired': return 'secondary';
      case 'NoShow': return 'danger';
      case 'Cancelled': return 'danger';
      default: return 'secondary';
    }
  }

  getStatusLabel(status: string): string {
    const opt = this.statusOptions.find(o => o.value === status);
    return opt ? opt.label : status;
  }

  navigateToDetail(reservation: Reservation): void {
    this.router.navigate(['/admin/reservations/detail', reservation.id]);
  }

  // ✅ CustomConfirmDialogService kullanıldı
  cancelReservation(reservation: Reservation): void {
    this.confirmDialog.showConfirm(
      'Rezervasyon İptali',
      `"${reservation.reservationNumber}" numaralı rezervasyonu iptal etmek istediğinize emin misiniz?`,
      () => {
        this.reservationService.cancel(reservation.id, 'Admin tarafından iptal edildi.').subscribe({
          next: (res) => {
            if (res.isSuccessful) {
              this.messageService.add({
                severity: 'success',
                summary: 'Başarılı',
                detail: 'Rezervasyon iptal edildi.'
              });
              this.loadReservations();
            }
          },
          error: (err) => {
            this.messageService.add({
              severity: 'error',
              summary: 'Hata',
              detail: err?.error?.errorMessages?.[0] || 'İptal edilemedi.'
            });
          }
        });
      },
      undefined,
      'Evet, İptal Et',
      'Vazgeç',
      'p-button-danger'
    );
  }
}