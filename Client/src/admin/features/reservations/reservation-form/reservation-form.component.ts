import { ChangeDetectionStrategy, Component, computed, inject, signal, ViewEncapsulation, OnInit } from '@angular/core';
import { CustomerListDto } from '../../../core/models/customers/customer.model';
import { BranchModel } from '../../../core/models/branch.model';
import { Vehicle } from '../../../core/models/vehicle/vehicle.model';
import { ExtraSummary } from '../../../core/models/extra/extraSummary';
import { SelectedExtra } from '../../../core/models/rental-extra/selected-extra.model';
import { BreadCrumbModel } from '../../../core/models/breadcrumb';
import { BreadcrumbService } from '../../../core/services/breadcrumb.service';
import { CustomerManagementService } from '../../../core/services/customers/customer-management.service';
import { MessageService } from 'primeng/api';
import { BranchService } from '../../../../core/services/branch.service';
import { VehicleService } from '../../../core/services/vehicle.service';
import { ExtraService } from '../../../core/services/extra.service';
import { CreateReservationRequest } from '../../../core/models/reservations/reservation-create.model';
import { ReservationService } from '../../../core/services/reservation.service';
import { Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DividerModule } from 'primeng/divider';
import { StepperModule } from 'primeng/stepper';
import { InputNumber } from 'primeng/inputnumber';
import { MessageModule } from 'primeng/message';
import { DatePickerModule } from 'primeng/datepicker';
import { SelectModule } from 'primeng/select';
import { TagModule } from 'primeng/tag';
import { AutoComplete } from 'primeng/autocomplete';
import { CardModule } from 'primeng/card';
import { TextareaModule } from 'primeng/textarea';
import { TooltipModule } from 'primeng/tooltip';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-reservation-form',
  imports: [
    FormsModule,
    CurrencyPipe,
    DatePipe,
    ButtonModule,
    DividerModule,
    StepperModule,
    InputNumber,
    MessageModule,
    DatePickerModule,
    SelectModule,
    TagModule,
    AutoComplete,
    CardModule,
    TextareaModule,
    TooltipModule
  ],
  templateUrl: './reservation-form.component.html',
  styleUrl: './reservation-form.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.Emulated
})
export class ReservationFormComponent implements OnInit {
  private readonly breadcrumbService = inject(BreadcrumbService);
  private readonly customerService = inject(CustomerManagementService);
  private readonly messageService = inject(MessageService);
  private readonly branchService = inject(BranchService);
  private readonly vehicleService = inject(VehicleService);
  private readonly extraService = inject(ExtraService);
  private readonly reservationService = inject(ReservationService);
  private readonly router = inject(Router);

  // ==================== SABİTLER ====================
  // Takvimin geçmişi göstermesini engellemek için bugünün gece yarısı (00:00) alınır
  readonly today = new Date(new Date().setHours(0, 0, 0, 0));
  readonly minReturnDate = computed(() => this.plannedStartDate() ?? this.today);
  readonly currentStep = signal<number>(this.getInitialStep());

  // ==================== STEP 1: MÜŞTERİ ====================
  readonly customerSuggestions = signal<CustomerListDto[]>([]);
  readonly selectedCustomer = signal<CustomerListDto | null>(null);
  readonly isSearchingCustomer = signal<boolean>(false);

  // ==================== STEP 2: TARİH + ŞUBE ====================
  readonly branches = signal<BranchModel[]>([]);
  readonly selectedPickupBranch = signal<BranchModel | null>(null);
  readonly selectedReturnBranch = signal<BranchModel | null>(null);
  readonly plannedStartDate = signal<Date | null>(null);
  readonly plannedEndDate = signal<Date | null>(null);

  // ==================== STEP 3: ARAÇ ====================
  readonly availableVehicles = signal<Vehicle[]>([]);
  readonly selectedVehicle = signal<Vehicle | null>(null);
  readonly isLoadingVehicles = signal<boolean>(false);

  // ==================== STEP 4: EXTRALAR ====================
  readonly allExtras = signal<ExtraSummary[]>([]);
  readonly selectedExtras = signal<SelectedExtra[]>([]);
  readonly isLoadingExtras = signal<boolean>(false);

  // ==================== STEP 5: ÖZET ====================
  readonly notes = signal<string>('');
  readonly isSubmitting = signal<boolean>(false);

