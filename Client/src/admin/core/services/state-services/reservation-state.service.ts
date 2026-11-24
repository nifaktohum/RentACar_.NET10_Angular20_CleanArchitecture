// reservation-state.service.ts
import { computed, Injectable, signal } from '@angular/core';
import { ReservationFilter } from '../../models/reservations/reservation-filter.model';

@Injectable({
  providedIn: 'root',
})
export class ReservationStateService {
  // ⭐ Filtreler
  readonly filterSearch = signal<string>('');
  readonly selectedStatus = signal<string | null>(null);

  // 🆕 Aktif Rezervasyonlar için
  readonly activeOnly = signal<boolean>(false);
  readonly paymentFilter = signal<string | null>(null);   // 'deposit' | 'full' | 'active'

  // ⭐ Pagination & Sorting
  readonly pageIndex = signal<number>(1);
  readonly pageSize = signal<number>(10);

  // ⭐ Sıralama
  readonly sortField = signal<string | null>('createdAt');
  readonly sortOrder = signal<number>(-1); // -1 Desc, 1 Asc

  readonly filterParams = computed<ReservationFilter>(() => {
    return {
      searchTerm: this.filterSearch() || null,
      status: this.selectedStatus() || null,
      pageNumber: this.pageIndex(),
      pageSize: this.pageSize(),
      sort: this.getSortString(),
      activeOnly: this.activeOnly(),
      paymentFilter: this.paymentFilter(),
    };
  });

  private getSortString(): string | null {
    const field = this.sortField();
    const order = this.sortOrder();
    if (!field) return null;

    const direction = order === 1 ? 'Asc' : 'Desc';
    return `${field}${direction}`;
  }

  resetPage(): void {
    this.pageIndex.set(1);
  }

  resetFilters(): void {
    this.filterSearch.set('');
    this.selectedStatus.set(null);
    this.paymentFilter.set(null);
    this.resetPage();
  }

  // 🆕 Aktif sayfa için: tüm listeye dönerken aktif filtreyi temizle
  deactivateActiveMode(): void {
    this.activeOnly.set(false);
    this.paymentFilter.set(null);
  }

  // 🆕 Aktif sayfaya girerken: aktif modu aç
  activateActiveMode(): void {
    this.activeOnly.set(true);
    this.paymentFilter.set(null);
    this.selectedStatus.set(null);
    this.filterSearch.set('');
    this.resetPage();
  }
}