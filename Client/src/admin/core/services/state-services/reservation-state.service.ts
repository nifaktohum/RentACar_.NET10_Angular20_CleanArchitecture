import { computed, Injectable, signal } from '@angular/core';
import { ReservationFilter } from '../../models/reservations/reservation-filter.model';

@Injectable({
  providedIn: 'root',
})
export class ReservationStateService {
  // ⭐ Filtreler
  readonly filterSearch = signal<string>('');
  readonly selectedStatus = signal<string | null>(null);

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
    };
  });

  private getSortString(): string | null {
    const field = this.sortField();
    const order = this.sortOrder();
    if (!field) return null;

    // Backend'deki sort stringleri (örn: "createdDesc", "priceAsc")
    const direction = order === 1 ? 'Asc' : 'Desc';
    return `${field}${direction}`;
  }

  resetPage(): void {
    this.pageIndex.set(1);
  }

  resetFilters(): void {
    this.filterSearch.set('');
    this.selectedStatus.set(null);
    this.resetPage();
  }

}