  // ==================== HESAPLAMALAR ====================
  readonly plannedDays = computed(() => {
    const s = this.plannedStartDate();
    const e = this.plannedEndDate();
    if (!s || !e) return 0;

    const startUtc = Date.UTC(s.getFullYear(), s.getMonth(), s.getDate());
    const endUtc = Date.UTC(e.getFullYear(), e.getMonth(), e.getDate());

    if (endUtc < startUtc) return 0;
    const diffDays = Math.round((endUtc - startUtc) / (1000 * 60 * 60 * 24));
    return Math.max(diffDays, 1);
  });

  readonly vehicleTotal = computed(() => {
    const v = this.selectedVehicle();
    const d = this.plannedDays();
    if (!v || d === 0) return 0;
    return v.dailyPrice * d;
  });

  readonly extrasTotal = computed(() => {
    const d = this.plannedDays();
    return this.selectedExtras().reduce((sum, se) => {
      const unit = se.extra.priceType === 'Daily' ? se.extra.price * d : se.extra.price;
      return sum + unit * se.quantity;
    }, 0);
  });

  readonly grandTotal = computed(() => this.vehicleTotal() + this.extrasTotal());
  readonly depositAmount = computed(() => Math.round(this.grandTotal() * 0.20 * 100) / 100);
  readonly remainingAmount = computed(() => Math.round((this.grandTotal() - this.depositAmount()) * 100) / 100);

  // ==================== ADIM VALİDASYONLARI ====================
  readonly isStep1Valid = computed(() => this.selectedCustomer() !== null);
  readonly isStep2Valid = computed(() => {
    const s = this.plannedStartDate();
    const e = this.plannedEndDate();
    if (!s || !e || !this.selectedPickupBranch()) return false;
    return e.getTime() > s.getTime();
  });
  readonly isStep3Valid = computed(() => this.selectedVehicle() !== null);
  readonly isStep4Valid = computed(() => true);
  readonly isAllValid = computed(() =>
    this.isStep1Valid() && this.isStep2Valid() && this.isStep3Valid() && this.isStep4Valid());

  // ==================== BREADCRUMB ====================
  readonly breadcrumbs = signal<BreadCrumbModel[]>([
    { title: 'Rezervasyonlar', url: '/admin/reservations', icon: 'ri-calendar-check-line', isActive: false },
    { title: 'Yeni Rezervasyon', url: '/admin/reservations/add', icon: 'ri-add-line', isActive: true },
  ]);

  // ==================== LIFECYCLE ====================
  ngOnInit(): void {
    this.breadcrumbService.reset(this.breadcrumbs());
    this.loadExtras();

    const draft = localStorage.getItem('reservation_draft');
    if (draft) {
      try {
        const parsed = JSON.parse(draft);
        if (parsed.customer) this.selectedCustomer.set(parsed.customer);
        if (parsed.pickupBranch) this.selectedPickupBranch.set(parsed.pickupBranch);
        if (parsed.returnBranch) this.selectedReturnBranch.set(parsed.returnBranch);

        // Tarihleri güvenli bir şekilde (Timezone kayması olmadan) geri yükle
        if (parsed.startDate) {
          const sd = new Date(parsed.startDate);
          if (!isNaN(sd.getTime())) this.plannedStartDate.set(sd);
        }
        if (parsed.endDate) {
          const ed = new Date(parsed.endDate);
          if (!isNaN(ed.getTime())) this.plannedEndDate.set(ed);
        }

        if (parsed.vehicle) this.selectedVehicle.set(parsed.vehicle);
        if (parsed.extras) this.selectedExtras.set(parsed.extras);
        if (parsed.notes) this.notes.set(parsed.notes);

        if (this.currentStep() === 3 && this.isStep2Valid()) {
          setTimeout(() => {
            this.loadAvailableVehicles();
          }, 500);
        }
      } catch (e) {
        console.error('Taslak yüklenemedi', e);
      }
    }

    this.loadBranches();
  
  }

  // Yerel saat dilimini koruyan ISO formatı (Timezone kaymasını önler)
  private toLocalISO(date: Date | null): string | null {
    if (!date) return null;
    const offset = date.getTimezoneOffset() * 60000;
    return new Date(date.getTime() - offset).toISOString();
  }

