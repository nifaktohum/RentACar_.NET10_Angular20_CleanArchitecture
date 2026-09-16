import { NgClass, CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, input, OnInit, output, signal, ViewEncapsulation } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { CheckboxModule } from 'primeng/checkbox';
import { DividerModule } from 'primeng/divider';
import { InputNumberModule } from 'primeng/inputnumber';
import { ExtraService } from '../../../core/services/extra.service';
import { MessageService } from 'primeng/api';
import { SelectedExtra } from '../../../core/models/rental-extra/selected-extra.model';
import { ExtraSummary } from '../../../core/models/extra/extraSummary';
import { ExtraCategoriesConfig, PriceTypeLabels } from '../../../core/models/extra/enum/extra-enums.model';
import { BreadCrumbModel } from '../../../core/models/breadcrumb';
import { BreadcrumbService } from '../../../core/services/breadcrumb.service';

@Component({
  selector: 'app-extra-selector',
  imports: [
    FormsModule,
    DividerModule,
    NgClass,
    CurrencyPipe,
    CheckboxModule,
    InputNumberModule,
    ButtonModule
  ],
  templateUrl: './extra-selector.component.html',
  styleUrl: './extra-selector.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.Emulated
})
export class ExtraSelectorComponent implements OnInit {
  private extraService = inject(ExtraService);
  private messageService = inject(MessageService);
  private breadcrumbService = inject(BreadcrumbService);

  // ==================== INPUTS ====================
  readonly rentalDays = input<number>(1);
  readonly selectedExtraIds = input<string[]>([]);

  // ==================== OUTPUTS ====================
  readonly extrasChanged = output<SelectedExtra[]>();

  // ==================== SIGNALS ====================
  readonly allExtras = signal<ExtraSummary[]>([]);
  readonly selectedExtras = signal<SelectedExtra[]>([]);
  readonly isLoading = signal<boolean>(false);
  readonly filterCategory = signal<string | null>(null);

  readonly breadcrumbs = signal<BreadCrumbModel[]>([
    {
      title: 'Extra Selection',
      url: '/admin/extras/extra-selector',
      icon: 'ri-checkbox-circle-line',
      isActive: true
    }
  ]);

  // ==================== COMPUTED ====================
  readonly totalPrice = computed(() => {
    return this.selectedExtras().reduce((sum, item) => sum + item.totalPrice, 0);
  });

  readonly selectedCount = computed(() => {
    return this.selectedExtras().length;
  });

  readonly categories = computed(() => {
    const cats = new Set<string>();
    this.allExtras().forEach(e => cats.add(e.category));
    return Array.from(cats).sort((a, b) =>
      ExtraCategoriesConfig[a]?.value - ExtraCategoriesConfig[b]?.value
    );
  });

  readonly filteredExtras = computed(() => {
    let items = this.allExtras();

    if (this.filterCategory()) {
      items = items.filter(e => e.category === this.filterCategory());
    }

    return items;
  });

  // ==================== LIFECYCLE ====================
  ngOnInit(): void {
    this.breadcrumbService.reset(this.breadcrumbs());
    this.loadExtras();
  }

  // ==================== LOAD METHODS ====================
  loadExtras(): void {
    this.isLoading.set(true);

    this.extraService.getAll().subscribe({
      next: (response) => {
        if (response.isSuccessful && response.data) {
          this.allExtras.set(response.data);

          // Daha önce seçilen extra'lar varsa işaretle
          const selectedIds = this.selectedExtraIds();
          if (selectedIds.length > 0) {
            const selected = response.data
              .filter(e => selectedIds.includes(e.id))
              .map(e => ({
                extra: e,
                quantity: 1,
                totalPrice: e.price * this.rentalDays()
              }));
            this.selectedExtras.set(selected);
          }
        }
        this.isLoading.set(false);
      },
      error: (err) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Hata',
          detail: 'Ekstra hizmetler yüklenirken hata oluştu.'
        });
        this.isLoading.set(false);
      }
    });
  }

  // ==================== SELECTION METHODS ====================
  toggleExtra(extra: ExtraSummary, event: any): void {
    const isChecked = event.checked;

    if (isChecked) {
      this.addExtra(extra);
    } else {
      this.removeExtra(extra.id);
    }
  }

  addExtra(extra: ExtraSummary): void {
    const current = this.selectedExtras();
    const exists = current.find(e => e.extra.id === extra.id);

    if (!exists) {
      const totalPrice = extra.priceType === 'Daily'
        ? extra.price * this.rentalDays()
        : extra.price;

      const updated = [...current, {
        extra,
        quantity: 1,
        totalPrice
      }];

      this.selectedExtras.set(updated);
      this.emitChanges();
    }
  }

  removeExtra(extraId: string): void {
    const current = this.selectedExtras();
    const updated = current.filter(e => e.extra.id !== extraId);
    this.selectedExtras.set(updated);
    this.emitChanges();
  }

  updateQuantity(extraId: string, quantity: number): void {
    if (quantity < 1) {
      this.removeExtra(extraId);
      return;
    }

    const current = this.selectedExtras();
    const item = current.find(e => e.extra.id === extraId);

    if (item) {
      const extra = item.extra;
      const totalPrice = extra.priceType === 'Daily'
        ? extra.price * this.rentalDays() * quantity
        : extra.price * quantity;

      const updated = current.map(e =>
        e.extra.id === extraId
          ? { ...e, quantity, totalPrice }
          : e
      );

      this.selectedExtras.set(updated);
      this.emitChanges();
    }
  }

  // ==================== FILTER METHODS ====================
  setFilter(category: string | null): void {
    this.filterCategory.set(category);
  }

  clearFilter(): void {
    this.filterCategory.set(null);
  }

  // ==================== HELPERS ====================
  // getCategoryLabel(category: string): string {
  //   return ExtraCategoryLabels[category] || category;
  // }

  getCategoryLabel(category: string): string {
    return ExtraCategoriesConfig[category]?.label || category;
  }

  getPriceTypeLabel(priceType: string): string {
    return PriceTypeLabels[priceType as keyof typeof PriceTypeLabels] || priceType;
  }

  isSelected(extraId: string): boolean {
    return this.selectedExtras().some(e => e.extra.id === extraId);
  }

  getQuantity(extraId: string): number {
    const item = this.selectedExtras().find(e => e.extra.id === extraId);
    return item?.quantity || 0;
  }

  private emitChanges(): void {
    this.extrasChanged.emit(this.selectedExtras());
  }


}
