import { CurrencyPipe, DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal, ViewEncapsulation } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { DatePickerModule } from 'primeng/datepicker';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { TableModule, TablePageEvent } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { BreadcrumbService } from '../../../core/services/breadcrumb.service';
import { ReservationService } from '../../../core/services/reservation.service';
import { ReservationStateService } from '../../../core/services/state-services/reservation-state.service';
import { CustomConfirmDialogService } from '../../../shared/services/custom-confirm-dialog.service';
import { ReservationStats } from '../../../core/models/reservations/reservation-stats.model';
import { Reservation } from '../../../core/models/reservations/reservation.model';
import { BreadCrumbModel } from '../../../core/models/breadcrumb';

@Component({
  selector: 'app-reservations-active',
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
    DialogModule,
    DatePickerModule,
  ],
  templateUrl: './reservations-active.component.html',
  styleUrl: './reservations-active.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.Emulated
})
export class ReservationsActiveComponent {
  private reservationService = inject(ReservationService);
  private stateService = inject(ReservationStateService);
  private messageService = inject(MessageService);
  private confirmDialog = inject(CustomConfirmDialogService);
  private breadcrumbService = inject(BreadcrumbService);
  private router = inject(Router);

  // ⭐ SADECE BU 3 DURUM
  readonly ACTIVE_STATUSES = ['PendingPayment', 'Confirmed', 'Active'] as const;

  // Signals
  readonly reservations = signal<Reservation[]>([]);
  readonly totalCount = signal<number>(0);
  readonly isLoading = signal<boolean>(false);

  // State
  readonly filterSearch = this.stateService.filterSearch;
  readonly paymentFilter = this.stateService.paymentFilter;
  readonly pageIndex = this.stateService.pageIndex;
  readonly pageSize = this.stateService.pageSize;
  readonly sortField = this.stateService.sortField;
  readonly sortOrder = this.stateService.sortOrder;

  readonly stats = signal<ReservationStats | null>(null);

  readonly statusOptions: { label: string; value: string | null }[] = [
    { label: 'Tümü (Aktif Süreç)', value: null },
    { label: 'Kaporası Alınmış', value: 'deposit' },
    { label: 'Tam Ödemesi Alınmış', value: 'full' },
    { label: 'Kirada', value: 'active' },
  ];
  readonly breadcrumbs = signal<BreadCrumbModel[]>([
    { title: 'Rezervasyonlar', url: '/admin/reservations', icon: 'ri-calendar-check-line', isActive: false },
    { title: 'Aktif Rezervasyonlar', url: '/admin/reservations/active', icon: 'ri-radar-line', isActive: true }
  ]);

  // ⭐ Aktifleştirme dialog state
  readonly showActivateDialog = signal(false);
  readonly activatingReservation = signal<Reservation | null>(null);
  readonly activatePickupAt = signal<Date | null>(new Date());
  readonly isActivating = signal(false);

  // ⭐ İade alma dialog state
  readonly showCompleteDialog = signal(false);
  readonly completingReservation = signal<Reservation | null>(null);
  readonly completeReturnAt = signal<Date | null>(new Date());
  readonly isCompleting = signal(false);

  ngOnInit(): void {
    this.breadcrumbService.reset(this.breadcrumbs());
    this.stateService.activateActiveMode();   // ⭐ AKTİF MODU AÇ
    this.loadStats();
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
          detail: 'Aktif rezervasyonlar yüklenemedi.'
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
      case 'Confirmed': return 'info';
      case 'Active': return 'success';
      default: return 'secondary';
    }
  }
  getStatusLabel(status: string): string {
    const opt = this.statusOptions.find(o => o.value === status);
    return opt ? opt.label : status;
  }

  getStatusCount(key: string): number {
    const s = this.stats();
    if (!s) return 0;

    switch (key) {
      case 'deposit': return s.depositOnlyCount;    // 🆕
      case 'full': return s.fullPaidCount;       // 🆕
      case 'active': return s.activeCount;
      default: return 0;
    }
  }


getActiveTotalCount(): number {
  const s = this.stats();
  if (!s) return 0;
  return s.depositOnlyCount + s.fullPaidCount + s.activeCount;   // 🆕
}

  loadStats(): void {
    this.reservationService.getStats().subscribe({
      next: (res) => {
        if (res.isSuccessful && res.data) {
          this.stats.set(res.data);
        }
      },
      error: (err) => console.error('Stats yüklenemedi:', err)
    });
  }

  navigateToDetail(reservation: Reservation): void {
    this.router.navigate(['/admin/reservations/detail', reservation.id]);
  }

  // ==================== ARAÇ TESLİM ET (Confirmed → Active) ====================
  openActivateDialog(reservation: Reservation): void {
    this.activatingReservation.set(reservation);
    this.activatePickupAt.set(new Date());
    this.showActivateDialog.set(true);
  }

  confirmActivate(): void {
    const r = this.activatingReservation();
    const pickupAt = this.activatePickupAt();
    if (!r || !pickupAt) return;

    this.isActivating.set(true);

    this.reservationService.activate(r.id, pickupAt.toISOString()).subscribe({
      next: (res) => {
        if (res.isSuccessful) {
          this.messageService.add({
            severity: 'success',
            summary: 'Başarılı',
            detail: 'Araç teslim edildi, kiralama başladı.'
          });
          this.showActivateDialog.set(false);
          this.loadReservations();
          this.loadStats();
        }
      },
      error: (err) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Hata',
          detail: err?.error?.errorMessages?.[0] || 'Araç teslim edilemedi.'
        });
      },
      complete: () => this.isActivating.set(false)
    });
  }

  // ==================== İADE AL (Active → Completed) ====================
  openCompleteDialog(reservation: Reservation): void {
    this.completingReservation.set(reservation);
    this.completeReturnAt.set(new Date());
    this.showCompleteDialog.set(true);
  }

  confirmComplete(): void {
    const r = this.completingReservation();
    const returnAt = this.completeReturnAt();
    if (!r || !returnAt) return;

    this.isCompleting.set(true);

    this.reservationService.complete(r.id, returnAt.toISOString()).subscribe({
      next: (res) => {
        if (res.isSuccessful) {
          this.messageService.add({
            severity: 'success',
            summary: 'Başarılı',
            detail: 'Araç iade alındı, kiralama tamamlandı.'
          });
          this.showCompleteDialog.set(false);
          this.loadReservations();
          this.loadStats();
        }
      },
      error: (err) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Hata',
          detail: err?.error?.errorMessages?.[0] || 'İade alınamadı.'
        });
      },
      complete: () => this.isCompleting.set(false)
    });
  }


  // ==================== İPTAL ====================
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
              this.loadStats();
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


  ngOnDestroy(): void {
    // 🆕 Aktif modu temizle — tüm listeye dönünce etkilenmesin
    this.stateService.deactivateActiveMode();
  }



}