  saveDraft(): void {
    const draftData = {
      customer: this.selectedCustomer(),
      pickupBranch: this.selectedPickupBranch(),
      returnBranch: this.selectedReturnBranch(),
      startDate: this.toLocalISO(this.plannedStartDate()), // DÜZELTİLDİ
      endDate: this.toLocalISO(this.plannedEndDate()),     // DÜZELTİLDİ
      vehicle: this.selectedVehicle(),
      extras: this.selectedExtras(),
      notes: this.notes()
    };
    localStorage.setItem('reservation_draft', JSON.stringify(draftData));
  }

  private getInitialStep(): number {
    const saved = localStorage.getItem('reservation_current_step');
    return saved ? Number(saved) : 1;
  }

  onStepChange(step: number | undefined): void {
    if (step === undefined || step === null) return;

    this.currentStep.set(step);
    localStorage.setItem('reservation_current_step', step.toString());

    // 3. adıma gelindiyse ve liste boşsa yükle
    if (step === 3 && this.availableVehicles().length === 0 && this.isStep2Valid()) {
      this.loadAvailableVehicles();
    }
  }

  // ==================== STEP 1: MÜŞTERİ ====================
  searchCustomer(event: { query: string }): void {
    const term = event.query?.trim() ?? '';
    this.isSearchingCustomer.set(true);

    this.customerService.searchCustomers(term, 10).subscribe({
      next: (res) => {
        if (res.isSuccessful && res.data) {
          this.customerSuggestions.set(res.data.items);
        }
      },
      complete: () => this.isSearchingCustomer.set(false),
      error: () => {
        this.isSearchingCustomer.set(false);
        this.messageService.add({
          severity: 'error',
          summary: 'Hata',
          detail: 'Müşteri araması başarısız.',
        });
      },
    });
  }

  onCustomerSelected(customer: CustomerListDto): void {
    this.selectedCustomer.set(customer);
    this.invalidateVehicleSelection();
    this.saveDraft();
  }

  getCustomerDisplayName(c: CustomerListDto | null): string {
    if (!c) return '-';
    if (c.type === 'Corporate') return c.companyName?.trim() || '-';
    return `${c.firstName ?? ''} ${c.lastName ?? ''}`.trim() || '-';
  }

  // ==================== STEP 2: ŞUBE + TARİH ====================
  loadBranches(): void {
    this.branchService.getBranches(1, 100).subscribe({
      next: (res) => {
        if (res.isSuccessful && res.data) {
          this.branches.set(res.data.items.filter((b) => b.isActive));
        }
      },
      error: () => {
        this.messageService.add({
          severity: 'error',
          summary: 'Hata',
          detail: 'Şubeler yüklenemedi.',
        });
      },
    });
  }

  onDatesOrBranchChanged(): void {
    this.invalidateVehicleSelection();
  }

  private invalidateVehicleSelection(): void {
    this.selectedVehicle.set(null);
    this.availableVehicles.set([]);
  }

