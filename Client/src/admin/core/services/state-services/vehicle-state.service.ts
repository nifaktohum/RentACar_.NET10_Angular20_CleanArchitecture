import { computed, Injectable, signal } from '@angular/core';
import { VehicleFilterParams } from '../../models/vehicle/vehicle-filter-params';

@Injectable({
  providedIn: 'root',
})
export class VehicleStateService {
  // ⭐ Sayfalama
  readonly pageIndex = signal<number>(1);
  readonly pageSize = signal<number>(5);

  // ⭐ Sıralama
  readonly sortField = signal<string>('createdAt');
  readonly sortOrder = signal<number>(-1);

  // ⭐ Filtreler
  readonly filterSearch = signal<string>('');
  readonly selectedBrand = signal<string>('');
  readonly selectedModel = signal<string>('');
  readonly selectedType = signal<string>('');
  readonly filterIsAvailable = signal<boolean | null>(null);

  // 🚀 Tam Reaktif Parametre Nesnesi (Filtre değiştiğinde otomatik tetiklenir)
  readonly filterParams = computed<VehicleFilterParams>(() => {
    const params: VehicleFilterParams = {
      pageNumber: this.pageIndex(),
      pageSize: this.pageSize()
    };

    if (this.selectedBrand()) params.brands = [this.selectedBrand()];
    if (this.selectedModel()) params.models = [this.selectedModel()];
    if (this.selectedType()) params.types = [this.selectedType()];
    if (this.filterSearch()) params.searchTerm = this.filterSearch();
    if (this.filterIsAvailable() !== null) params.isAvailable = this.filterIsAvailable();
    if (this.sortField()) {
      const direction = this.sortOrder() === 1 ? 'Asc' : 'Desc';
      params.sort = `${this.sortField()}${direction}`;
    }

    return params;
  });

  // ⭐ Filtre var mı?
  readonly hasActiveFilters = computed(() => {
    return !!(
      this.selectedBrand() ||
      this.selectedModel() ||
      this.selectedType() ||
      this.filterSearch() ||
      this.filterIsAvailable() !== null
    );
  });

  // ⭐ State'i sıfırla (opsiyonel)
  reset(): void {
    this.pageIndex.set(1);
    this.pageSize.set(5);
    this.sortField.set('createdAt');
    this.sortOrder.set(-1);
    this.filterSearch.set('');
    this.selectedBrand.set('');
    this.selectedModel.set('');
    this.selectedType.set('');
    this.filterIsAvailable.set(null);
  }

  // ⭐ Sadece filtreleri sıfırla
  resetFilters(): void {
    this.filterSearch.set('');
    this.selectedBrand.set('');
    this.selectedModel.set('');
    this.selectedType.set('');
    this.filterIsAvailable.set(null);
    this.pageIndex.set(1);
  }

  // ⭐ Sayfa numarasını sıfırla (filtre değişince)
  resetPage(): void {
    this.pageIndex.set(1);
  }


}