  // ==================== STEP 3: MÜSAİT ARAÇLAR ====================
  loadAvailableVehicles(): void {
    if (!this.isStep2Valid()) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Uyarı',
        detail: 'Önce tarih ve şube seçin.',
      });
      return;
    }

    this.isLoadingVehicles.set(true);
    // this.invalidateVehicleSelection();

    const start = this.plannedStartDate()!;
    const end = this.plannedEndDate()!;

    this.vehicleService
      .getAvailable({
        pageNumber: 1,
        pageSize: 100,
        startDate: start.toISOString(),
        endDate: end.toISOString(),
        isAvailable: true,
      })
      .subscribe({
        next: (res) => {
          if (res.isSuccessful && res.data) {
            this.availableVehicles.set(res.data.items);
          }
        },
        complete: () => this.isLoadingVehicles.set(false),
        error: () => {
          this.isLoadingVehicles.set(false);
          this.messageService.add({
            severity: 'error',
            summary: 'Hata',
            detail: 'Araçlar yüklenemedi.',
          });
        },
      });
  }

  onVehicleSelected(v: Vehicle): void {
    this.selectedVehicle.set(v);
    this.saveDraft();
  }

  // ==================== STEP 4: EXTRALAR ====================
  loadExtras(): void {
    this.isLoadingExtras.set(true);
    this.extraService.getAll().subscribe({
      next: (res) => {
        if (res.isSuccessful && res.data) {
          const active = res.data.filter((e) => e.isActive);
          this.allExtras.set(active.sort((a, b) => a.displayOrder - b.displayOrder));
        }
      },
      complete: () => this.isLoadingExtras.set(false),
    });
  }

  isExtraSelected(extraId: string): boolean {
    return this.selectedExtras().some((se) => se.extra.id === extraId);
  }

  toggleExtra(extra: ExtraSummary): void {
    const current = this.selectedExtras();
    const exists = current.some((se) => se.extra.id === extra.id);

    if (exists) {
      this.selectedExtras.set(current.filter((se) => se.extra.id !== extra.id));
    } else {
      const quantity = 1;
      this.selectedExtras.set([
        ...current,
        {
          extra,
          quantity,
          totalPrice: this.calculateExtraTotal(extra, quantity),
        },
      ]);
    }
    this.saveDraft(); // Her durumda kaydet
  }

  private calculateExtraTotal(extra: ExtraSummary, quantity: number): number {
    const days = this.plannedDays();
    const unitPrice = extra.priceType === 'Daily' ? extra.price * Math.max(days, 1) : extra.price;
    return Math.round(unitPrice * quantity * 100) / 100;
  }

  updateExtraQuantity(extraId: string, quantity: number): void {
    const q = quantity && quantity >= 1 ? quantity : 1;
    this.selectedExtras.set(
      this.selectedExtras().map((se) =>
        se.extra.id === extraId ? { ...se, quantity: q } : se
      )
    );
    this.saveDraft();
  }

  getExtraQuantity(extraId: string | number): number {
    return this.selectedExtras().find((se) => se.extra.id === extraId)?.quantity ?? 1;
  }

  calculateExtraPrice(item: Pick<SelectedExtra, 'extra' | 'quantity'>): number {
    const d = this.plannedDays();
    const unit = item.extra.priceType === 'Daily' ? item.extra.price * d : item.extra.price;
    return Math.round(unit * item.quantity * 100) / 100;
  }

  // ==================== STEP 5: SUBMIT ====================
  submit(): void {
    if (!this.isAllValid()) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Uyarı',
        detail: 'Lütfen tüm adımları tamamlayın.',
      });
      return;
    }

    const request: CreateReservationRequest = {
      customerId: this.selectedCustomer()!.id,
      vehicleId: this.selectedVehicle()!.id,
      pickupBranchId: this.selectedPickupBranch()!.id!,
      returnBranchId: this.selectedReturnBranch()?.id ?? null,
      plannedStartDate: this.plannedStartDate()!.toISOString(),
      plannedEndDate: this.plannedEndDate()!.toISOString(),
      notes: this.notes().trim() || null,
      extras: this.selectedExtras().map((se) => ({
        extraId: se.extra.id,
        quantity: se.quantity,
      })),
    };

    this.isSubmitting.set(true);
    this.reservationService.create(request).subscribe({
      next: (res) => {
        if (res.isSuccessful) {
          this.messageService.add({
            severity: 'success',
            summary: 'Başarılı',
            detail: 'Rezervasyon oluşturuldu.',
          });
          // BAŞARILI OLUNCA HAFIZAYI TEMİZLE
          localStorage.removeItem('reservation_current_step');
          localStorage.removeItem('reservation_draft');

          this.router.navigate(['/admin/reservations/detail', res.data]);
        }
      },
      error: (err) => {
        this.isSubmitting.set(false);
        this.messageService.add({
          severity: 'error',
          summary: 'Hata',
          detail: err?.error?.errorMessages?.[0] || 'Rezervasyon oluşturulamadı.',
        });
      },
      complete: () => this.isSubmitting.set(false),
    });
  }

  getImageUrl(imageUrl: string | null | undefined): string {
    if (!imageUrl) return 'assets/no-image.png';
    if (imageUrl.startsWith('http://') || imageUrl.startsWith('https://')) return imageUrl;

    const baseUrl = environment.apiUrl.replace(/\/api\/?$/, '');
    const path = imageUrl.startsWith('/') ? imageUrl : `/${imageUrl}`;

    return `${baseUrl}${path}`;
  }

  cancel(): void {
    this.router.navigate(['/admin/reservations']);
    // İPTAL EDİLİNCE HAFIZAYI TEMİZLE
    localStorage.removeItem('reservation_current_step');
    localStorage.removeItem('reservation_draft');
  }
}